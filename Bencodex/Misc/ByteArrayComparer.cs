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
    public struct ByteArrayComparer
        : IComparer<byte[]>, IComparer<ImmutableArray<byte>>, IComparer<IReadOnlyList<byte>>
    {
        private static readonly ByteArrayComparer<byte[]> _mutableArrayComparer =
            new ByteArrayComparer<byte[]>();

        private static readonly ByteArrayComparer<ImmutableArray<byte>> _immutableArrayComparer =
            new ByteArrayComparer<ImmutableArray<byte>>();

        private static readonly ByteArrayComparer<IReadOnlyList<byte>> _readOnlyListComparer =
            new ByteArrayComparer<IReadOnlyList<byte>>();

        public int Compare(byte[] x, byte[] y) =>
            _mutableArrayComparer.Compare(x, y);

        public int Compare(ImmutableArray<byte> x, ImmutableArray<byte> y) =>
            _immutableArrayComparer.Compare(x, y);

        public int Compare(IReadOnlyList<byte> x, IReadOnlyList<byte> y) =>
            _readOnlyListComparer.Compare(x, y);
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
