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
        public void Constructor()
        {
            Assert.NotNull(default(Binary).Value);
            Assert.Empty(default(Binary).Value);

            var hello = new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f });
            Assert.Equal(
                new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                hello.Value
            );

            var fromString = new Binary("hello", Encoding.ASCII);
            Assert.Equal(hello.Value, fromString.Value);
        }

        [Fact]
        public void Equality()
        {
            Assert.Equal(_empty, new Binary(new byte[0]));
            Assert.Equal(
                _hello,
                new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f })
            );

            Assert.NotEqual(_empty, _hello);
            Assert.NotEqual(
                _hello,
                new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6f, 0x6f })
            );
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
