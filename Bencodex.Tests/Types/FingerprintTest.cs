using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Bencodex.Types;
using Xunit;
using ValueType = Bencodex.Types.ValueType;

namespace Bencodex.Tests.Types
{
    public class FingerprintTest
    {
        [Fact]
        public void Constructor()
        {
            var f = new Fingerprint(ValueType.Binary, 5, new byte[] { 1, 2, 3, 4, 5 });
            Assert.Equal(ValueType.Binary, f.Type);
            Assert.Equal(5, f.EncodingLength);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, f.GetDigest());

            byte[] hash = new Random().NextBytes(20);
            f = new Fingerprint(ValueType.List, 100, hash);
            Assert.Equal(ValueType.List, f.Type);
            Assert.Equal(100, f.EncodingLength);
            Assert.Equal(hash, f.GetDigest());
        }

        [Fact]
        public void Digest()
        {
            var nullId = new Fingerprint(ValueType.Null, 1);
            Assert.Empty(nullId.Digest);
            Assert.Empty(nullId.GetDigest());

            var binId = new Fingerprint(ValueType.Binary, 5, new byte[] { 1, 2, 3, 4, 5 });
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, binId.GetDigest());
            Assert.Equal(binId.GetDigest(), binId.Digest);

            var hash = new Random().NextBytes(20);
            var listId = new Fingerprint(ValueType.List, 123, hash);
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

            var l123A = new Fingerprint(ValueType.List, 123, hashA);
            Assert.False(l123A.Equals(null));
            Assert.False(l123A.Equals("other"));

            var l123A_ = new Fingerprint(ValueType.List, 123, hashA.ToImmutableList());
            Assert.Equal(l123A, l123A_);
            Assert.True(l123A.Equals((object)l123A_));
            Assert.Equal(l123A.GetHashCode(), l123A_.GetHashCode());
            Assert.Equal(l123A.Serialize(), l123A_.Serialize());

            var d123A = new Fingerprint(ValueType.Dictionary, 123, hashA);
            Assert.NotEqual(l123A, d123A);
            Assert.False(l123A.Equals((object)d123A));
            Assert.NotEqual(l123A.GetHashCode(), d123A.GetHashCode());
            Assert.NotEqual(l123A.Serialize(), d123A.Serialize());

            var l122A = new Fingerprint(ValueType.List, 122, hashA);
            Assert.NotEqual(l123A, l122A);
            Assert.False(l123A.Equals((object)l122A));
            Assert.NotEqual(l123A.GetHashCode(), l122A.GetHashCode());
            Assert.NotEqual(l123A.Serialize(), l122A.Serialize());

            var l123B = new Fingerprint(ValueType.List, 123, hashB);
            Assert.NotEqual(l123A, l123B);
            Assert.False(l123A.Equals((object)l123B));
            Assert.NotEqual(l123A.GetHashCode(), l123B.GetHashCode());
            Assert.NotEqual(l123A.Serialize(), l123B.Serialize());
        }

        [Fact]
        public void Serialize()
        {
            var random = new Random();
            byte[] hash = random.NextBytes(20);
            var f = new Fingerprint(ValueType.List, 123, hash);
            Assert.Equal(
                new Fingerprint(ValueType.List, 123, hash.ToImmutableList()).Serialize(),
                f.Serialize()
            );
            Assert.Equal(f, Fingerprint.Deserialize(f.Serialize()));

            var s = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(s, f);
            s.Seek(0, SeekOrigin.Begin);
            Assert.Equal(f, (Fingerprint)formatter.Deserialize(s));

            hash = random.NextBytes(20);
            f = new Fingerprint(ValueType.Dictionary, 456, hash);
            Assert.Equal(f, Fingerprint.Deserialize(f.Serialize()));

            s = new MemoryStream();
            formatter.Serialize(s, f);
            s.Seek(0, SeekOrigin.Begin);
            Assert.Equal(f, (Fingerprint)formatter.Deserialize(s));
        }

        [Fact]
        public void String()
        {
            var f = new Fingerprint(ValueType.Null, 1);
            Assert.Equal("Null [1 B]", f.ToString());

            f = new Fingerprint(ValueType.Boolean, 1, new byte[] { 1 });
            Assert.Equal("Boolean 01 [1 B]", f.ToString());

            f = new Fingerprint(ValueType.Binary, 7, new byte[] { 0x12, 0xff, 0xab, 0x67, 0x99 });
            Assert.Equal("Binary 12ffab6799 [7 B]", f.ToString());

            f = new Fingerprint(ValueType.List, 100, new byte[20]);
            Assert.Equal("List 0000000000000000000000000000000000000000 [100 B]", f.ToString());
        }
    }
}
