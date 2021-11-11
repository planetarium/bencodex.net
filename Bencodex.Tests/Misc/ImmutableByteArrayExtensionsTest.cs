using Bencodex.Misc;
using Xunit;
using static System.Collections.Immutable.ImmutableArray;
using static Bencodex.Misc.ImmutableByteArrayExtensions;

namespace Bencodex.Tests.Misc
{
    public class ImmutableByteArrayExtensionsTest
    {
        [Fact]
        public void Hex()
        {
            Assert.Equal("00000000", Create<byte>(0, 0, 0, 0).Hex());
            Assert.Equal("ffffffffff", Create<byte>(0xff, 0xff, 0xff, 0xff, 0xff).Hex());
            Assert.Equal(
                "abbcdef01234567890",
                Create<byte>(0xab, 0xbc, 0xde, 0xf0, 0x12, 0x34, 0x56, 0x78, 0x90).Hex()
            );
        }

        [Fact]
        public void ParseHex_()
        {
            Assert.Equal(new byte[4], ParseHex("00000000"));
            Assert.Equal(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff }, ParseHex("ffffffffff"));
            Assert.Equal(
                new byte[] { 0xab, 0xbc, 0xde, 0xf0, 0x12, 0x34, 0x56, 0x78, 0x90 },
                ParseHex("abbcdef01234567890")
            );
        }
    }
}
