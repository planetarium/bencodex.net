using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using Bencodex.Types;
using Xunit;
using Xunit.Abstractions;
using static Bencodex.Tests.TestUtils;
using ValueType = Bencodex.Types.ValueType;

namespace Bencodex.Tests.Types
{
    public class ValueTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Codec _codec;

        public ValueTests(ITestOutputHelper output)
        {
            _output = output;
            _codec = new Codec();
        }

        [Fact]
        public void Null()
        {
            // FIXME: Move to NullTest.
            AssertEqual(
                new byte[] { 0x6e },  // "n"
                _codec.Encode(default(Null))
            );

            Assert.Equal(ValueType.Null, default(Null).Type);
            Assert.Equal(new Fingerprint(ValueType.Null, 1), default(Null).Fingerprint);
            Assert.Equal(1L, default(Null).EncodingLength);
            Assert.Equal("null", default(Null).Inspection);
            Assert.Equal("Bencodex.Types.Null", default(Null).ToString());
        }

        [Fact]
        public void Boolean()
        {
            // FIXME: Move to BooleanTest.
            var t = new Bencodex.Types.Boolean(true);
            var f = new Bencodex.Types.Boolean(false);
            AssertEqual(
                new byte[] { 0x74 },  // "t"
                _codec.Encode(t)
            );
            AssertEqual(
                new byte[] { 0x66 },  // "f"
                _codec.Encode(f)
            );

            Assert.Equal(ValueType.Boolean, t.Type);
            Assert.Equal(ValueType.Boolean, f.Type);
            Assert.Equal(new Fingerprint(ValueType.Boolean, 1, new byte[] { 1 }), t.Fingerprint);
            Assert.Equal(new Fingerprint(ValueType.Boolean, 1, new byte[] { 0 }), f.Fingerprint);
            Assert.Equal(1L, t.EncodingLength);
            Assert.Equal(1L, f.EncodingLength);
            Assert.Equal("true", t.Inspection);
            Assert.Equal("false", f.Inspection);
            Assert.Equal("Bencodex.Types.Boolean true", t.ToString());
            Assert.Equal("Bencodex.Types.Boolean false", f.ToString());
        }

        [Fact]
        public void Integer()
        {
            // FIXME: Move to IntegerTest.
            IntegerGeneric(i => new Integer((short)i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((ushort)i) : (Integer?)null
            );
            IntegerGeneric(i => new Integer(i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((uint)i) : (Integer?)null
            );
            IntegerGeneric(i => new Integer((long)i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((ulong)i) : (Integer?)null
            );
            IntegerGeneric(i => new Integer(new BigInteger(i)));
            IntegerGeneric(i => new Integer(i.ToString()));
            var locale = new CultureInfo("ar-SA");
            IntegerGeneric(i => new Integer(i.ToString(locale), locale));

            Assert.Equal(ValueType.Integer, new Integer(0).Type);
            Assert.Equal(ValueType.Integer, new Integer(123).Type);
            Assert.Equal(ValueType.Integer, new Integer(-456).Type);
            Assert.Equal(3L, new Integer(0).EncodingLength);
            Assert.Equal(5L, new Integer(123).EncodingLength);
            Assert.Equal(6L, new Integer(-456).EncodingLength);
            Assert.Equal("123", new Integer(123).Inspection);
            Assert.Equal("-456", new Integer(-456).Inspection);
            Assert.Equal("Bencodex.Types.Integer 123", new Integer(123).ToString());
            Assert.Equal("Bencodex.Types.Integer -456", new Integer(-456).ToString());
        }

        [Fact]
        public void Binary()
        {
            // FIXME: Move to BinaryTest.
            var empty = default(Binary);
            var hello = new Binary(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f });

            AssertEqual(
                new byte[] { 0x30, 0x3a },  // "0:"
                _codec.Encode(empty)
            );
            AssertEqual(
                new byte[] { 0x35, 0x3a, 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                _codec.Encode(hello) // "5:hello"
            );
        }

        [Fact]
        public void UnicodeText()
        {
            // FIXME: Move to TextTest.
            var empty = default(Text);
            var nihao = new Text("\u4f60\u597d");

            AssertEqual(
                new byte[] { 0x75, 0x30, 0x3a },  // "u0:"
                _codec.Encode(empty)
            );
            AssertEqual(
                new byte[]
                {
                    0x75, 0x36, 0x3a, 0xe4, 0xbd,
                    0xa0, 0xe5, 0xa5, 0xbd,

                    // "u6:\xe4\xbd\xa0\xe5\xa5\xbd"
                },
                _codec.Encode(nihao) // "你好"
            );

            var complex = new Text("new lines and\n\"quotes\" become escaped to \\");

            Assert.Equal(ValueType.Text, empty.Type);
            Assert.Equal(ValueType.Text, nihao.Type);
            Assert.Equal(ValueType.Text, complex.Type);
            Assert.Equal(3L, empty.EncodingLength);
            Assert.Equal(9L, nihao.EncodingLength);
            Assert.Equal(46L, complex.EncodingLength);
            Assert.Equal("\"\"", empty.Inspection);
            Assert.Equal("\"\u4f60\u597d\"", nihao.Inspection);
            Assert.Equal(
                "\"new lines and\\n\\\"quotes\\\" become escaped to \\\\\"",
                complex.Inspection
            );
            Assert.Equal("Bencodex.Types.Text \"\"", empty.ToString());
            Assert.Equal("Bencodex.Types.Text \"\u4f60\u597d\"", nihao.ToString());
        }

        [Theory]
        [ClassData(typeof(SpecTheoryData))]
        public void SpecTestSuite(Spec spec)
        {
            _output.WriteLine("YAML: {0}", spec.SemanticsPath);
            _output.WriteLine("Data: {0}", spec.EncodingPath);
            AssertEqual(
                spec.Encoding,
                _codec.Encode(spec.Semantics),
                spec.SemanticsPath
            );
            Assert.Equal(spec.Encoding.LongLength, spec.Semantics.EncodingLength);
        }

        private void IntegerGeneric(Func<int, Integer?> convert)
        {
            AssertEqual(
                new byte[] { 0x69, 0x31, 0x32, 0x33, 0x65 },  // "i123e"
                _codec.Encode(convert(123))
            );
            Integer? i = convert(-123);
            if (i != null)
            {
                AssertEqual(
                    new byte[]
                    {
                        // "i-123e"
                        0x69, 0x2d, 0x31, 0x32, 0x33, 0x65,
                    },
                    _codec.Encode(i)
                );
            }

            AssertEqual(
                new byte[] { 0x69, 0x30, 0x65 },  // "i0e"
                _codec.Encode(convert(0))
            );
            i = convert(-0);
            if (i != null)
            {
                AssertEqual(
                    new byte[] { 0x69, 0x30, 0x65 },  // "i0e"
                    _codec.Encode(i)
                );
            }
        }
    }
}
