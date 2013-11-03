using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman_ECS
{
    // This is based on http://damieng.com/blog/2006/08/08/Calculating_CRC32_in_C_and_NET,
    // except we can't inherit from HashAlgorithm, since it doesn't exist on the Xbox.
    public class Crc32
    {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;

        private static UInt32[] defaultTable;

        static Crc32()
        {
            defaultTable = new UInt32[256];
            for (int i = 0; i < defaultTable.Length; i++)
            {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                    {
                        entry = (entry >> 1) ^ DefaultPolynomial;
                    }
                    else
                    {
                        entry = entry >> 1;
                    }
                }
                defaultTable[i] = entry;
            }
        }

        public static UInt32 Compute(byte[] buffer)
        {
            return ~CalculateHash(buffer, 0, buffer.Length);
        }

        private static UInt32 CalculateHash(byte[] buffer, int start, int size)
        {
            UInt32 crc = DefaultSeed;
            for (int i = start; i < size; i++)
            {
                unchecked
                {
                    crc = (crc >> 8) ^ defaultTable[buffer[i] ^ crc & 0xff];
                }
            }
            return crc;
        }

        // This is the one to use. It only looks at the lower byte of characters, so it won't differentiate
        // non-ASCII characters from one another. This should be ok, because we're never displaying these strings
        // in the UI. They are only used to identify objects internally.
        public static UInt32 CalculateHashASCII(string theString)
        {
            UInt32 crc = DefaultSeed;
            foreach (char c in theString)
            {
                unchecked
                {
                    crc = (crc >> 8) ^ defaultTable[((byte)(c & 0xff)) ^ crc & 0xff];
                }
            }
            return ~crc; // For some reason we ~ this.
        }

        private byte[] UInt32ToBigEndianBytes(UInt32 x)
        {
            return new byte[] {
			(byte)((x >> 24) & 0xff),
			(byte)((x >> 16) & 0xff),
			(byte)((x >> 8) & 0xff),
			(byte)(x & 0xff)
		};
        }
    }
}
