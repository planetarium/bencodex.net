using System.Collections.Generic;
using Bencodex.Misc;
using Xunit;

namespace Bencodex.Tests.Misc
{
    public class ByteArrayComparerTest
    {
        [Fact]
        public void TestComparison()
        {
            var comparer = new ByteArrayComparer();
            ComparerTestUtils.TestComparison(
                comparer,
                new List<byte[]>()
                {
                    new byte[] {},
                    new byte[] { 0x00 },
                    new byte[] { 0x00, 0x00 },
                    new byte[] { 0x00, 0x80 },
                    new byte[] { 0x00, 0xff },
                    new byte[] { 0x01 },
                    new byte[] { 0x01, 0x01 },
                    new byte[] { 0x01, 0x80 },
                    new byte[] { 0x01, 0xff },
                }
            );
        }
    }
}
