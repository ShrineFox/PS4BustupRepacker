using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS4BustupRepacker
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> gnfFiles = new List<string>();
            List<string> gnfBins = new List<string>();
            if (args.Count() <= 0)
            {
                Console.WriteLine("  PS4BustupRepacker by ShrineFox" +
                    "\n\n  1. Drag a folder containing PS4 Bustup BINs onto this EXE to Unpack" +
                    "\n  OR" +
                    "\n  2. Drag an unpacked folder containing GNF files onto this EXE to Repack");
                Console.ReadKey();
                return;
            }

            foreach (string gnf in Directory.GetFiles(args[0]))
            {
                if (Path.GetExtension(gnf).ToLower() == ".gnf")
                    gnfFiles.Add(gnf);
                else if (Path.GetExtension(gnf).ToLower() == ".bin")
                {
                    gnfBins.Add(gnf);
                }
            }

            if (gnfFiles.Count > 0)
            {
                string binPath = Path.GetDirectoryName(args[0]);
                string binName = Path.GetFileName(args[0]);

                if (File.Exists($"{binPath}//{binName}_repacked.bin"))
                {
                    File.Delete($"{binPath}//{binName}_repacked.bin");
                }

                using (FileStream stream = new FileStream($"{binPath}//{binName}_repacked.bin", FileMode.CreateNew))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(ConvertInt32(gnfFiles.Count()));
                        foreach (string gnf in gnfFiles)
                        {
                            byte[] gnfBytes = File.ReadAllBytes(gnf);

                            PadName32(writer, $"{Path.GetFileName(gnf)}");
                            writer.Write(ConvertInt32(gnfBytes.Count()));
                            writer.Write(gnfBytes);
                        }
                    }
                }
            }
            else if (gnfBins.Count > 0)
            {
                foreach (string bin in gnfBins)
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(bin, FileMode.Open)))
                    {
                        int gnfCount = ReadInt32(reader);
                        string binName = Path.GetFileNameWithoutExtension(bin);
                        string dirName = $"{Path.GetDirectoryName(bin)}\\{binName}";
                        if (!Directory.Exists(dirName))
                            Directory.CreateDirectory(dirName);

                        for (int i = gnfCount; i > 0; i--)
                        {
                            string gnfName = ReadName(reader);
                            int gnfSize = ReadInt32(reader);

                            byte[] gnfFile = reader.ReadBytes(gnfSize);
                            using (FileStream stream = new FileStream($"{dirName}\\{gnfName}", FileMode.Create))
                            {
                                using (BinaryWriter writer = new BinaryWriter(stream))
                                {
                                    writer.Write(gnfFile);
                                }
                            }
                        }
                    }
                }
                
            }


        }

        public static int ReadInt32(BinaryReader reader) //Read 32-bit value with endian shifted
        {
            return reader.ReadByte() << 24 | reader.ReadByte() << 16 | reader.ReadByte() << 8 | reader.ReadByte();
        }

        public static string ReadName(BinaryReader reader) //Read DDS filename
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0');
        }

        public static int GetFileSize(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            int length = (int)file.Length;
            return length;
        }

        public static void PadName32(BinaryWriter writer, string gnfFile)
        {
            writer.Write(Encoding.ASCII.GetBytes(gnfFile));
            int padAmount = 32 - gnfFile.Length;
            writer.Write(new byte[padAmount]);
        }

        public static int ConvertInt32(int word)
        {
            byte[] bytes = BitConverter.GetBytes(word);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            int result = BitConverter.ToInt32(bytes, 0);
            return result;
        }
    }
}
