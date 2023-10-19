using System.Collections.Generic;
using System.Collections.Immutable;
using Bencodex.Misc;
using Xunit;

namespace Bencodex.Tests.Misc
{
    public class ByteArrayComparerTest
    {
        [Fact]
        public void CompareImmutableArrays()
        {
            var comparer = default(ByteArrayComparer);
            ComparerTestUtils.TestComparison(
                comparer,
                new List<ImmutableArray<byte>>()
                {
                    ImmutableArray<byte>.Empty,
                    new byte[] { 0x00 }.ToImmutableArray(),
                    new byte[] { 0x00, 0x00 }.ToImmutableArray(),
                    new byte[] { 0x00, 0x80 }.ToImmutableArray(),
                    new byte[] { 0x00, 0xff }.ToImmutableArray(),
                    new byte[] { 0x01 }.ToImmutableArray(),
                    new byte[] { 0x01, 0x01 }.ToImmutableArray(),
                    new byte[] { 0x01, 0x80 }.ToImmutableArray(),
                    new byte[] { 0x01, 0xff }.ToImmutableArray(),
                }
            );
        }
    }
}
