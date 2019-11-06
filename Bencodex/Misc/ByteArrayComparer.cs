using System;
using System.Collections.Generic;
using System.Linq;

namespace Bencodex.Misc
{
    /// <summary>Similar to <c cref="StringComparer">StringComparer</c>
    /// but for <c cref="byte">Byte</c> arrays instead of
    /// Unicode <c cref="string">String</c>s.</summary>
    public struct ByteArrayComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            IEnumerable<int> cmps = x.Zip(y, (a, b) => a.CompareTo(b));
            try
            {
                return cmps.First(cmp => cmp != 0);
            }
            catch (InvalidOperationException)
            {
                return x.Length.CompareTo(y.Length);
            }
        }
    }
}
