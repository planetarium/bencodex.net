using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public readonly ITestOutputHelper Output;

        public ValueTests(ITestOutputHelper output)
        {
            Output = output;
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
                String.Format(
                    "{4}{5}" +
                    "Expected: {0}\nActual:   {1}\n" +
                    "Expected (hex): {2}\nActual (hex):   {3}",
                    utf8.GetString(expected),
                    utf8.GetString(actual),
                    BitConverter.ToString(expected),
                    BitConverter.ToString(actual),
                    message ?? "",
                    message == null ? "" : "\n"
                )
            );
        }

        [Fact]
        public void Null()
        {
            AssertEqual(
                new byte[] { 0x6e },  // "n"
                new Null().EncodeIntoByteArray()
            );
        }

        [Fact]
        public void Boolean()
        {
            AssertEqual(
                new byte[] { 0x74 },  // "t"
                new Bencodex.Types.Boolean(true).EncodeIntoByteArray()
            );
            AssertEqual(
                new byte[] { 0x66 },  // "f"
                new Bencodex.Types.Boolean(false).EncodeIntoByteArray()
            );
        }

        private void IntegerGeneric(Func<int, Integer?> convert)
        {
            AssertEqual(
                new byte[] { 0x69, 0x31, 0x32, 0x33, 0x65 },  // "i123e"
                convert(123).EncodeIntoByteArray()
            );
            Integer? i = convert(-123);
            if (i != null)
            {
                AssertEqual(
                    new byte[]
                    {
                        0x69, 0x2d, 0x31, 0x32, 0x33, 0x65
                        // "i-123e"
                    },
                    i.EncodeIntoByteArray()
                );
            }
            AssertEqual(
                new byte[] { 0x69, 0x30, 0x65 },  // "i0e"
                convert(0).EncodeIntoByteArray()
            );
            i = convert(-0);
            if (i != null)
            {
                AssertEqual(
                    new byte[] { 0x69, 0x30, 0x65 },  // "i0e"
                    i.EncodeIntoByteArray()
                );
            }
        }

        [Fact]
        public void Integer()
        {
            IntegerGeneric(i => new Integer((short) i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((ushort) i) : (Integer?) null
            );
            IntegerGeneric(i => new Integer(i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((uint) i) : (Integer?) null
            );
            IntegerGeneric(i => new Integer((long) i));
            IntegerGeneric(
                i => i >= 0 ? new Integer((ulong) i) : (Integer?) null
            );
            IntegerGeneric(i => new Integer(new BigInteger(i)));
            IntegerGeneric(i => new Integer(i.ToString()));
        }

        [Fact]
        public void ByteString()
        {
            AssertEqual(
                new byte[] { 0x30, 0x3a },  // "0:"
                new Binary().EncodeIntoByteArray()
            );
            AssertEqual(
                new byte[] { 0x35, 0x3a, 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                // "5:hello"
                new Binary(
                    new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f }
                    // "hello"
                ).EncodeIntoByteArray()
            );
        }

        [Fact]
        public void UnicodeText()
        {
            AssertEqual(
                new byte[] { 0x75, 0x30, 0x3a },  // "u0:"
                new Text().EncodeIntoByteArray()
            );
            AssertEqual(
                new byte[]
                {
                    0x75, 0x36, 0x3a, 0xe4, 0xbd,
                    0xa0, 0xe5, 0xa5, 0xbd,
                    // "u6:\xe4\xbd\xa0\xe5\xa5\xbd"
                },
                new Text("\u4f60\u597d").EncodeIntoByteArray()  // "你好"
            );
        }

        [Fact]
        public void List()
        {
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                new List().EncodeIntoByteArray()
            );
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                new List(new IValue[0]).EncodeIntoByteArray()
            );
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                new List(ImmutableList<IValue>.Empty).EncodeIntoByteArray()
            );
            AssertEqual(
                new byte[]
                {
                    0x6c, 0x75, 0x35, 0x3a, 0x68, 0x65, 0x6c, 0x6c, 0x6f,
                    0x75, 0x35, 0x3a, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x65,
                    // "lu5:hellou5:worlde"
                },
                new List(
                    new Text[]
                    {
                        "hello",
                        "world",
                    }.Cast<IValue>()
                ).EncodeIntoByteArray()
            );
        }

        [Fact]
        public void Dictionary()
        {
            AssertEqual(
                new byte[] { 0x64, 0x65 },  // "de"
                new Dictionary(
                    ImmutableDictionary<IKey, IValue>.Empty
                ).EncodeIntoByteArray()
            );
            AssertEqual(
                new byte[] { 0x64, 0x65 },  // "de"
                new Dictionary(
                    new KeyValuePair<IKey, IValue>[0]
                ).EncodeIntoByteArray()
            );
            AssertEqual(
                new byte[]
                {
                    0x64, 0x31, 0x3a, 0x63, 0x69, 0x31, 0x65,
                    0x75, 0x31, 0x3a, 0x61, 0x69, 0x32, 0x65,
                    0x75, 0x31, 0x3a, 0x62, 0x69, 0x33, 0x65, 0x65,
                    // "d1:ci1eu1:ai2eu1:bi3ee"
                },
                new Dictionary(
                    new Dictionary<IKey, IValue>()
                    {
                        {(Text) "a", (Integer) 2},
                        {(Text) "b", (Integer) 3},
                        {(Binary) new byte[] { 0x63 }, (Integer) 1}  // "c" => 3
                    }
                ).EncodeIntoByteArray()
            );
        }

        [Fact]
        public void SpecTestSuite()
        {
            SpecData specData = SpecData.GetInstance();
            Output.WriteLine("Test suite path: {0}", specData.TestSuitePath);
            foreach (Spec spec in specData)
            {
                Output.WriteLine("");
                Output.WriteLine("Spec: {0}", spec);
                AssertEqual(
                    spec.Encoding,
                    spec.Semantics.EncodeIntoByteArray(),
                    spec.SemanticsPath
                );
            }
        }
    }
}
