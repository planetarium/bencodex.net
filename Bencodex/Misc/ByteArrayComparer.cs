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
            int shortestLength = Math.Min(x.Length, y.Length);
            for (int i = 0; i < shortestLength; i++)
            {
                int c = x[i].CompareTo(y[i]);
                if (c != 0)
                {
                    return c;
                }
            }

            return x.Length.CompareTo(y.Length);
        }
    }
}
