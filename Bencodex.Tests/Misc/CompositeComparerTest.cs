using System;
using System.Collections.Generic;
using System.Globalization;
using Bencodex.Misc;
using Xunit;

namespace Bencodex.Tests.Misc
{
    public class CompositeComparerTest
    {
        [Fact]
        public void TestComparison()
        {
            var comparer = new CompositeComparer<string, string>(
                StringComparer.Create(CultureInfo.InvariantCulture, true),
                StringComparer.Create(CultureInfo.InvariantCulture, false)
            );
            ComparerTestUtils.TestComparison(
                comparer,
                new List<(string, string)>()
                {
                    (string.Empty, string.Empty),
                    (string.Empty, "world"),
                    (string.Empty, "world1"),
                    ("hello", string.Empty),
                    ("hello", "world"),
                    ("hello", "world1"),
                    ("hello1", string.Empty),
                    ("hello1", "world"),
                    ("hello1", "world1"),
                }
            );
        }
    }
}
