using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests.Types
{
    public class FingerprintTest
    {
        [Fact]
        public void Constructor()
        {
            var f = new Fingerprint(ValueKind.Binary, 5, new byte[] { 1, 2, 3, 4, 5 });
            Assert.Equal(ValueKind.Binary, f.Kind);
            Assert.Equal(5L, f.EncodingLength);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, f.GetDigest());

            byte[] hash = new Random().NextBytes(20);
            f = new Fingerprint(ValueKind.List, 100, hash);
            Assert.Equal(ValueKind.List, f.Kind);
            Assert.Equal(100L, f.EncodingLength);
            Assert.Equal(hash, f.GetDigest());
        }

        [Fact]
        public void Digest()
        {
            var nullId = new Fingerprint(ValueKind.Null, 1);
            Assert.Empty(nullId.Digest);
            Assert.Empty(nullId.GetDigest());

            var binId = new Fingerprint(ValueKind.Binary, 5, new byte[] { 1, 2, 3, 4, 5 });
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, binId.GetDigest());
            Assert.Equal(binId.GetDigest(), binId.Digest);

            var hash = new Random().NextBytes(20);
            var listId = new Fingerprint(ValueKind.List, 123, hash);
            Assert.Equal(hash, listId.Digest);
            Assert.Equal(hash, listId.GetDigest());
        }

        [Fact]
        public void Equality()
        {
            byte[] hashA = new Random().NextBytes(20);
            var hashB = new byte[hashA.Length];
            hashA.CopyTo(hashB, 0);
            hashB[19] = (byte)(hashA[19] >= 0xff ? 0 : hashA[19] + 1);
            Assert.NotEqual(hashA, hashB);

            var l123A = new Fingerprint(ValueKind.List, 123, hashA);
            Assert.False(l123A.Equals(null));
            Assert.False(l123A.Equals("other"));

            var l123A_ = new Fingerprint(ValueKind.List, 123, hashA.ToImmutableList());
            Assert.Equal(l123A, l123A_);
            Assert.True(l123A.Equals((object)l123A_));
            Assert.Equal(l123A.GetHashCode(), l123A_.GetHashCode());
            Assert.Equal(l123A.Serialize(), l123A_.Serialize());
            Assert.True(l123A == l123A_);
            Assert.False(l123A != l123A_);

            var d123A = new Fingerprint(ValueKind.Dictionary, 123, hashA);
            Assert.NotEqual(l123A, d123A);
            Assert.False(l123A.Equals((object)d123A));
            Assert.NotEqual(l123A.GetHashCode(), d123A.GetHashCode());
            Assert.NotEqual(l123A.Serialize(), d123A.Serialize());
            Assert.False(l123A == d123A);
            Assert.True(l123A != d123A);

            var l122A = new Fingerprint(ValueKind.List, 122, hashA);
            Assert.NotEqual(l123A, l122A);
            Assert.False(l123A.Equals((object)l122A));
            Assert.NotEqual(l123A.GetHashCode(), l122A.GetHashCode());
            Assert.NotEqual(l123A.Serialize(), l122A.Serialize());
            Assert.False(l123A == l122A);
            Assert.True(l123A != l122A);

            var l123B = new Fingerprint(ValueKind.List, 123, hashB);
            Assert.NotEqual(l123A, l123B);
            Assert.False(l123A.Equals((object)l123B));
            Assert.NotEqual(l123A.GetHashCode(), l123B.GetHashCode());
            Assert.NotEqual(l123A.Serialize(), l123B.Serialize());
            Assert.False(l123A == l123B);
            Assert.True(l123A != l123B);
        }

        [Fact]
        public void Serialize()
        {
            var random = new Random();
            byte[] hash = random.NextBytes(20);
            var f = new Fingerprint(ValueKind.List, 123, hash);
            Assert.Equal(
                new Fingerprint(ValueKind.List, 123, hash.ToImmutableList()).Serialize(),
                f.Serialize()
            );
            Assert.Equal(f, Fingerprint.Deserialize(f.Serialize()));

            var s = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(s, f);
            s.Seek(0, SeekOrigin.Begin);
            Assert.Equal(f, (Fingerprint)formatter.Deserialize(s));

            hash = random.NextBytes(20);
            f = new Fingerprint(ValueKind.Dictionary, 456, hash);
            Assert.Equal(f, Fingerprint.Deserialize(f.Serialize()));

            byte[] tooShort = f.Serialize().Take(8).ToArray();
            FormatException e = Assert.Throws<FormatException>(
                () => Fingerprint.Deserialize(tooShort)
            );
            Assert.Contains("too short", e.Message, StringComparison.OrdinalIgnoreCase);

            byte[] invalidKind = f.Serialize();
            invalidKind[0] = byte.MaxValue;
            e = Assert.Throws<FormatException>(
                () => Fingerprint.Deserialize(invalidKind)
            );
            Assert.Contains("invalid value kind", e.Message, StringComparison.OrdinalIgnoreCase);

            s = new MemoryStream();
            formatter.Serialize(s, f);
            s.Seek(0, SeekOrigin.Begin);
            Assert.Equal(f, (Fingerprint)formatter.Deserialize(s));
        }

        [Fact]
        public void String()
        {
            var f = new Fingerprint(ValueKind.Null, 1);
            Assert.Equal("Null [1 B]", f.ToString());

            f = new Fingerprint(ValueKind.Boolean, 1, new byte[] { 1 });
            Assert.Equal("Boolean 01 [1 B]", f.ToString());

            f = new Fingerprint(ValueKind.Binary, 7, new byte[] { 0x12, 0xff, 0xab, 0x67, 0x99 });
            Assert.Equal("Binary 12ffab6799 [7 B]", f.ToString());

            f = new Fingerprint(ValueKind.List, 100, new byte[20]);
            Assert.Equal("List 0000000000000000000000000000000000000000 [100 B]", f.ToString());
        }
    }
}
