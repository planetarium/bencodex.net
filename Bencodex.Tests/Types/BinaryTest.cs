using System.Collections.Immutable;
using System.Text;
using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests.Types
{
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
            Assert.Equal(
                new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                hello.ToByteArray()
            );
        }

        [Fact]
        public void ConstructorTakingString()
        {
            var hello = new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f });
            var fromString = new Binary("hello", Encoding.ASCII);
            Assert.Equal(hello, fromString);
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
            Assert.Equal(2, _empty.EncodingLength);
            Assert.Equal(7, _hello.EncodingLength);
            Assert.Equal(13, new Binary(new byte[10]).EncodingLength);
        }

        [Fact]
        public void Inspection()
        {
            Assert.Equal("b\"\"", _empty.Inspection);
            Assert.Equal(@"b""\x68\x65\x6c\x6c\x6f""", _hello.Inspection);
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
    }
}
