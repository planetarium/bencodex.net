using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;

namespace Bencodex.Tests.Types
{
    // FIXME: Still some tests remain ValueTests.UnicodeText; they should come here.
    public class TextTest
    {
        private readonly Text _empty = default(Text);
        private readonly Text _nihao = new Text("\u4f60\u597d");
        private readonly Text _complex = new Text("new lines and\n\"quotes\" become escaped to \\");

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
    }
}
