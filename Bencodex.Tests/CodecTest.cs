using System.Text;
using Bencodex.Types;
using Xunit;
using Xunit.Abstractions;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests
{
    public class CodecTest
    {
        private readonly ITestOutputHelper _output;
        private readonly Encoding _utf8;

        public CodecTest(ITestOutputHelper output)
        {
            _output = output;
            _utf8 = Encoding.GetEncoding(
                "UTF-8",
                new EncoderReplacementFallback(),
                new DecoderReplacementFallback()
            );
        }

        [Theory]
        [ClassData(typeof(SpecTheoryData))]
        public void SpecTestSuite(Spec spec)
        {
            _output.WriteLine("YAML: {0}", spec.SemanticsPath);
            _output.WriteLine("Data: {0}", spec.EncodingPath);
            Codec codec = new Codec();
            IValue decoded = codec.Decode(spec.Encoding);
            _output.WriteLine("Value: {0}", decoded.Inspect(false));
            Assert.Equal(spec.Semantics, decoded);
            Assert.Equal(spec.Encoding.LongLength, decoded.EncodingLength);
            Assert.Equal(spec.Semantics.EncodingLength, decoded.EncodingLength);
            Assert.Equal(spec.Semantics.Fingerprint, decoded.Fingerprint);

            byte[] encoded = codec.Encode(spec.Semantics);
            AssertEqual(spec.Encoding, encoded);
        }
    }
}
