using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class IntegerTest
    {
        [Fact]
        public void Constructors()
        {
            IntegerGeneric(i => new Integer((short)i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((ushort)i) : (Integer?)null
            );
            IntegerGeneric(i => new Integer(i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((uint)i) : (Integer?)null
            );
            IntegerGeneric(i => new Integer((long)i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((ulong)i) : (Integer?)null
            );
            IntegerGeneric(i => new Integer(new BigInteger(i)));
            IntegerGeneric(i => new Integer(i.ToString()));
            var locale = new CultureInfo("ar-SA");
            IntegerGeneric(i => new Integer(i.ToString(locale), locale));
        }

        [Fact]
        public void Equality()
        {
            short s = 5;
            int i = 5;
            long l = 5;
            BigInteger b = new BigInteger(5);
            Integer x = new Integer(5);
            object os = s;
            object oi = i;
            object ol = l;
            object ob = b;
            object ox = x;

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(x == x);
            Assert.True(x.Equals(x));
            Assert.True(x.Equals(ox));
            Assert.True(ox.Equals(x));
            Assert.True(ox.Equals(ox));
#pragma warning restore CS1718

            Assert.True(s == x);
            Assert.True(i == x);
            Assert.True(l == x);
            Assert.True(b == x);

            Assert.True(x == s);
            Assert.True(x == i);
            Assert.True(x == l);
            Assert.True(x == b);

            Assert.True(s.Equals(x));
            Assert.True(i.Equals(x));
            Assert.True(l.Equals(x));
            Assert.True(b.Equals(x));

            Assert.True(x.Equals(s));
            Assert.True(x.Equals(i));
            Assert.True(x.Equals(l));
            Assert.True(x.Equals(b));

            Assert.False(s.Equals(ox));
            Assert.False(i.Equals(ox));
            Assert.False(l.Equals(ox));
            Assert.False(b.Equals(ox));

            Assert.False(x.Equals(os));
            Assert.False(x.Equals(oi));
            Assert.False(x.Equals(ol));
            Assert.False(x.Equals(ob));

            Assert.False(os.Equals(x));
            Assert.False(oi.Equals(x));
            Assert.False(ol.Equals(x));
            Assert.False(ob.Equals(x));

            Assert.False(os.Equals(ox));
            Assert.False(oi.Equals(ox));
            Assert.False(ol.Equals(ox));
            Assert.False(ob.Equals(ox));

            Assert.False(ox.Equals(os));
            Assert.False(ox.Equals(oi));
            Assert.False(ox.Equals(ol));
            Assert.False(ox.Equals(ob));
        }

        [Fact]
        public void Comparison()
        {
            short s = 5;
            int i = 5;
            long l = 5;
            BigInteger b = new BigInteger(5);
            Integer x = new Integer(5);
            Integer? n = null;
            object os = s;
            object oi = i;
            object ol = l;
            object ob = b;
            object ox = x;
            object on = null;

            Assert.Equal(0, x.CompareTo(x));
            Assert.Equal(0, x.CompareTo(ox));
            Assert.Equal(1, x.CompareTo(n));
            Assert.Equal(1, x.CompareTo(on));

            // NOTE: This behavior is different from comparing a primitive type
            // to a BigInteger; this is due to implicit operators present for the Integer type
            Assert.Equal(0, s.CompareTo(x));
            Assert.Equal(0, i.CompareTo(x));
            Assert.Equal(0, l.CompareTo(x));
            Assert.Equal(0, b.CompareTo(x));

            Assert.Equal(0, x.CompareTo(s));
            Assert.Equal(0, x.CompareTo(i));
            Assert.Equal(0, x.CompareTo(l));
            Assert.Equal(0, x.CompareTo(b));

            Assert.Throws<ArgumentException>(() => s.CompareTo(ox));
            Assert.Throws<ArgumentException>(() => i.CompareTo(ox));
            Assert.Throws<ArgumentException>(() => l.CompareTo(ox));
            Assert.Throws<ArgumentException>(() => b.CompareTo(ox));

            Assert.Throws<ArgumentException>(() => x.CompareTo(os));
            Assert.Throws<ArgumentException>(() => x.CompareTo(oi));
            Assert.Throws<ArgumentException>(() => x.CompareTo(ol));
            Assert.Throws<ArgumentException>(() => x.CompareTo(ob));
        }

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.Integer, new Integer(0).Kind);
            Assert.Equal(ValueKind.Integer, new Integer(123).Kind);
            Assert.Equal(ValueKind.Integer, new Integer(-456).Kind);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(3L, new Integer(0).EncodingLength);
            Assert.Equal(5L, new Integer(123).EncodingLength);
            Assert.Equal(6L, new Integer(-456).EncodingLength);
        }

        [Theory]
        [InlineData(new object[] { false })]
        [InlineData(new object[] { true })]
        public void Inspect(bool loadAll)
        {
            Assert.Equal("123", new Integer(123).Inspect(loadAll));
            Assert.Equal("-456", new Integer(-456).Inspect(loadAll));
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.Integer 123", new Integer(123).ToString());
            Assert.Equal("Bencodex.Types.Integer -456", new Integer(-456).ToString());
        }

        [Fact]
        public void CountDecimalDigits()
        {
            for (int i = -1000; i <= 1000; i++)
            {
                Assert.Equal(
                    i.ToString(CultureInfo.InvariantCulture).Length,
                    new Integer(i).CountDecimalDigits()
                );
            }

            var random = new Random();
            for (int i = 0; i < 100; i++)
            {
                int n = random.Next(int.MinValue, int.MaxValue);
                Assert.Equal(
                    n.ToString(CultureInfo.InvariantCulture).Length,
                    new Integer(n).CountDecimalDigits()
                );
            }
        }

        [Fact]
        public void InvalidFormats()
        {
            Codec codec = new Codec();
            byte[] case1 = Encoding.ASCII.GetBytes("ie");
            byte[] case2 = Encoding.ASCII.GetBytes("i+142e");
            byte[] case3 = Encoding.ASCII.GetBytes("i00e");
            byte[] case4 = Encoding.ASCII.GetBytes("i-0e");

            Assert.Throws<DecodingException>(() => codec.Decode(case1));
            Assert.Throws<DecodingException>(() => codec.Decode(case2));
            Assert.Throws<DecodingException>(() => codec.Decode(case3));
            Assert.Throws<DecodingException>(() => codec.Decode(case4));
        }

        private void IntegerGeneric(Func<int, Integer?> convert)
        {
            Codec codec = new Codec();
            AssertEqual(
                new byte[] { 0x69, 0x31, 0x32, 0x33, 0x65 },  // "i123e"
                codec.Encode(convert(123))
            );
            Integer? i = convert(-123);
            if (i != null)
            {
                AssertEqual(
                    new byte[]
                    {
                        // "i-123e"
                        0x69, 0x2d, 0x31, 0x32, 0x33, 0x65,
                    },
                    codec.Encode(i)
                );
            }

            AssertEqual(
                new byte[] { 0x69, 0x30, 0x65 },  // "i0e"
                codec.Encode(convert(0))
            );
            i = convert(-0);
            if (i != null)
            {
                AssertEqual(
                    new byte[] { 0x69, 0x30, 0x65 },  // "i0e"
                    codec.Encode(i)
                );
            }
        }
    }
}
