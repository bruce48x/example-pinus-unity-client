using System;

namespace Pomelo.Protobuf
{
    public class Decoder
    {
        /// <summary>
        /// Decodes the UInt32.
        /// </summary>
        public static uint decodeUInt32(int offset, byte[] bytes, out int length)
        {
            uint n = 0;
            length = 0;

            for (int i = offset; i < bytes.Length; i++)
            {
                length++;
                uint m = Convert.ToUInt32(bytes[i]);
                n = n + Convert.ToUInt32((m & 0x7f) * Math.Pow(2, (7 * (i - offset))));
                if (m < 128)
                {
                    break;
                }
            }

            return n;
        }

        public static uint decodeUInt32(byte[] bytes)
        {
            int length;
            return decodeUInt32(0, bytes, out length);
        }

        public static int decodeSInt32(byte[] bytes)
        {
            uint n = decodeUInt32(bytes);
            int flag = ((n % 2) == 1) ? -1 : 1;

            int result = Convert.ToInt32(((n % 2 + n) / 2) * flag);
            return result;
        }

        public static ulong decodeUInt64(byte[] bytes)
        {
            int length;
            return decodeUInt64(0, bytes, out length);
        }

        public static ulong decodeUInt64(int offset, byte[] bytes, out int length)
        {
            ulong n = 0;
            length = 0;

            for (int i = offset; i < bytes.Length; i++)
            {
                length++;
                uint m = Convert.ToUInt32(bytes[i]);
                n = n + Convert.ToUInt64((m & 0x7f) * Math.Pow(2, (7 * (i - offset))));
                if (m < 128)
                {
                    break;
                }
            }

            return n;
        }

        public static long decodeSInt64(byte[] bytes)
        {
            ulong n = decodeUInt64(bytes);
            long flag = ((n % 2) == 1) ? -1 : 1;

            long result = Convert.ToInt64((n % 2 + n) / 2) * flag;
            return result;
        }
    }
}