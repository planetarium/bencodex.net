using Bencodex.Types;
using Xunit;
using Xunit.Abstractions;

namespace Bencodex.Tests
{
    public class CodecTest
    {
        private readonly ITestOutputHelper _output;

        public CodecTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [ClassData(typeof(SpecTheoryData))]
        public void SpecTestSuite(Spec spec)
        {
            _output.WriteLine("YAML: {0}", spec.SemanticsPath);
            _output.WriteLine("Data: {0}", spec.EncodingPath);
            Codec codec = new Codec();
            IValue decoded = codec.Decode(spec.Encoding);
            Assert.Equal(spec.Semantics, decoded);
            Assert.Equal(spec.Encoding.LongLength, decoded.EncodingLength);
            Assert.Equal(spec.Semantics.EncodingLength, decoded.EncodingLength);
            Assert.Equal(spec.Semantics.Fingerprint, decoded.Fingerprint);
        }
    }
}
