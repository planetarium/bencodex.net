using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests.Types
{
    // FIXME: Still some tests remain ValueTests.Boolean; they should come here.
    public class BooleanTest
    {
        private readonly Boolean _t = new Boolean(true);
        private readonly Boolean _f = new Boolean(false);

        [Fact]
        public void SingletonFingerprints()
        {
            Assert.Equal(
                new Fingerprint(ValueType.Boolean, 1, new byte[] { 1 }),
                Boolean.TrueFingerprint
            );
            Assert.Equal(
                new Fingerprint(ValueType.Boolean, 1, new byte[] { 0 }),
                Boolean.FalseFingerprint
            );
            Assert.NotEqual(Boolean.FalseFingerprint, Boolean.TrueFingerprint);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(new Fingerprint(ValueType.Boolean, 1, new byte[] { 1 }), _t.Fingerprint);
            Assert.Equal(new Fingerprint(ValueType.Boolean, 1, new byte[] { 0 }), _f.Fingerprint);
        }
    }
}
