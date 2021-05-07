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
    }
}
