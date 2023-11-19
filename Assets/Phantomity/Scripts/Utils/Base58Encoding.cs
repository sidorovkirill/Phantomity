using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Phantomity.Utils
{
    public static class Base58Encoding
    {
        private const int CheckSumSizeInBytes = 4;
        private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static string Encode(byte[] data)
        {
            var intData = GetDataInt(data);

            var result = "";
            while (intData > 0)
            {
                var remainder = (int)(intData % 58);
                intData /= 58;
                result = Digits[remainder] + result;
            }

            for (var i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }
            return result;
        }

        public static byte[] Decode(string s)
        {
            BigInteger intData = 0;
            for (var i = 0; i < s.Length; i++)
            {
                var digit = Digits.IndexOf(s[i]);
                if (digit < 0)
                    throw new FormatException($"Invalid Base58 character {s[i]} at position {i}");
                intData = intData * 58 + digit;
            }
            
            var leadingZeroCount = s.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            var bytesWithoutLeadingZeros = intData.ToByteArray()
                .Reverse() // to big endian
                .SkipWhile(b => b == 0); //strip sign byte
            var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();
            return result;
        }

        private static BigInteger GetDataInt(byte[] data)
        {
            BigInteger intData = 0;
            foreach (var digit in data)
            {
                intData = intData * 256 + digit;
            }
            return intData;
        }

        private static byte[] GetCheckSum(byte[] data)
        {
            var sha256 = new SHA256Managed();
            var hash1 = sha256.ComputeHash(data);
            var hash2 = sha256.ComputeHash(hash1);

            var result = new byte[CheckSumSizeInBytes];
            Buffer.BlockCopy(hash2, 0, result, 0, result.Length);

            return result;
        }
    }
}