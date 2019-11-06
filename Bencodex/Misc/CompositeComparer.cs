using System;
using System.Collections.Generic;

namespace Bencodex.Misc
{
    /// <summary>Compose given two comparers into one comparer.</summary>
    /// <typeparam name="TA">An element type of the first comparer.</typeparam>
    /// <typeparam name="TB">An element type of the second comparer.</typeparam>
    public struct CompositeComparer<TA, TB> : IComparer<ValueTuple<TA, TB>>
    {
        public CompositeComparer(IComparer<TA> comparerA, IComparer<TB> comparerB)
        {
            ComparerA =
                comparerA ?? throw new ArgumentNullException(nameof(comparerA));
            ComparerB =
                comparerB ?? throw new ArgumentNullException(nameof(comparerB));
        }

        public IComparer<TA> ComparerA { get; }

        public IComparer<TB> ComparerB { get; }

        public int Compare((TA, TB) x, (TA, TB) y)
        {
            (TA xA, TB xB) = x;
            (TA yA, TB yB) = y;
            int resultA = ComparerA.Compare(xA, yA);
            if (resultA != 0)
            {
                return resultA;
            }

            return ComparerB.Compare(xB, yB);
        }
    }
}
