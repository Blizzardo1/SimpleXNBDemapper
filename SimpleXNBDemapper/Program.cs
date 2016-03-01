using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXNBDemapper
{
    class Program
    {

        static void pack(string filePath, string outPath)
        {
            try
            {
                using (FileStream ifs = File.OpenRead(filePath))
                {
                    using (BinaryReader br = new BinaryReader(ifs))
                    {
                        byte[] data = br.ReadBytes((int)ifs.Length);
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
            Console.WriteLine("usage xnbdemapper (pack|unpack) <input> <output>");
            Console.WriteLine("pack - packs a .tbin file to .xnb");
            Console.WriteLine("unpacks - unpacks a .xnb map file to .tbin");
        }

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                help();
                return;
            }
            switch (args[0])
            {
                case "pack":
                    {
                        pack(args[1], args[2]);
                    } break;
                case "unpack":
                    {
                        unpack(args[1], args[2]);
                    } break;
                default:
                    {
                        help();
                    } break;
            }
        }
    }
}
