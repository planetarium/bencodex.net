using System;
using System.Globalization;
using System.Numerics;
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

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(
                new Fingerprint(ValueKind.Integer, 3L, new byte[] { 0 }),
                new Integer(0).Fingerprint
            );
            Assert.Equal(
                new Fingerprint(ValueKind.Integer, 4L, new byte[] { 45 }),
                new Integer(45).Fingerprint
            );
            Assert.Equal(
                new Fingerprint(ValueKind.Integer, 6L, new byte[] { 0b10000101 }),
                new Integer(-123).Fingerprint
            );
            BigInteger bigint = BigInteger.Parse(
                "10000000000000000000000000000000000000000",
                NumberStyles.HexNumber
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.Integer,
                    51L,
                    ParseHex("8209ad2f4fad401d8e3d33def02577bd9ab550e5")
                ),
                new Integer(bigint).Fingerprint
            );
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
