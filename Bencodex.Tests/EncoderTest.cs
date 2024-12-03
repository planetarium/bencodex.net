using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Bencodex.Types;
using Xunit;
using static System.Array;
using static Bencodex.Tests.TestUtils;
using Boolean = Bencodex.Types.Boolean;

namespace Bencodex.Tests
{
    public class EncoderTest
    {
        [Fact]
        public void EstimateLength()
        {
            Assert.Equal(1, Encoder.EstimateLength(Null.Value));
            Assert.Equal(1, Encoder.EstimateLength(new Boolean(true)));
            Assert.Equal(5, Encoder.EstimateLength(new Integer(123)));
            Assert.Equal(
                14,
                Encoder.EstimateLength(new Binary("hello world", Encoding.ASCII))
            );
            Assert.Equal(15, Encoder.EstimateLength(new Text("hello world")));
        }

        [Fact]
        public void EncodeNull()
        {
            var buffer = new byte[10];
            long offset = 3L;
            Encoder.EncodeNull(buffer, ref offset);
            Assert.Equal(3L + 1L, offset);
            AssertEqual(new byte[] { 0, 0, 0, 0x6e, 0, 0, 0, 0, 0, 0 }, buffer);
        }

        [Fact]
        public void EncodeBoolean()
        {
            var buffer = new byte[10];
            long offset = 2L;
            Encoder.EncodeBoolean(new Boolean(true), buffer, ref offset);
            Assert.Equal(2L + 1L, offset);
            AssertEqual(new byte[] { 0, 0, 0x74, 0, 0, 0, 0, 0, 0, 0 }, buffer);

            offset = 5L;
            Encoder.EncodeBoolean(new Boolean(false), buffer, ref offset);
            Assert.Equal(5L + 1L, offset);
            AssertEqual(new byte[] { 0, 0, 0x74, 0, 0, 0x66, 0, 0, 0, 0 }, buffer);
        }

        [Fact]
        public void EncodeInteger()
        {
            var buffer = new byte[10];
            long offset = 2L;
            Encoder.EncodeInteger(0, buffer, ref offset);
            Assert.Equal(2L + 3L, offset);
            AssertEqual(new byte[] { 0, 0, 0x69, 0x30, 0x65, 0, 0, 0, 0, 0 }, buffer);

            Clear(buffer, 0, buffer.Length);
            offset = 1L;
            Encoder.EncodeInteger(-123, buffer, ref offset);
            Assert.Equal(1L + 6L, offset);
            AssertEqual(new byte[] { 0, 0x69, 0x2d, 0x31, 0x32, 0x33, 0x65, 0, 0, 0 }, buffer);

            Clear(buffer, 0, buffer.Length);
            offset = 4L;
            Encoder.EncodeInteger(456, buffer, ref offset);
            Assert.Equal(4L + 5L, offset);
            AssertEqual(new byte[] { 0, 0, 0, 0, 0x69, 0x34, 0x35, 0x36, 0x65, 0 }, buffer);
        }

        [Fact]
        public void EncodeBinary()
        {
            var buffer = new byte[20];
            long offset = 2L;
            Encoder.EncodeBinary(new Binary("hello world", Encoding.ASCII), buffer, ref offset);
            Assert.Equal(2L + 14L, offset);
            AssertEqual(
                new byte[20]
                {
                    0, 0,
                    0x31, 0x31, 0x3a,  // "11:"
                    0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x20,  // "hello "
                    0x77, 0x6f, 0x72, 0x6c, 0x64, // "world"
                    0, 0, 0, 0,
                },
                buffer
            );
        }

        [Fact]
        public void EncodeText()
        {
            var buffer = new byte[20];
            long offset = 5L;
            Encoder.EncodeText("한글", buffer, ref offset);
            Assert.Equal(5L + 9L, offset);
            AssertEqual(
                new byte[20]
                {
                    0, 0, 0, 0, 0,
                    0x75, 0x36, 0x3a,  // "u6:"
                    0xed, 0x95, 0x9c, 0xea, 0xb8, 0x80,  // "한글"
                    0, 0, 0, 0, 0, 0,
                },
                buffer
            );
        }

        [Fact]
        public void CountDecimalDigits()
        {
            for (long i = 0; i <= 1000L; i++)
            {
                Assert.Equal(
                    i.ToString(CultureInfo.InvariantCulture).Length,
                    Encoder.CountDecimalDigits(i)
                );
            }

            var random = new System.Random();
            for (int i = 0; i < 100; i++)
            {
                long n = (long)random.Next(0, int.MaxValue);
                Assert.Equal(
                    n.ToString(CultureInfo.InvariantCulture).Length,
                    Encoder.CountDecimalDigits(n)
                );
            }
        }

        [Fact]
        public void EncodeDigits()
        {
            var buffer = new byte[10];
            long offset = 2L;
            Encoder.EncodeDigits(0L, buffer, ref offset);
            Assert.Equal(2L + 1L, offset);
            AssertEqual(new byte[] { 0, 0, 0x30, 0, 0, 0, 0, 0, 0, 0 }, buffer);

            Clear(buffer, 0, buffer.Length);
            offset = 0L;
            Encoder.EncodeDigits(5L, buffer, ref offset);
            Assert.Equal(0L + 1L, offset);
            AssertEqual(new byte[] { 0x35, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, buffer);

            Clear(buffer, 0, buffer.Length);
            offset = 5L;
            Encoder.EncodeDigits(10L, buffer, ref offset);
            Assert.Equal(5L + 2L, offset);
            AssertEqual(new byte[] { 0, 0, 0, 0, 0, 0x31, 0x30, 0, 0, 0 }, buffer);

            Clear(buffer, 0, buffer.Length);
            offset = 6L;
            Encoder.EncodeDigits(123L, buffer, ref offset);
            Assert.Equal(6L + 3L, offset);
            AssertEqual(new byte[] { 0, 0, 0, 0, 0, 0, 0x31, 0x32, 0x33, 0 }, buffer);

            Clear(buffer, 0, buffer.Length);
            offset = 0L;
            Encoder.EncodeDigits(9876543210L, buffer, ref offset);
            Assert.Equal(0L + 10L, offset);
            AssertEqual(
                new byte[] { 0x39, 0x38, 0x37, 0x36, 0x35, 0x34, 0x33, 0x32, 0x31, 0x30 },
                buffer
            );
        }
    }
}
