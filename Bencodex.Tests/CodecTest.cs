using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests
{
    public class CodecTest
    {
        [Theory]
        [ClassData(typeof(SpecTheoryData))]
        public void SpecTestSuite(Spec spec)
        {
            Codec codec = new Codec();
            IValue decoded = codec.Decode(spec.Encoding);
            Assert.Equal(spec.Semantics, decoded);
        }
    }
}
