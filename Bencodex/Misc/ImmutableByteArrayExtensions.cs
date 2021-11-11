using System;
using System.Collections.Immutable;
using System.Text;

namespace Bencodex.Misc
{
    /// <summary>
    /// Extension methods on <see cref="ImmutableArray{T}"/> of <see cref="byte"/>s.
    /// </summary>
    public static class ImmutableByteArrayExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="bytes"/> into a string of hexadecimal digits.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The hexadecimal digits.  Alphabets are all lowercase.</returns>
        public static string Hex(this in ImmutableArray<byte> bytes)
        {
            const string hexDigits = "0123456789abcdef";
            var s = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                s.Append(hexDigits[b >> 4]);
                s.Append(hexDigits[b & 0xf]);
            }

            return s.ToString();
        }

        /// <summary>
        /// Parses the given <paramref name="hex"/> string into the bytes.
        /// </summary>
        /// <param name="hex">The hexadecimal digits to convert.</param>
        /// <returns>The converted byte array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
        /// <paramref name="hex"/> string is an odd number.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="hex"/>
        /// string contains non-hexadecimal digits.</exception>
        public static ImmutableArray<byte> ParseHex(string hex)
        {
            if (hex.Length % 2 > 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hex),
                    "A length of a hexadecimal string must be an even number."
                );
            }

            var bytes = ImmutableArray.CreateBuilder<byte>(hex.Length / 2);
            for (var i = 0; i < hex.Length / 2; i++)
            {
                bytes.Add(Convert.ToByte(hex.Substring(i * 2, 2), 16));
            }

            return bytes.MoveToImmutable();
        }
    }
}
