using Bencodex.Types;
using Xunit;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class TextTest
    {
        private readonly Text _empty = Text.Empty;
        private readonly Text _nihao = new Text("\u4f60\u597d");
        private readonly Text _complex = new Text("new lines and\n\"quotes\" become escaped to \\");

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.Text, _empty.Kind);
            Assert.Equal(ValueKind.Text, _nihao.Kind);
            Assert.Equal(ValueKind.Text, _complex.Kind);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(3L, _empty.EncodingLength);
            Assert.Equal(9L, _nihao.EncodingLength);
            Assert.Equal(46L, _complex.EncodingLength);
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
