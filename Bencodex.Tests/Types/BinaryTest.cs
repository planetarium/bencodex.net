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
        public void Fingerprint()
        {
            Assert.Equal(new Fingerprint(ValueKind.Binary, 2L), _empty.Fingerprint);
            Assert.Equal(
                new Fingerprint(ValueKind.Binary, 7L, _hello.ByteArray),
                _hello.Fingerprint
            );

            var longBin = new Binary(
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor " +
                "incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis " +
                "nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore " +
                "eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, " +
                "sunt in culpa qui officia deserunt mollit anim id est laborum.",
                Encoding.UTF8
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.Binary,
                    449L,
                    ParseHex("cd36b370758a259b34845084a6cc38473cb95e27")
                ),
                longBin.Fingerprint
            );
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
        public void Equality()
        {
            Assert.Equal(_empty, new Binary(new byte[0]));
            Assert.Equal<IValue>(_empty, new Binary(new byte[0]));
            Assert.Equal(_empty, ImmutableArray<byte>.Empty);
            Assert.Equal(_empty, new byte[0]);

            Assert.Equal(
                _hello,
                new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f })
            );
            Assert.Equal(
                _hello,
                new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f }.ToImmutableArray<byte>()
            );
            Assert.Equal(
                _hello,
                new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f }
            );

            Assert.NotEqual(_empty, _hello);
            Assert.NotEqual<IValue>(_empty, _hello);
            Assert.NotEqual(
                _hello,
                new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6f, 0x6f })
            );

            Assert.NotEqual<IValue>(Null.Value, _empty);
            Assert.NotEqual<IValue>(Null.Value, _hello);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(2L, _empty.EncodingLength);
            Assert.Equal(7L, _hello.EncodingLength);
            Assert.Equal(13L, new Binary(new byte[10]).EncodingLength);
        }

        [Theory]
        [InlineData(new object[] { false })]
        [InlineData(new object[] { true })]
        public void Inspect(bool loadAll)
        {
            Assert.Equal("b\"\"", _empty.Inspect(loadAll));
            Assert.Equal(@"b""\x68\x65\x6c\x6c\x6f""", _hello.Inspect(loadAll));
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
