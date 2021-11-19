using Bencodex.Types;
using Xunit;
using Xunit.Abstractions;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class ValueTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Codec _codec;

        public ValueTests(ITestOutputHelper output)
        {
            _output = output;
            _codec = new Codec();
        }

        [Fact]
        public void Binary()
        {
            // FIXME: Move to BinaryTest.
            var empty = default(Binary);
            var hello = new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f });

            AssertEqual(
                new byte[] { 0x30, 0x3a },  // "0:"
                _codec.Encode(empty)
            );
            AssertEqual(
                new byte[] { 0x35, 0x3a, 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                _codec.Encode(hello) // "5:hello"
            );
        }

        [Theory]
        [ClassData(typeof(SpecTheoryData))]
        public void SpecTestSuite(Spec spec)
        {
            _output.WriteLine("YAML: {0}", spec.SemanticsPath);
            _output.WriteLine("Data: {0}", spec.EncodingPath);
            AssertEqual(
                spec.Encoding,
                _codec.Encode(spec.Semantics),
                spec.SemanticsPath
            );
            Assert.Equal(spec.Encoding.LongLength, spec.Semantics.EncodingLength);
        }
    }
}
