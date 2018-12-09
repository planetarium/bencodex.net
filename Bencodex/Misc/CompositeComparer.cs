using System;
using System.Collections.Generic;

namespace Bencodex.Misc
{
    /// <summary>Compose given two comparers into one comparer.</summary>
    /// <typeparam name="A">An element type of the first comparer.</typeparam>
    /// <typeparam name="B">An element type of the second comparer.</typeparam>
    public struct CompositeComparer<A, B> : IComparer<ValueTuple<A, B>>
    {
        public IComparer<A> ComparerA { get; }
        public IComparer<B> ComparerB { get; }

        public CompositeComparer(IComparer<A> comparerA, IComparer<B> comparerB)
        {
            ComparerA =
                comparerA ?? throw new ArgumentNullException(nameof(comparerA));
            ComparerB =
                comparerB ?? throw new ArgumentNullException(nameof(comparerB));
        }

        public int Compare((A, B) x, (A, B) y)
        {
            (A xA, B xB) = x;
            (A yA, B yB) = y;
            int resultA = ComparerA.Compare(xA, yA);
            if (resultA != 0) return resultA;
            return ComparerB.Compare(xB, yB);
        }
    }
}
