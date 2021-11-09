using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
            byte[] hash = new Random().NextBytes(20);
            var f = new Fingerprint(ValueType.Binary, 10, hash);
            Assert.Equal(ValueType.Binary, f.Type);
            Assert.Equal(10, f.EncodingLength);
            Assert.Equal(hash, f.GetHashArray());

            Assert.Throws<ArgumentException>(
                () => { new Fingerprint(ValueType.List, 3, new byte[19]); }
            );
            Assert.Throws<ArgumentException>(
                () => { new Fingerprint(ValueType.List, 3, new byte[21]); }
            );
        }

        [Fact]
        public void Hash()
        {
            var nullId = new Fingerprint(ValueType.Null, 1);
            Assert.Empty(nullId.Hash);
            Assert.Empty(nullId.GetHashArray());

            byte[] hash = new Random().NextBytes(20);
            var listId = new Fingerprint(ValueType.List, 123, hash);
            Assert.Equal(hash, listId.Hash);
            Assert.Equal(hash, listId.GetHashArray());
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

            var d123A = new Fingerprint(ValueType.Dictionary, 123, hashA);
            Assert.NotEqual(l123A, d123A);
            Assert.False(l123A.Equals((object)d123A));
            Assert.NotEqual(l123A.GetHashCode(), d123A.GetHashCode());

            var l122A = new Fingerprint(ValueType.List, 122, hashA);
            Assert.NotEqual(l123A, l122A);
            Assert.False(l123A.Equals((object)l122A));
            Assert.NotEqual(l123A.GetHashCode(), l122A.GetHashCode());

            var l123B = new Fingerprint(ValueType.List, 123, hashB);
            Assert.NotEqual(l123A, l123B);
            Assert.False(l123A.Equals((object)l123B));
            Assert.NotEqual(l123A.GetHashCode(), l123B.GetHashCode());
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
    }
}
