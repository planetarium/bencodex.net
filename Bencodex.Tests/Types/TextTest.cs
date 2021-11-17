using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class TextTest
    {
        private readonly Text _empty = default(Text);
        private readonly Text _nihao = new Text("\u4f60\u597d");
        private readonly Text _complex = new Text("new lines and\n\"quotes\" become escaped to \\");

        [Fact]
        public void Test()
        {
            Assert.Equal(ValueType.Text, _empty.Type);
            Assert.Equal(ValueType.Text, _nihao.Type);
            Assert.Equal(ValueType.Text, _complex.Type);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(3L, _empty.EncodingLength);
            Assert.Equal(9L, _nihao.EncodingLength);
            Assert.Equal(46L, _complex.EncodingLength);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(new Fingerprint(ValueType.Text, 3L), _empty.Fingerprint);
            Assert.Equal(
                new Fingerprint(
                    ValueType.Text,
                    9L,
                    new byte[] { 0xe4, 0xbd, 0xa0, 0xe5, 0xa5, 0xbd }
                ),
                _nihao.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueType.Text,
                    46L,
                    ParseHex("e72dcfd0ae50a80aaa8c1a78b27e2e11bef66488")
                ),
                _complex.Fingerprint
            );
        }

        [Fact]
        public void Encode()
        {
            Codec codec = new Codec();
            AssertEqual(
                new byte[] { 0x75, 0x30, 0x3a },  // "u0:"
                codec.Encode(_empty)
            );
            AssertEqual(
                new byte[]
                {
                    0x75, 0x36, 0x3a, 0xe4, 0xbd,
                    0xa0, 0xe5, 0xa5, 0xbd,

                    // "u6:\xe4\xbd\xa0\xe5\xa5\xbd"
                },
                codec.Encode(_nihao) // "你好"
            );
        }

        [Theory]
        [InlineData(new object[] { false })]
        [InlineData(new object[] { true })]
        public void Inspect(bool loadAll)
        {
            Assert.Equal("\"\"", _empty.Inspect(loadAll));
            Assert.Equal("\"\u4f60\u597d\"", _nihao.Inspect(loadAll));
            Assert.Equal(
                "\"new lines and\\n\\\"quotes\\\" become escaped to \\\\\"",
                _complex.Inspect(loadAll)
            );
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.Text \"\"", _empty.ToString());
            Assert.Equal("Bencodex.Types.Text \"\u4f60\u597d\"", _nihao.ToString());
        }
    }
}
