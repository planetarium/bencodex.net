using Bencodex.Types;
using Xunit;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class BooleanTest
    {
        private readonly Boolean _t = new Boolean(true);
        private readonly Boolean _f = new Boolean(false);

        [Fact]
        public void Equality()
        {
            bool b = true;
            Boolean x = new Boolean(true);
            object ob = (object)b;
            object ox = (object)x;

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(x == x);
            Assert.True(x.Equals(x));
            Assert.True(x.Equals(ox));
            Assert.True(ox.Equals(x));
            Assert.True(ox.Equals(ox));
#pragma warning restore CS1718

            Assert.True(b == x);
            Assert.True(x == b);
            Assert.True(b.Equals(x));
            Assert.True(x.Equals(x));

            Assert.False(b.Equals(ox));
            Assert.False(x.Equals(ob));
            Assert.False(ob.Equals(x));
            Assert.False(ox.Equals(b));
            Assert.False(ob.Equals(ox));
            Assert.False(ox.Equals(ob));
        }

        [Fact]
        public void Comparison()
        {
            bool b = true;
            Boolean x = new Boolean(true);
            Boolean? n = null;
            object ob = (object)b;
            object ox = (object)x;
            object on = null;

            Assert.Equal(0, x.CompareTo(x));
            Assert.Equal(0, x.CompareTo(ox));
            Assert.Equal(1, x.CompareTo(n));
            Assert.Equal(1, x.CompareTo(on));

            Assert.Equal(0, b.CompareTo(x));
            Assert.Equal(0, x.CompareTo(b));

            Assert.Throws<System.ArgumentException>(() => b.CompareTo(ox));
            Assert.Throws<System.ArgumentException>(() => x.CompareTo(ob));

            Boolean f = new Boolean(false);
            Boolean t = new Boolean(true);

            Assert.Equal(0, f.CompareTo(f));
            Assert.Equal(0, t.CompareTo(t));
            Assert.True(f.CompareTo(t) < 0);
            Assert.True(t.CompareTo(f) > 0);
        }

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.Boolean, _t.Kind);
            Assert.Equal(ValueKind.Boolean, _f.Kind);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(1L, _t.EncodingLength);
            Assert.Equal(1L, _f.EncodingLength);
        }

        [Fact]
        public void Encode()
        {
            Codec codec = new Codec();
            AssertEqual(
                new byte[] { 0x74 },  // "t"
                codec.Encode(_t)
            );
            AssertEqual(
                new byte[] { 0x66 },  // "f"
                codec.Encode(_f)
            );
        }

        [Fact]
        public void Inspect()
        {
            Assert.Equal("true", _t.Inspect());
            Assert.Equal("false", _f.Inspect());
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.Boolean true", _t.ToString());
            Assert.Equal("Bencodex.Types.Boolean false", _f.ToString());
        }
    }
}
