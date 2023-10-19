using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bencodex.Misc
{
    /// <summary>
    /// Similar to <see cref="StringComparer"/> but for <see cref="byte"/>s instead of Unicode
    /// <see cref="string"/>s.
    /// </summary>
    public struct ByteArrayComparer : IComparer<ImmutableArray<byte>>
    {
        private static readonly ByteArrayComparer<ImmutableArray<byte>> _immutableArrayComparer =
            new ByteArrayComparer<ImmutableArray<byte>>();

        public int Compare(ImmutableArray<byte> x, ImmutableArray<byte> y) =>
            _immutableArrayComparer.Compare(x, y);
    }

    internal class ByteArrayComparer<T> : IComparer<T>
        where T : IReadOnlyList<byte>
    {
        public int Compare(T x, T y)
        {
            int shortestLength = Math.Min(x.Count, y.Count);
            for (int i = 0; i < shortestLength; i++)
            {
                int c = x[i].CompareTo(y[i]);
                if (c != 0)
                {
                    return c;
                }
            }

            return x.Count.CompareTo(y.Count);
        }
    }
}
