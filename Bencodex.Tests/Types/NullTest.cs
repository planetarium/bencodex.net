using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests.Types
{
    public class NullTest
    {
        [Fact]
        public void Value()
        {
            Assert.IsType<Null>(Null.Value);
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
