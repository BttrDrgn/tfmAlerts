using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfmAlert
{
    public static class Extentions
    {
        public static bool StartsWith(this byte[] source, byte[] prefix)
        {
            if (source == null || prefix == null)
            {
                throw new ArgumentNullException(source == null ? nameof(source) : nameof(prefix));
            }

            if (prefix.Length > source.Length) return false;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (source[i] != prefix[i]) return false;
            }

            return true;
        }

        public static List<byte[]> SplitByPattern(this byte[] data, string pattern)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(pattern)) throw new ArgumentNullException(nameof(pattern));

            List<byte[]> result = new List<byte[]>();
            int currentPosition = 0;
            int patternLength = pattern.Split(' ').Length;

            while (currentPosition < data.Length)
            {
                int matchPosition = data.FindPattern(pattern);

                if (matchPosition == -1)
                {
                    result.Add(data[currentPosition..]);
                    break;
                }

                result.Add(data[currentPosition..matchPosition]);
                currentPosition = matchPosition + patternLength;
            }

            return result;
        }

        public static int FindPattern(this byte[] data, string pattern)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(pattern)) throw new ArgumentNullException(nameof(pattern));

            var (patternBytes, mask) = ParsePattern(pattern);

            for (int i = 0; i <= data.Length - patternBytes.Length; i++)
            {
                bool found = true;

                for (int j = 0; j < patternBytes.Length; j++)
                {
                    if (mask[j] && data[i + j] != patternBytes[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found) return i;
            }

            return -1;
        }

        private static (byte[] patternBytes, bool[] mask) ParsePattern(string pattern)
        {
            string[] tokens = pattern.Split(' ');
            byte[] patternBytes = new byte[tokens.Length];
            bool[] mask = new bool[tokens.Length];

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == "??")
                {
                    mask[i] = false;
                    patternBytes[i] = 0x00;
                }
                else if (tokens[i].StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    mask[i] = true;
                    patternBytes[i] = Convert.ToByte(tokens[i], 16);
                }
                else
                {
                    throw new ArgumentException($"Invalid pattern token: {tokens[i]}");
                }
            }

            return (patternBytes, mask);
        }
    }
}
