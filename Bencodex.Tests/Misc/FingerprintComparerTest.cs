using System.Collections.Generic;
using System.Text;
using Bencodex.Misc;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using F = Bencodex.Types.Fingerprint;

namespace Bencodex.Tests.Misc
{
    public class FingerprintComparerTest
    {
        [Fact]
        public void Compare()
        {
            var n = new F(ValueKind.Null, 1);
            var f = new F(ValueKind.Boolean, 1, new byte[] { 0 });
            var t = new F(ValueKind.Boolean, 1, new byte[] { 1 });
            var i0 = new F(ValueKind.Integer, 3, new byte[] { 0 });
            var i45 = new F(ValueKind.Integer, 4, new byte[] { 45 });
            var iM123 = new F(ValueKind.Integer, 6, new byte[] { 0b10000101 });
            var b = new F(ValueKind.Binary, 2);
            var bHello = new F(ValueKind.Binary, 7, Encoding.ASCII.GetBytes("hello"));
            var b445 = new F(ValueKind.Binary, 449, ParseHex("cd36b370758a259b34845084a6cc38473cb95e27"));
            var u = new F(ValueKind.Text, 3);
            var uNihao = new F(ValueKind.Text, 9, new byte[] { 0xe4, 0xbd, 0xa0, 0xe5, 0xa5, 0xbd });
            var u42 = new F(ValueKind.Text, 46, ParseHex("e72dcfd0ae50a80aaa8c1a78b27e2e11bef66488"));
            var l = new F(ValueKind.List, 2);
            var l1 = new F(ValueKind.List, 3, ParseHex("ae7fca60943c2ef2f6cf5420477da41acf29b01d"));
            var l2 = new F(ValueKind.List, 18, ParseHex("22852139f287a01cdb803fd86ed70e4c4d121254"));
            var lNest = new F(ValueKind.List, 26, ParseHex("24caa983a5225522ca798be3b31a1abecdb36fe5"));
            var d = new F(ValueKind.Dictionary, 2);
            var d1 = new F(ValueKind.Dictionary, 14, ParseHex("9312bb9fe32ec11b4e6478f557ea821b0c44523d"));
            var unordered = new List<Fingerprint>
            {
                lNest, l2, u, n, bHello, i0, l1, uNihao,
                d1, f, iM123, d, b445, l, i45, t, u42, b,
            };
            unordered.Sort(new FingerprintComparer());
            Fingerprint[] ordered =
            {
                n, f, t, i0, i45, iM123, b, bHello, b445,
                u, uNihao, u42, l, l1, l2, lNest, d, d1,
            };
            Assert.Equal(ordered, unordered);
        }
    }
}
