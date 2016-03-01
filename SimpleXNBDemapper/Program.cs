using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXNBDemapper
{
    static class Helpers
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }
    }

    class Program
    {
        static void WriteString(BinaryWriter bw, String str)
        {
            bw.Write(BitConverter.GetBytes(str.Length));
            bw.Write(Encoding.ASCII.GetBytes(str));
        }

        static string LoadString(BinaryReader br)
        {
            int len = br.ReadInt32();
            return Encoding.ASCII.GetString(br.ReadBytes(len));
        }

        static void SkipString(BinaryWriter bw, BinaryReader br)
        {
            WriteString(bw, LoadString(br));
        }

        static void LoadProperties(BinaryWriter bw, BinaryReader br, int propCount)
        {
            for (int i = 0; i < propCount; ++i)
            {
                SkipString(bw, br);
                //strlen = BitConverter.ToInt32(data.SubArray(index, 4), 0);

                byte propertyType = br.ReadByte();
                bw.Write(propertyType);

                switch (propertyType)
                {
                    case PROPERTY_BOOL:
                        {
                            bw.Write(br.ReadByte());
                        }
                        break;
                    case PROPERTY_INT:
                        {
                            bw.Write(br.ReadBytes(4));
                        }
                        break;
                    case PROPERTY_FLOAT:
                        {
                            bw.Write(br.ReadBytes(4));
                        }
                        break;
                    case PROPERTY_STRING:
                        {
                            SkipString(bw, br);
                        }
                        break;
                }
            }
        }

        static void pack(string filePath, string outPath)
        {
            try
            {
                using (FileStream ifs = File.OpenRead(filePath))
                {
                    using (BinaryReader br = new BinaryReader(ifs))
                    {
                        byte[] data = br.ReadBytes((int)ifs.Length);
                        MemoryStream ms = new MemoryStream(data);
                        data = processPngExtensions(ms, false);
                        byte[] header = { 0x58, 0x4E, 0x42, 0x77, 0x05, 0x01, 0x4C, 0x10, 0x00, 0x00, 0x01, 0x20, 0x78, 0x54, 0x69, 0x6C, 0x65, 0x2E, 0x50, 0x69, 0x70, 0x65, 0x6C, 0x69, 0x6E, 0x65, 0x2E, 0x54, 0x69, 0x64, 0x65, 0x52, 0x65, 0x61, 0x64, 0x65, 0x72, 0x2C, 0x20, 0x78, 0x54, 0x69, 0x6C, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
                        byte[] sizeHeader = BitConverter.GetBytes(data.Length);
                        try
                        {
                            FileStream ofs = File.OpenWrite(outPath);
                            BinaryWriter bw = new BinaryWriter(ofs);
                            bw.Write(header);
                            bw.Write(sizeHeader);
                            bw.Write(data);
                            bw.Close();
                            ofs.Close();
                            Console.WriteLine("File packed.");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error. Can't open/write output file: " + ex.Message);
                        }
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine("Error. Can't open/read input file: " + ex.Message);
            }
        }

        const byte PROPERTY_BOOL = 0;
        const byte PROPERTY_INT = 1;
        const byte PROPERTY_FLOAT = 2;
        const byte PROPERTY_STRING = 3;
        static byte[] processPngExtensions(Stream dataStream, bool add)
        {
            MemoryStream outStream = new MemoryStream();
            BinaryWriter ow = new BinaryWriter(outStream);
            BinaryReader or = new BinaryReader(dataStream);

            ow.Write(or.ReadBytes(6)); // skip header
            byte[] buffer;
            SkipString(ow, or);
            SkipString(ow, or);

            buffer = or.ReadBytes(4);
            int propCount = BitConverter.ToInt32(buffer, 0);
            ow.Write(buffer);

            LoadProperties(ow, or, propCount);

            buffer = or.ReadBytes(4);
            int sheetCount = BitConverter.ToInt32(buffer, 0);
            ow.Write(buffer);
            for (int i = 0; i < sheetCount; ++i)
            {
                SkipString(ow, or);
                SkipString(ow, or);

                string str = LoadString(or);
                if (add)
                {
                    str += ".png";
                } else
                {
                    str = Path.GetFileNameWithoutExtension(str);
                }
                WriteString(ow, str);

                ow.Write(or.ReadBytes(4 * 8));

                buffer = or.ReadBytes(4);
                propCount = BitConverter.ToInt32(buffer, 0);
                ow.Write(buffer);
                LoadProperties(ow, or, propCount);
            }

            ow.Write(or.ReadAllBytes());
            return outStream.GetBuffer();
        }

        static void unpack(string filePath, string outPath)
        {
            try
            {
                using (FileStream ifs = File.OpenRead(filePath))
                {
                    using (BinaryReader br = new BinaryReader(ifs))
                    {
                        byte[] tBinBytes = { 0x74, 0x42, 0x49, 0x4E };
                        byte[] data = br.ReadBytes((int)ifs.Length);
                        int index = 0;
                        bool found = false;
                        while (index < data.Length - 4)
                        {
                            if (Enumerable.SequenceEqual(new byte[] { data[index], data[index + 1], data[index + 2], data[index + 3] }, tBinBytes))
                            {
                                found = true;
                                break;
                            }
                            index++;
                        }

                        if (!found)
                        {
                            Console.WriteLine("Error: Can't find tBin header in file. Make sure it is an uncompressed XNB file of a Stardew Valley map");
                            return;
                        }
                        byte[] dataUnpacked = new byte[data.Length - index];
                        Array.Copy(data, index, dataUnpacked, 0, dataUnpacked.Length);

                        MemoryStream ms = new MemoryStream(dataUnpacked);
                        dataUnpacked = processPngExtensions(ms, true);
                        try
                        {
                            FileStream ofs = File.OpenWrite(outPath);
                            BinaryWriter bw = new BinaryWriter(ofs);
                            bw.Write(dataUnpacked);
                            bw.Close();
                            ofs.Close();
                            Console.WriteLine("File unpacked.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error. Can't open/write output file: " + ex.Message);
                        }
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine("Error. Can't open/read input file: " + ex.Message);
            }
        }

        static void help()
        {
            Console.WriteLine("usage xnbdemapper (pack|unpack) <input> [<output>]");
            Console.WriteLine("pack - packs a .tbin file to .xnb");
            Console.WriteLine("unpacks - unpacks a .xnb map file to .tbin");
        }

        static void Main(string[] args)
        {
            String outPath;
            String inPath;
            String command;
            if (args.Length < 1)
            {
                help();
                return;
            }
            inPath = args[0];
            if (args.Length == 1)
            {
                switch (Path.GetExtension(inPath))
                {
                    case ".tbin":
                        {
                            command = "pack";
                        }
                        break;
                    case ".xnb":
                        {
                            command = "unpack";
                        }
                        break;
                    default:
                        {
                            Console.WriteLine("Error: Unrecognized file extension.");
                            Console.ReadKey();
                            return;
                        }
                }
                outPath = Path.GetFileNameWithoutExtension(inPath);
            } else if (args.Length == 2)
            {
                command = args[0];
                inPath = args[1];
                outPath = Path.GetFileNameWithoutExtension(args[1]);
            } else
            {
                command = args[0];
                inPath = args[1];
                outPath = args[2];
            }
            switch (command)
            {
                case "pack":
                    {
                        pack(inPath, (args.Length <= 2 ? outPath + ".xnb" : outPath));
                    } break;
                case "unpack":
                    {
                        unpack(inPath, (args.Length <= 2 ? outPath + ".tbin" : outPath));
                    } break;
                default:
                    {
                        help();
                    } break;
            }
        }
    }
}
