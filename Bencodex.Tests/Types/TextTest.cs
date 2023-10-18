using System;
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
        public void Kind()
        {
            Assert.Equal(ValueKind.Text, _empty.Kind);
            Assert.Equal(ValueKind.Text, _nihao.Kind);
            Assert.Equal(ValueKind.Text, _complex.Kind);
        }

        [Fact]
        public void Equality()
        {
            string s = "foo";
            Text t = new Text("foo");
            object os = (object)s;
            object ot = (object)t;

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(t == t);
            Assert.True(t.Equals(t));
            Assert.True(t.Equals(ot));
            Assert.True(ot.Equals(t));
            Assert.True(ot.Equals(ot));
#pragma warning restore CS1718

            Assert.True(s == t);
            Assert.True(t == s);
            Assert.True(s.Equals(t));
            Assert.True(t.Equals(t));

            Assert.False(s.Equals(ot));
            Assert.False(t.Equals(os));
            Assert.False(os.Equals(t));
            Assert.False(ot.Equals(s));
            Assert.False(os.Equals(ot));
            Assert.False(ot.Equals(os));
        }

        [Fact]
        public void Comparison()
        {
            string s = "foo";
            Text t = new Text("foo");
            Text? n = null;
            object os = (object)s;
            object ot = (object)t;
            object on = null;

            Assert.Equal(0, t.CompareTo(t));
            Assert.Equal(0, t.CompareTo(ot));
            Assert.Equal(1, t.CompareTo(n));
            Assert.Equal(1, t.CompareTo(on));

            Assert.Equal(0, s.CompareTo(t));
            Assert.Equal(0, t.CompareTo(s));

            Assert.Throws<ArgumentException>(() => s.CompareTo(ot));
            Assert.Throws<ArgumentException>(() => t.CompareTo(os));

            Text t0 = new Text("0");
            Text t1 = new Text("1");
            Text t00 = new Text("00");

            Assert.Equal(0, t0.CompareTo(t0));
            Assert.True(t0.CompareTo(t1) < 0);
            Assert.True(t1.CompareTo(t0) > 0);
            Assert.True(t0.CompareTo(t00) < 0);
            Assert.True(t00.CompareTo(t0) > 0);
            Assert.True(t1.CompareTo(t00) > 0);
            Assert.True(t00.CompareTo(t1) < 0);
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
            Assert.Equal(new Fingerprint(ValueKind.Text, 3L), _empty.Fingerprint);
            Assert.Equal(
                new Fingerprint(
                    ValueKind.Text,
                    9L,
                    new byte[] { 0xe4, 0xbd, 0xa0, 0xe5, 0xa5, 0xbd }
                ),
                _nihao.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.Text,
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
