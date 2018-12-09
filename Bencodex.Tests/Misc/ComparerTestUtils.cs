using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bencodex.Tests.Misc
{
    public static class ComparerTestUtils
    {
        public static void TestComparison<T>(
            IComparer<T> comparer,
            IList<T> sortedTestSet
        )
        {
            T[] setA = sortedTestSet.ToArray();
            T[] setB = sortedTestSet.ToArray();
            for (int i = 0; i < setA.Length; i++)
            {
                for (int j = 0; j < setB.Length; j++)
                {
                    int expected = i.CompareTo(j);
                    int actual = comparer.Compare(setA[i], setB[j]);
                    if (expected < 0)
                    {
                        Assert.InRange(actual, int.MinValue, 1);
                    }
                    else if (expected > 0)
                    {
                        Assert.InRange(actual, 1, int.MaxValue);
                    }
                    else
                    {
                        Assert.Equal(expected, actual);
                    }
                }
            }
        }
    }
}
