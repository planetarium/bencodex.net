using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;
using System.Text;
using Xunit.Abstractions;

namespace Bencodex.Tests
{
    public class SerializerTests
    {
        public readonly ITestOutputHelper Output;

        public SerializerTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private byte[] Serialize(object o)
        {
            var stream = new MemoryStream();
            var serializer = new Serializer();
            serializer.Serialize(o, stream);
            stream.Flush();
            return stream.ToArray();
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
                Serialize(null)
            );
        }

        [Fact]
        public void True()
        {
            AssertEqual(
                new byte[] { 0x74 },  // "t"
                Serialize(true)
            );
        }

        [Fact]
        public void False()
        {
            AssertEqual(
                new byte[] { 0x66 },  // "f"
                Serialize(false)
            );
        }

        [Fact]
        public void Integers()
        {
            Integer<short>(123, -456, 0);
            Integer<int>(123, -456, 0);
            Integer<long>(123, -456, 0);
            Integer<ushort>(123, 0, 0);
            Integer<uint>(123, 0, 0);
            Integer<ulong>(123, 0, 0);
            Integer(
                new BigInteger(123),
                new BigInteger(-456),
                new BigInteger(0)
            );
        }

        private void Integer<T>(T a, T b, T zero) where T : IComparable<T>
        {
            AssertEqual(
                new byte[] { 0x69, 0x31, 0x32, 0x33, 0x65 },  // "i123e"
                Serialize(a)
            );
            if (b.CompareTo(zero) < 0)
            {
                AssertEqual(
                    new byte[] { 0x69, 0x2d, 0x34, 0x35, 0x36, 0x65 },
                    // "i-456e"
                    Serialize(b)
                );
            }
            AssertEqual(
                new byte[] { 0x69, 0x30, 0x65 },  // "i0e"
                Serialize(zero)
            );
        }

        [Fact]
        public void ByteString()
        {
            AssertEqual(
                new byte[] { 0x30, 0x3a },  // "0:"
                Serialize(new byte[] { })
            );
            AssertEqual(
                new byte[] { 0x35, 0x3a, 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                // "5:hello"
                Serialize(new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f })
                // "hello"
            );
        }

        [Fact]
        public void UnicodeText()
        {
            AssertEqual(
                new byte[] { 0x75, 0x30, 0x3a },  // "u0:"
                Serialize("")
            );
            AssertEqual(
                new byte[]
                {
                    0x75, 0x36, 0x3a, 0xe4, 0xbd,
                    0xa0, 0xe5, 0xa5, 0xbd,
                    // "u6:\xe4\xbd\xa0\xe5\xa5\xbd"
                },
                Serialize("\u4f60\u597d")  // "你好"
            );
        }

        [Fact]
        public void List()
        {
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                Serialize(new string[] {})
            );
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                Serialize(new List<int>())
            );
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                Serialize(new ArrayList())
            );
            AssertEqual(
                new byte[]
                {
                    0x6c, 0x75, 0x35, 0x3a, 0x68, 0x65, 0x6c, 0x6c, 0x6f,
                    0x75, 0x35, 0x3a, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x65,
                    // "lu5:hellou5:worlde"
                },
                Serialize(new string[] { "hello", "world" })
            );
            AssertEqual(
                new byte[]
                {
                    0x6c, 0x35, 0x3a, 0x68, 0x65, 0x6c, 0x6c, 0x6f,
                    0x35, 0x3a, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x65,
                    // "l5:hellou5:worlde"
                },
                Serialize(
                    new List<object>
                    {
                        new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f },
                        // "hello"
                        new byte[] { 0x77, 0x6f, 0x72, 0x6c, 0x64 },
                        // "world"
                    }
                )
            );
        }

        [Fact]
        public void Dictionary()
        {
            AssertEqual(
                new byte[] { 0x64, 0x65 },  // "de"
                Serialize(new Hashtable())
            );
            AssertEqual(
                new byte[] { 0x64, 0x65 },  // "de"
                Serialize(new Dictionary<object, int>())
            );
            AssertEqual(
                new byte[]
                {
                    0x64, 0x31, 0x3a, 0x63, 0x69, 0x31, 0x65,
                    0x75, 0x31, 0x3a, 0x61, 0x69, 0x32, 0x65,
                    0x75, 0x31, 0x3a, 0x62, 0x69, 0x33, 0x65, 0x65,
                    // "d1:ci1eu1:ai2eu1:bi3ee"
                },
                Serialize(
                    new Hashtable
                    {
                        {"a", 2},
                        {"b", 3},
                        {new byte[] { 0x63 }, 1}  // "c" => 3
                    }
                )
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
                    Serialize(spec.Semantics),
                    spec.SemanticsPath
                );
            }
        }
    }
}
