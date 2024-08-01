using System.Collections.Generic;
using Bencodex.Types;

namespace Bencodex.Misc
{
    /// <summary>
    /// Compares two <see cref="IKey"/> values.  The order is according to the Bencodex
    /// specification on dictionary keys.
    /// </summary>
    public sealed class KeyComparer : IComparer<IKey>
    {
        /// <summary>
        /// The singleton instance of <see cref="KeyComparer"/>.
        /// </summary>
        public static readonly KeyComparer Instance = new KeyComparer();

        private KeyComparer()
        {
        }

        /// <inheritdoc cref="IComparer{T}.Compare(T, T)"/>
        public int Compare(IKey? x, IKey? y)
        {
            if (x is Binary xb && y is Binary yb)
            {
                return xb.CompareTo(yb);
            }
            else if (x is Text xt && y is Text yt)
            {
                return xt.CompareTo(yt);
            }

            return (x?.Kind == ValueKind.Text).CompareTo(y?.Kind == ValueKind.Text);
        }
    }
}
