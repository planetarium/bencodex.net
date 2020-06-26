using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using Bencodex.Types;
using Xunit;
using Xunit.Abstractions;

namespace Bencodex.Tests.Types
{
    public class ValueTests
    {
        private Codec _codec;

        public ValueTests()
        {
            _codec = new Codec();
        }

        [Fact]
        public void Null()
        {
            AssertEqual(
                new byte[] { 0x6e },  // "n"
                _codec.Encode(default(Null))
            );

            Assert.Equal("null", default(Null).Inspection);
            Assert.Equal("Bencodex.Types.Null", default(Null).ToString());
        }

        [Fact]
        public void Boolean()
        {
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

            Assert.Equal("true", t.Inspection);
            Assert.Equal("false", f.Inspection);
            Assert.Equal("Bencodex.Types.Boolean true", t.ToString());
            Assert.Equal("Bencodex.Types.Boolean false", f.ToString());
        }

        [Fact]
        public void Integer()
        {
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

            Assert.Equal("123", new Integer(123).Inspection);
            Assert.Equal("-456", new Integer(-456).Inspection);
            Assert.Equal("Bencodex.Types.Integer 123", new Integer(123).ToString());
            Assert.Equal("Bencodex.Types.Integer -456", new Integer(-456).ToString());
        }

        [Fact]
        public void Binary()
        {
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

            Assert.Equal("\"\"", empty.Inspection);
            Assert.Equal("\"\u4f60\u597d\"", nihao.Inspection);
            Assert.Equal(
                "\"new lines and\\n\\\"quotes\\\" become escaped to \\\\\"",
                complex.Inspection
            );
            Assert.Equal("Bencodex.Types.Text \"\"", empty.ToString());
            Assert.Equal("Bencodex.Types.Text \"\u4f60\u597d\"", nihao.ToString());
        }

        [Fact]
        public void List()
        {
            var zero = default(List);
            var two = new List(new Text[] { "hello", "world" }.Cast<IValue>());

            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                _codec.Encode(zero)
            );
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                _codec.Encode(new List(new IValue[0]))
            );
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                _codec.Encode(new List(ImmutableList<IValue>.Empty))
            );
            AssertEqual(
                new byte[]
                {
                    0x6c, 0x75, 0x35, 0x3a, 0x68, 0x65, 0x6c, 0x6c, 0x6f,
                    0x75, 0x35, 0x3a, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x65,

                    // "lu5:hellou5:worlde"
                },
                _codec.Encode(two)
            );
        }

        [Fact]
        public void Dictionary()
        {
            AssertEqual(
                new byte[] { 0x64, 0x65 },  // "de"
                _codec.Encode(
                    new Dictionary(ImmutableDictionary<IKey, IValue>.Empty)
                )
            );
            AssertEqual(
                new byte[] { 0x64, 0x65 },  // "de"
                _codec.Encode(
                    new Dictionary(new KeyValuePair<IKey, IValue>[0])
                )
            );
            AssertEqual(
                new byte[]
                {
                    0x64, 0x31, 0x3a, 0x63, 0x69, 0x31, 0x65,
                    0x75, 0x31, 0x3a, 0x61, 0x69, 0x32, 0x65,
                    0x75, 0x31, 0x3a, 0x62, 0x69, 0x33, 0x65, 0x65,

                    // "d1:ci1eu1:ai2eu1:bi3ee"
                },
                _codec.Encode(
                    new Dictionary(
                        new Dictionary<IKey, IValue>()
                        {
                            { (Text)"a", (Integer)2 },
                            { (Text)"b", (Integer)3 },
                            {
                                // "c" => 3
                                (Binary)new byte[] { 0x63 },
                                (Integer)1
                            },
                        }
                    )
                )
            );
        }

        [Theory]
        [ClassData(typeof(SpecTheoryData))]
        public void SpecTestSuite(Spec spec)
        {
            AssertEqual(
                spec.Encoding,
                _codec.Encode(spec.Semantics),
                spec.SemanticsPath
            );
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

        private void AssertEqual(
            byte[] expected,
            byte[] actual,
            string message = null
        )
        {
            Encoding utf8 = Encoding.GetEncoding(
                "UTF-8",
                new EncoderReplacementFallback(),
                new DecoderReplacementFallback()
            );
            Assert.True(
                expected.SequenceEqual(actual),
                string.Format(
                    "{4}{5}" +
                    "Expected: {0}\nActual:   {1}\n" +
                    "Expected (hex): {2}\nActual (hex):   {3}",
                    utf8.GetString(expected),
                    utf8.GetString(actual),
                    BitConverter.ToString(expected),
                    BitConverter.ToString(actual),
                    message ?? string.Empty,
                    message == null ? string.Empty : "\n"
                )
            );
        }
    }
}
