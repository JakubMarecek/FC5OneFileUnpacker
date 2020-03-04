/* 
 * FC5 One File Unpacker
 * Copyright (C) 2020  Jakub Mareček (info@jakubmarecek.cz)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with FC5 One File Unpacker.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FC5OneFileUnpacker
{
    class Program
    {
        public static string version = "20200105-2300";

        private static readonly UInt64[] crc_64_table = {
                0x0000000000000000ul, 0x01B0000000000000ul, 0x0360000000000000ul, 0x02D0000000000000ul,
                0x06C0000000000000ul, 0x0770000000000000ul, 0x05A0000000000000ul, 0x0410000000000000ul,
                0x0D80000000000000ul, 0x0C30000000000000ul, 0x0EE0000000000000ul, 0x0F50000000000000ul,
                0x0B40000000000000ul, 0x0AF0000000000000ul, 0x0820000000000000ul, 0x0990000000000000ul,
                0x1B00000000000000ul, 0x1AB0000000000000ul, 0x1860000000000000ul, 0x19D0000000000000ul,
                0x1DC0000000000000ul, 0x1C70000000000000ul, 0x1EA0000000000000ul, 0x1F10000000000000ul,
                0x1680000000000000ul, 0x1730000000000000ul, 0x15E0000000000000ul, 0x1450000000000000ul,
                0x1040000000000000ul, 0x11F0000000000000ul, 0x1320000000000000ul, 0x1290000000000000ul,
                0x3600000000000000ul, 0x37B0000000000000ul, 0x3560000000000000ul, 0x34D0000000000000ul,
                0x30C0000000000000ul, 0x3170000000000000ul, 0x33A0000000000000ul, 0x3210000000000000ul,
                0x3B80000000000000ul, 0x3A30000000000000ul, 0x38E0000000000000ul, 0x3950000000000000ul,
                0x3D40000000000000ul, 0x3CF0000000000000ul, 0x3E20000000000000ul, 0x3F90000000000000ul,
                0x2D00000000000000ul, 0x2CB0000000000000ul, 0x2E60000000000000ul, 0x2FD0000000000000ul,
                0x2BC0000000000000ul, 0x2A70000000000000ul, 0x28A0000000000000ul, 0x2910000000000000ul,
                0x2080000000000000ul, 0x2130000000000000ul, 0x23E0000000000000ul, 0x2250000000000000ul,
                0x2640000000000000ul, 0x27F0000000000000ul, 0x2520000000000000ul, 0x2490000000000000ul,
                0x6C00000000000000ul, 0x6DB0000000000000ul, 0x6F60000000000000ul, 0x6ED0000000000000ul,
                0x6AC0000000000000ul, 0x6B70000000000000ul, 0x69A0000000000000ul, 0x6810000000000000ul,
                0x6180000000000000ul, 0x6030000000000000ul, 0x62E0000000000000ul, 0x6350000000000000ul,
                0x6740000000000000ul, 0x66F0000000000000ul, 0x6420000000000000ul, 0x6590000000000000ul,
                0x7700000000000000ul, 0x76B0000000000000ul, 0x7460000000000000ul, 0x75D0000000000000ul,
                0x71C0000000000000ul, 0x7070000000000000ul, 0x72A0000000000000ul, 0x7310000000000000ul,
                0x7A80000000000000ul, 0x7B30000000000000ul, 0x79E0000000000000ul, 0x7850000000000000ul,
                0x7C40000000000000ul, 0x7DF0000000000000ul, 0x7F20000000000000ul, 0x7E90000000000000ul,
                0x5A00000000000000ul, 0x5BB0000000000000ul, 0x5960000000000000ul, 0x58D0000000000000ul,
                0x5CC0000000000000ul, 0x5D70000000000000ul, 0x5FA0000000000000ul, 0x5E10000000000000ul,
                0x5780000000000000ul, 0x5630000000000000ul, 0x54E0000000000000ul, 0x5550000000000000ul,
                0x5140000000000000ul, 0x50F0000000000000ul, 0x5220000000000000ul, 0x5390000000000000ul,
                0x4100000000000000ul, 0x40B0000000000000ul, 0x4260000000000000ul, 0x43D0000000000000ul,
                0x47C0000000000000ul, 0x4670000000000000ul, 0x44A0000000000000ul, 0x4510000000000000ul,
                0x4C80000000000000ul, 0x4D30000000000000ul, 0x4FE0000000000000ul, 0x4E50000000000000ul,
                0x4A40000000000000ul, 0x4BF0000000000000ul, 0x4920000000000000ul, 0x4890000000000000ul,
                0xD800000000000000ul, 0xD9B0000000000000ul, 0xDB60000000000000ul, 0xDAD0000000000000ul,
                0xDEC0000000000000ul, 0xDF70000000000000ul, 0xDDA0000000000000ul, 0xDC10000000000000ul,
                0xD580000000000000ul, 0xD430000000000000ul, 0xD6E0000000000000ul, 0xD750000000000000ul,
                0xD340000000000000ul, 0xD2F0000000000000ul, 0xD020000000000000ul, 0xD190000000000000ul,
                0xC300000000000000ul, 0xC2B0000000000000ul, 0xC060000000000000ul, 0xC1D0000000000000ul,
                0xC5C0000000000000ul, 0xC470000000000000ul, 0xC6A0000000000000ul, 0xC710000000000000ul,
                0xCE80000000000000ul, 0xCF30000000000000ul, 0xCDE0000000000000ul, 0xCC50000000000000ul,
                0xC840000000000000ul, 0xC9F0000000000000ul, 0xCB20000000000000ul, 0xCA90000000000000ul,
                0xEE00000000000000ul, 0xEFB0000000000000ul, 0xED60000000000000ul, 0xECD0000000000000ul,
                0xE8C0000000000000ul, 0xE970000000000000ul, 0xEBA0000000000000ul, 0xEA10000000000000ul,
                0xE380000000000000ul, 0xE230000000000000ul, 0xE0E0000000000000ul, 0xE150000000000000ul,
                0xE540000000000000ul, 0xE4F0000000000000ul, 0xE620000000000000ul, 0xE790000000000000ul,
                0xF500000000000000ul, 0xF4B0000000000000ul, 0xF660000000000000ul, 0xF7D0000000000000ul,
                0xF3C0000000000000ul, 0xF270000000000000ul, 0xF0A0000000000000ul, 0xF110000000000000ul,
                0xF880000000000000ul, 0xF930000000000000ul, 0xFBE0000000000000ul, 0xFA50000000000000ul,
                0xFE40000000000000ul, 0xFFF0000000000000ul, 0xFD20000000000000ul, 0xFC90000000000000ul,
                0xB400000000000000ul, 0xB5B0000000000000ul, 0xB760000000000000ul, 0xB6D0000000000000ul,
                0xB2C0000000000000ul, 0xB370000000000000ul, 0xB1A0000000000000ul, 0xB010000000000000ul,
                0xB980000000000000ul, 0xB830000000000000ul, 0xBAE0000000000000ul, 0xBB50000000000000ul,
                0xBF40000000000000ul, 0xBEF0000000000000ul, 0xBC20000000000000ul, 0xBD90000000000000ul,
                0xAF00000000000000ul, 0xAEB0000000000000ul, 0xAC60000000000000ul, 0xADD0000000000000ul,
                0xA9C0000000000000ul, 0xA870000000000000ul, 0xAAA0000000000000ul, 0xAB10000000000000ul,
                0xA280000000000000ul, 0xA330000000000000ul, 0xA1E0000000000000ul, 0xA050000000000000ul,
                0xA440000000000000ul, 0xA5F0000000000000ul, 0xA720000000000000ul, 0xA690000000000000ul,
                0x8200000000000000ul, 0x83B0000000000000ul, 0x8160000000000000ul, 0x80D0000000000000ul,
                0x84C0000000000000ul, 0x8570000000000000ul, 0x87A0000000000000ul, 0x8610000000000000ul,
                0x8F80000000000000ul, 0x8E30000000000000ul, 0x8CE0000000000000ul, 0x8D50000000000000ul,
                0x8940000000000000ul, 0x88F0000000000000ul, 0x8A20000000000000ul, 0x8B90000000000000ul,
                0x9900000000000000ul, 0x98B0000000000000ul, 0x9A60000000000000ul, 0x9BD0000000000000ul,
                0x9FC0000000000000ul, 0x9E70000000000000ul, 0x9CA0000000000000ul, 0x9D10000000000000ul,
                0x9480000000000000ul, 0x9530000000000000ul, 0x97E0000000000000ul, 0x9650000000000000ul,
                0x9240000000000000ul, 0x93F0000000000000ul, 0x9120000000000000ul, 0x9090000000000000ul};

        public static UInt64 gethash64(string lpString)
        {
            UInt64 dwHash = 0;
            byte[] m_String = Encoding.ASCII.GetBytes(lpString.ToLower());
            for (int i = 0; i < lpString.Length; i++)
            {
                dwHash = crc_64_table[(byte)(dwHash ^ m_String[i])] ^ (dwHash >> 8);
            }

            return dwHash;
        }

        static void Main(string[] args)
        {
            Console.Title = "FC5OneFileUnpacker";

            if (args.Length < 1)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("****************************************************************");
                Console.WriteLine("**** FC5OneFileUnpacker v" + version);
                Console.WriteLine("****   Author: ArmanIII");
                Console.WriteLine("****   Special thanks to: Ekey (FC5 Unpacker)");
                Console.WriteLine("****************************************************************");
                Console.ResetColor();
                Console.WriteLine("");
                Console.WriteLine("Extracts only defined file.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FC5OneFileUnpacker <fatFile> <outDir> <fileName>");
                Console.WriteLine("    fatFile  - *.fat file");
                Console.WriteLine("    outDir   - output directory");
                Console.WriteLine("    fileName - name of the file which will be extracted");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FC5OneFileUnpacker D:\\patch.fat D:\\unpacked patch.dbt.fcb");
                Console.WriteLine("");
                Console.ResetColor();
                return;
            }

            if (args.Length < 3)
            {
                Console.WriteLine("Bad launch arguments.");
                return;
            }

            string file = args[0];
            string outDir = args[1];
            string extrFile = args[2];

            Console.Title = "FC5OneFileUnpacker - " + file;

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            byte[] bt = GetFileFromDat(file, extrFile);

            if (bt.Length == 0)
            {
                Console.WriteLine("File " + extrFile + " was not found in " + file);
                return;
            }

            File.WriteAllBytes(outDir + "\\" + Path.GetFileName(extrFile), bt);

            Console.WriteLine("FIN");
            return;
        }

        static byte[] GetFileFromDat(string fatFile, string fileName, string datFile = "")
        {
            SortedDictionary<ulong, FatEntry> Entries = GetFatEntries(fatFile);

            if (Entries == null)
                return new byte[] { };

            ulong hash;
            if (fileName.Contains("__unknown"))
            {
                var partName = Path.GetFileNameWithoutExtension(fileName);

                if (partName.Length > 16)
                {
                    partName = partName.Substring(0, 16);
                }

                hash = ulong.Parse(partName, NumberStyles.AllowHexSpecifier);
            }
            else
            {
                hash = gethash64(fileName);
            }

            byte[] result = new byte[] { };
            if (Entries.TryGetValue(hash, out FatEntry value))
            {
                using (FileStream fileStream = File.Open(datFile == "" ? fatFile.Replace(".fat", ".dat") : datFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.Seek(value.Offset, SeekOrigin.Begin);
                    result = new byte[value.UncompressedSize];

                    switch (value.CompressionScheme)
                    {
                        case CompressionScheme.None:
                            new BinaryReader(fileStream).Read(result, 0, (int)value.UncompressedSize);
                            Console.WriteLine("Getting file completed.");
                            fileStream.Dispose();
                            fileStream.Close();
                            return result;
                        case CompressionScheme.Zlib:
                            BinaryReader binaryReader = new BinaryReader(fileStream);
                            byte[] array = new byte[value.CompressedSize];
                            binaryReader.Read(array, 0, (int)value.CompressedSize);
                            new LZ4Sharp.LZ4Decompressor64().Decompress(array, result);

                            Console.WriteLine("Getting file completed.");

                            fileStream.Dispose();
                            fileStream.Close();

                            return result;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return result;
        }

        static SortedDictionary<ulong, FatEntry> GetFatEntries(string fatFile)
        {
            SortedDictionary<ulong, FatEntry> Entries = new SortedDictionary<ulong, FatEntry>();

            FileStream TFATStream = new FileStream(fatFile, FileMode.Open);
            BinaryReader TFATReader = new BinaryReader(TFATStream);

            int dwMagic = TFATReader.ReadInt32();
            int dwVersion = TFATReader.ReadInt32();
            int dwUnknown = TFATReader.ReadInt32();
            int dwZero1 = TFATReader.ReadInt32();
            int dwZero2 = TFATReader.ReadInt32();
            int dwTotalFiles = TFATReader.ReadInt32();

            if (dwMagic != 0x46415432)
            {
                Console.WriteLine("Invalid FAT Index file!");
                TFATReader.Dispose();
                TFATStream.Dispose();
                TFATReader.Close();
                TFATStream.Close();
                return null;
            }

            if (dwVersion != 10)
            {
                Console.WriteLine("Invalid version of FAT Index file!");
                TFATReader.Dispose();
                TFATStream.Dispose();
                TFATReader.Close();
                TFATStream.Close();
                return null;
            }

            for (int i = 0; i < dwTotalFiles; i++)
            {
                ulong dwHash = TFATReader.ReadUInt64();
                uint dwUncompressedSize = TFATReader.ReadUInt32();
                uint dwUnresolvedOffset = TFATReader.ReadUInt32();
                uint dwCompressedSize = TFATReader.ReadUInt32();

                uint dwFlag = dwUncompressedSize & 3;
                ulong dwOffset = dwCompressedSize >> 29 | 8ul * dwUnresolvedOffset;
                dwHash = (dwHash << 32) + (dwHash >> 32);
                dwCompressedSize = (dwCompressedSize & 0x1FFFFFFF);
                dwUncompressedSize = (dwUncompressedSize >> 2);

                var entry = new FatEntry();
                entry.NameHash = dwHash;
                entry.UncompressedSize = dwUncompressedSize;
                entry.Offset = (long)dwOffset;
                entry.CompressedSize = dwCompressedSize;
                entry.CompressionScheme = dwFlag == 0 ? CompressionScheme.None : CompressionScheme.Zlib;

                Entries[entry.NameHash] = entry;
            }

            TFATReader.Dispose();
            TFATStream.Dispose();
            TFATReader.Close();
            TFATStream.Close();

            return Entries;
        }
    }
}
