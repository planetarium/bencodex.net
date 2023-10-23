using Bencodex.Types;
using Xunit;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class BooleanTest
    {
        private readonly Boolean _t = new Boolean(true);
        private readonly Boolean _f = new Boolean(false);

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.Boolean, _t.Kind);
            Assert.Equal(ValueKind.Boolean, _f.Kind);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(1L, _t.EncodingLength);
            Assert.Equal(1L, _f.EncodingLength);
        }

        [Fact]
        public void Encode()
        {
            Codec codec = new Codec();
            AssertEqual(
                new byte[] { 0x74 },  // "t"
                codec.Encode(_t)
            );
            AssertEqual(
                new byte[] { 0x66 },  // "f"
                codec.Encode(_f)
            );
        }

        [Fact]
        public void Inspect()
        {
            Assert.Equal("true", _t.Inspect());
            Assert.Equal("false", _f.Inspect());
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.Boolean true", _t.ToString());
            Assert.Equal("Bencodex.Types.Boolean false", _f.ToString());
        }
    }
}
