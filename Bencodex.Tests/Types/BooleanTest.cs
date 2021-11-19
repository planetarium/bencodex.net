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
        public void SingletonFingerprints()
        {
            Assert.Equal(
                new Fingerprint(ValueType.Boolean, 1L, new byte[] { 1 }),
                Boolean.TrueFingerprint
            );
            Assert.Equal(
                new Fingerprint(ValueType.Boolean, 1L, new byte[] { 0 }),
                Boolean.FalseFingerprint
            );
            Assert.NotEqual(Boolean.FalseFingerprint, Boolean.TrueFingerprint);
        }

        [Fact]
        public void Type()
        {
            Assert.Equal(ValueType.Boolean, _t.Type);
            Assert.Equal(ValueType.Boolean, _f.Type);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(1L, _t.EncodingLength);
            Assert.Equal(1L, _f.EncodingLength);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(new Fingerprint(ValueType.Boolean, 1L, new byte[] { 1 }), _t.Fingerprint);
            Assert.Equal(new Fingerprint(ValueType.Boolean, 1L, new byte[] { 0 }), _f.Fingerprint);
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Inspect(bool loadAll)
        {
            Assert.Equal("true", _t.Inspect(loadAll));
            Assert.Equal("false", _f.Inspect(loadAll));
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.Boolean true", _t.ToString());
            Assert.Equal("Bencodex.Types.Boolean false", _f.ToString());
        }
    }
}
