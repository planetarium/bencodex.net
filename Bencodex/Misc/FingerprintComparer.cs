using System.Collections.Generic;
using Bencodex.Types;

namespace Bencodex.Misc
{
    /// <summary>
    /// Compares two <see cref="Fingerprint"/> values.  There is no meaning for the order,
    /// but it just purposes to make the order deterministic.
    /// </summary>
    public sealed class FingerprintComparer : IComparer<Fingerprint>
    {
        private static readonly ByteArrayComparer _digestComparer = default;

        /// <inheritdoc cref="IComparer{T}.Compare(T, T)"/>
        public int Compare(Fingerprint x, Fingerprint y)
        {
            if (x.Kind != y.Kind)
            {
                return x.Kind < y.Kind ? -1 : 1;
            }

            int encLenCmp = x.EncodingLength.CompareTo(y.EncodingLength);
            return encLenCmp != 0 ? encLenCmp : _digestComparer.Compare(x.Digest, y.Digest);
        }
    }
}
