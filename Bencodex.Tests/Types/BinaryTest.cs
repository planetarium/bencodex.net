using System;
using System.Collections.Immutable;
using System.Text;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;

namespace Bencodex.Tests.Types
{
    // FIXME: Still some tests remain ValueTests.Binary; they should come here.
    public class BinaryTest
    {
        private Binary _empty;
        private Binary _hello;

        public BinaryTest()
        {
            _empty = default(Binary);
            _hello = new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f });
        }

        [Fact]
        public void DefaultConstructor()
        {
            Assert.Empty(default(Binary).ByteArray);
            Assert.NotNull(default(Binary).ToByteArray());
            Assert.Empty(default(Binary).ToByteArray());
        }

        [Fact]
        public void Equality()
        {
            byte[] b = new byte[] { 0, 1 };
            ImmutableArray<byte> i = ImmutableArray.Create(b);
            Binary x = new Binary(i);
            object ob = (object)b;
            object oi = (object)i;
            object ox = (object)x;

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(x == x);
            Assert.True(x.Equals(x));
            Assert.True(x.Equals(ox));
            Assert.True(ox.Equals(x));
            Assert.True(ox.Equals(ox));
#pragma warning restore CS1718

            // Unlike Integer and Text, implicit conversion is not supported.
            Assert.False(b.Equals(x));
            Assert.False(i.Equals(x));
            Assert.False(x.Equals(b));
            Assert.False(x.Equals(i));

            Assert.False(b.Equals(ox));
            Assert.False(i.Equals(ox));
            Assert.False(ox.Equals(b));
            Assert.False(ox.Equals(i));

            Assert.False(ob.Equals(ox));
            Assert.False(oi.Equals(ox));
            Assert.False(ox.Equals(ob));
            Assert.False(ox.Equals(oi));

            Binary empty = new Binary(Array.Empty<byte>());
            IValue n = Null.Value;
            Assert.False(empty.Equals(x));
            Assert.False(x.Equals(empty));
            Assert.False(empty.Equals(n));
            Assert.False(n.Equals(empty));
        }

        [Fact]
        public void Comparison()
        {
            Binary b0 = new Binary(new byte[] { 0 });
            Binary b1 = new Binary(new byte[] { 1 });
            Binary b00 = new Binary(new byte[] { 0, 0 });

            Assert.Equal(0, b0.CompareTo(b0));
            Assert.True(b0.CompareTo(b1) < 0);
            Assert.True(b1.CompareTo(b0) > 0);
            Assert.True(b0.CompareTo(b00) < 0);
            Assert.True(b00.CompareTo(b0) > 0);
            Assert.True(b1.CompareTo(b00) > 0);
            Assert.True(b00.CompareTo(b1) < 0);
        }

        [Fact]
        public void ConstructorTakingImmutableByteArray()
        {
            ImmutableArray<byte> bytes =
                new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f }.ToImmutableArray();
            var hello = new Binary(bytes);
            Assert.Equal(bytes, hello.ByteArray);
            Assert.Equal(
                new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                hello.ToByteArray()
            );
        }

        [Fact]
        public void ConstructorTakingByteArray()
        {
            var hello = new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f });
            Assert.Equal(_hello, hello);

            var hello2 = new Binary(0x68, 0x65, 0x6c, 0x6c, 0x6f);
            Assert.Equal(_hello, hello2);
        }

        [Fact]
        public void ConstructorTakingString()
        {
            var hello = new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f });
            var fromString = new Binary("hello", Encoding.ASCII);
            Assert.Equal(hello, fromString);
        }

        [Fact]
        public void FromHex()
        {
            Assert.Equal(_empty, Binary.FromHex(string.Empty));
            Assert.Equal(_empty, Binary.FromHex("abc", 3));
            Assert.Equal(_empty, Binary.FromHex("ABC", 1, 0));

            Assert.Equal(_hello, Binary.FromHex("68656c6c6f"));
            Assert.Equal(_hello, Binary.FromHex("hex:68656C6C6F", 4));
            Assert.Equal(_hello, Binary.FromHex("hex:'68656C6C6F'", 5, 10));

            Assert.Throws<ArgumentOutOfRangeException>(() => Binary.FromHex("abc", -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Binary.FromHex("abc", 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => Binary.FromHex("abc", 0, -2));
            Assert.Throws<ArgumentOutOfRangeException>(() => Binary.FromHex("abc", 0, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => Binary.FromHex("abc", 2, 2));
            Assert.Throws<ArgumentException>(() => Binary.FromHex("abc"));
            Assert.Throws<ArgumentException>(() => Binary.FromHex("abc", 2));
            Assert.Throws<ArgumentException>(() => Binary.FromHex("abcd", 1, 3));
            Assert.Throws<FormatException>(() => Binary.FromHex("abcx"));
        }

        [Fact]
        public void FromBase64()
        {
            Assert.Equal(_empty, Binary.FromBase64(string.Empty));
            Assert.Equal(_hello, Binary.FromBase64("aGVsbG8="));

#if !NETSTANDARD2_0
            Assert.Equal(_empty, Binary.FromBase64(ReadOnlySpan<char>.Empty));
            Assert.Equal(_hello, Binary.FromBase64("aGVsbG8=".AsSpan()));
#endif
        }

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.Binary, _empty.Kind);
            Assert.Equal(ValueKind.Binary, _hello.Kind);
        }

        [Fact]
        public void ToByteArray()
        {
            byte[] a = _hello.ToByteArray();
            Assert.Equal(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f }, a);

            a[3] = 0x6f;
            Assert.Equal(
                new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                _hello.ToByteArray()
            );
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(2L, _empty.EncodingLength);
            Assert.Equal(7L, _hello.EncodingLength);
            Assert.Equal(13L, new Binary(new byte[10]).EncodingLength);
        }

        [Fact]
        public void Inspect()
        {
            Assert.Equal("b\"\"", _empty.Inspect());
            Assert.Equal(@"b""\x68\x65\x6c\x6c\x6f""", _hello.Inspect());
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.Binary b\"\"", _empty.ToString());
            Assert.Equal(
                @"Bencodex.Types.Binary b""\x68\x65\x6c\x6c\x6f""",
                _hello.ToString()
            );
        }

        [Fact]
        public void ToHex()
        {
            var builder = new StringBuilder();
            _empty.ToHex(builder);
            Assert.Empty(builder.ToString());
            Assert.Empty(_empty.ToHex());

            builder.Clear();
            _hello.ToHex(builder);
            Assert.Equal("68656c6c6f", builder.ToString());
            Assert.Equal("68656c6c6f", _hello.ToHex());
        }
    }
}
