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
        public void SingletonFingerprint()
        {
            Assert.Equal(new Fingerprint(ValueType.Null, 1L), Null.SingletonFingerprint);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(new Fingerprint(ValueType.Null, 1L), Null.Value.Fingerprint);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(1L, Null.Value.EncodingLength);
        }

        [Fact]
        public void Type()
        {
            Assert.Equal(ValueType.Null, Null.Value.Type);
        }

        [Theory]
        [InlineData(new object[] { false })]
        [InlineData(new object[] { true })]
        public void Inspect(bool loadAll)
        {
            Assert.Equal("null", Null.Value.Inspect(loadAll));
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
