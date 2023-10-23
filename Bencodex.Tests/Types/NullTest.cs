using Bencodex.Types;
using Xunit;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class NullTest
    {
        [Fact]
        public void Value()
        {
            Assert.IsType<Null>(Null.Value);
            Assert.Equal(default(Null), Null.Value);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(1L, Null.Value.EncodingLength);
        }

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.Null, Null.Value.Kind);
        }

        [Fact]
        public void Inspect()
        {
            Assert.Equal("null", Null.Value.Inspect());
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.Null", Null.Value.ToString());
        }

        [Fact]
        public void Encode()
        {
            Codec codec = new Codec();
            AssertEqual(
                new byte[] { 0x6e },  // "n"
                codec.Encode(default(Null))
            );
        }
    }
}
