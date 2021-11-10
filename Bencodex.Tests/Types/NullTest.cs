using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests.Types
{
    // FIXME: Still some tests remain ValueTests.Null; they should come here.
    public class NullTest
    {
        [Fact]
        public void Value()
        {
            Assert.IsType<Null>(Null.Value);
        }

        [Fact]
        public void SingletonFingerprint()
        {
            Assert.Equal(new Fingerprint(ValueType.Null, 1), Null.SingletonFingerprint);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(new Fingerprint(ValueType.Null, 1), Null.Value.Fingerprint);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(1, Null.Value.EncodingLength);
        }

        [Fact]
        public void Type()
        {
            Assert.Equal(ValueType.Null, Null.Value.Type);
        }
    }
}
