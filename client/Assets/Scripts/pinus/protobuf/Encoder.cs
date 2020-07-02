using System;
using System.Collections;
using System.Collections.Generic;

namespace Pomelo.Protobuf
{
    public class Encoder
    {

        //Encode the UInt32.
        public static byte[] encodeUInt32(string n)
        {
            return encodeUInt32(Convert.ToUInt32(n));
        }

        public static byte[] encodeUInt32(uint n)
        {
            List<byte> byteList = new List<byte>();
            do
            {
                uint tmp = n % 128;
                uint next = n >> 7;
                if (next != 0)
                {
                    tmp = tmp + 128;
                }
                byteList.Add(Convert.ToByte(tmp));
                n = next;
            } while (n != 0);

            return byteList.ToArray();
        }

        public static byte[] encodeSInt32(string n)
        {
            return encodeSInt32(Convert.ToInt32(n));
        }

        public static byte[] encodeSInt32(int n)
        {
            //uint num = (uint)(n < 0 ? (Math.Abs(n) * 2 - 1) : n * 2);
            var bArr = BitConverter.GetBytes((n << 1) ^ (n >> 31));
            uint num = BitConverter.ToUInt32(bArr, 0);
            return encodeUInt32(num);
        }

        public static byte[] encodeSInt64(string n)
        {
            return encodeSInt64(Convert.ToInt64(n));
        }

        public static byte[] encodeUInt64(string n)
        {
            return encodeUInt64(Convert.ToUInt64(n));
        }

        public static byte[] encodeUInt64(ulong n)
        {
            List<byte> byteList = new List<byte>();
            do
            {
                ulong tmp = n % 128;
                ulong next = n >> 7;
                if (next != 0)
                {
                    tmp = tmp + 128;
                }
                byteList.Add(Convert.ToByte(tmp));
                n = next;
            } while (n != 0);

            return byteList.ToArray();
        }       

        public static byte[] encodeSInt64(long n)
        {
            //ulong num = (ulong)(n < 0 ? (Math.Abs(n) * 2 - 1) : n * 2);
            var bArr = BitConverter.GetBytes((n << 1) ^ (n >> 63));
            ulong num = BitConverter.ToUInt64(bArr, 0);
            return encodeUInt64(num);
        }

        public static byte[] encodeFloat(float n)
        {
            byte[] bytes = BitConverter.GetBytes(n);
            if (!BitConverter.IsLittleEndian)
            {
                Util.Reverse(bytes);
            }
            return bytes;
        }

        //Get the byte length of message.
        public static int byteLength(string msg)
        {
            return System.Text.Encoding.UTF8.GetBytes(msg).Length;
        }

        public static byte[] encodeBool(object n)
        {
            return new byte[1] { Convert.ToByte(n) };
        }
    }
}