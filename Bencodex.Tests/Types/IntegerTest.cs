using System.Globalization;
using System.Numerics;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using ValueType = Bencodex.Types.ValueType;

namespace Bencodex.Tests.Types
{
    // FIXME: Still some tests remain ValueTests.Integer; they should come here.
    public class IntegerTest
    {
        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(
                new Fingerprint(ValueType.Integer, 3, new byte[] { 0 }),
                new Integer(0).Fingerprint
            );
            Assert.Equal(
                new Fingerprint(ValueType.Integer, 4, new byte[] { 45 }),
                new Integer(45).Fingerprint
            );
            Assert.Equal(
                new Fingerprint(ValueType.Integer, 6, new byte[] { 0b10000101 }),
                new Integer(-123).Fingerprint
            );
            BigInteger bigint = BigInteger.Parse(
                "10000000000000000000000000000000000000000",
                NumberStyles.HexNumber
            );
            Assert.Equal(
                new Fingerprint(
                    ValueType.Integer,
                    51,
                    ParseHex("8209ad2f4fad401d8e3d33def02577bd9ab550e5")
                ),
                new Integer(bigint).Fingerprint
            );
        }
    }
}
