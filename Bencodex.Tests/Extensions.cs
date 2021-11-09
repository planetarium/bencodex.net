using System;
using System.Diagnostics.Contracts;

namespace Bencodex.Tests
{
    public static class Extensions
    {
        [Pure]
        public static string NoCr(this string value) =>
            value.Replace("\r", string.Empty);

        public static byte[] NextBytes(this Random random, int size)
        {
            var buffer = new byte[size];
            random.NextBytes(buffer);
            return buffer;
        }
    }
}
