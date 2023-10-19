using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class DictionaryTest
    {
        private readonly Codec _codec = new Codec();
        private readonly Dictionary _textKey;
        private readonly Dictionary _binaryKey;
        private readonly Dictionary _mixedKeys;
        private readonly Dictionary _loaded;

        public DictionaryTest()
        {
            _textKey = Dictionary.Empty.SetItem("foo", "bar");
            _binaryKey = Dictionary.Empty
                .SetItem(Encoding.ASCII.GetBytes("foo"), "bar");
            _mixedKeys = Dictionary.Empty
                .Add("stringKey", "string")
                .Add(new byte[] { 0x00 }, "byte");
            _loaded = Dictionary.Empty
                .Add("unload0", Null.Value)
                .Add("unload1", new Integer(1234))
                .Add("unload2", _mixedKeys)
                .Add("a", Encoding.ASCII.GetBytes("foo"))
                .Add("b", "baz");
        }

        [Fact]
        public void Constructors()
        {
            var a = new Dictionary(new[]
            {
                new KeyValuePair<IKey, IValue>(new Binary(0x61), new Integer(1)),
                new KeyValuePair<IKey, IValue>(new Binary(0x62), new Integer(2)),
                new KeyValuePair<IKey, IValue>(new Binary(0x63), new Integer(3)),
            });
            var b = new Dictionary(
                ImmutableDictionary<IKey, IValue>.Empty
                    .Add(new Binary(0x61), new Integer(1))
                    .Add(new Binary(0x62), new Integer(2))
                    .Add(new Binary(0x63), new Integer(3))
            );
            Assert.Equal(a, b);
        }

        [Fact]
        public void ParameterTypesForConstructors()
        {
            Dictionary<string, byte[]> stringBytesDict = new Dictionary<string, byte[]>
            {
                { "foo", new byte[] { 1, 2 } },
                { "bar", new byte[] { 3, 4, 5 } },
            };
            Dictionary<string, Binary> stringBinarydict = stringBytesDict.ToDictionary(
                pair => pair.Key, pair => (Binary)pair.Value);
            Dictionary<string, IValue> stringValueDict = stringBytesDict.ToDictionary(
                pair => pair.Key, pair => (IValue)(Binary)pair.Value);
            Dictionary<Text, byte[]> textBytesDict = stringBytesDict.ToDictionary(
                pair => (Text)pair.Key, pair => pair.Value);
            Dictionary<Text, Binary> textBinaryDict = stringBytesDict.ToDictionary(
                pair => (Text)pair.Key, pair => (Binary)pair.Value);
            Dictionary<Text, IValue> textValueDict = stringBytesDict.ToDictionary(
                pair => (Text)pair.Key, pair => (IValue)(Binary)pair.Value);

            Assert.Equal(new Dictionary(stringBytesDict), new Dictionary(stringBinarydict));
            Assert.Equal(new Dictionary(stringBytesDict), new Dictionary(stringValueDict));
            Assert.Equal(new Dictionary(stringBytesDict), new Dictionary(textBytesDict));
            Assert.Equal(new Dictionary(stringBytesDict), new Dictionary(textBinaryDict));
            Assert.Equal(new Dictionary(stringBytesDict), new Dictionary(textValueDict));

            Dictionary<byte[], int> bytesIntDict = new Dictionary<byte[], int>
            {
                { new byte[] { 1, 2, 3 }, 4 },
                { new byte[] { 5, 6, 7, 8 }, 9 },
            };
            Dictionary<byte[], Integer> bytesIntegerDict = bytesIntDict.ToDictionary(
                pair => pair.Key, pair => (Integer)pair.Value);
            Dictionary<byte[], IValue> bytesValueDict = bytesIntDict.ToDictionary(
                pair => pair.Key, pair => (IValue)(Integer)pair.Value);
            Dictionary<Binary, int> binaryIntDict = bytesIntDict.ToDictionary(
                pair => (Binary)pair.Key, pair => pair.Value);
            Dictionary<Binary, Integer> binaryIntegerDict = bytesIntDict.ToDictionary(
                pair => (Binary)pair.Key, pair => (Integer)pair.Value);
            Dictionary<Binary, IValue> binaryValueDict = bytesIntDict.ToDictionary(
                pair => (Binary)pair.Key, pair => (IValue)(Integer)pair.Value);

            Assert.Equal(new Dictionary(bytesIntDict), new Dictionary(bytesIntegerDict));
            Assert.Equal(new Dictionary(bytesIntDict), new Dictionary(bytesValueDict));
            Assert.Equal(new Dictionary(bytesIntDict), new Dictionary(binaryIntDict));
            Assert.Equal(new Dictionary(bytesIntDict), new Dictionary(binaryIntegerDict));
            Assert.Equal(new Dictionary(bytesIntDict), new Dictionary(binaryValueDict));
        }

        [Fact]
        public void Equality()
        {
            var a = new Dictionary(new KeyValuePair<IKey, IValue>[]
            {
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x61 }),  // 'a'
                    new Integer(1)
                ),
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x62 }),  // 'b'
                    new Integer(2)
                ),
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x63 }),  // 'c'
                    new Integer(3)
                ),
            });
            Assert.Equal(a, a);
            Assert.Equal<IValue>(a, a);

            var a2 = new Dictionary(new KeyValuePair<IKey, IValue>[]
            {
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x62 }),  // 'b'
                    new Integer(2)
                ),
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x63 }),  // 'c'
                    new Integer(3)
                ),
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x61 }),  // 'a'
                    new Integer(1)
                ),
            });
            Assert.Equal(a, a2);
            Assert.Equal<IValue>(a, a2);

            var b = new Dictionary(new KeyValuePair<IKey, IValue>[]
            {
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x61 }),  // 'a'
                    new Integer(2) // diffrent
                ),
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x62 }),  // 'b'
                    new Integer(2)
                ),
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x63 }),  // 'c'
                    new Integer(3)
                ),
            });
            Assert.NotEqual(a, b);
            Assert.NotEqual<IValue>(a, b);

            var c = new Dictionary(new KeyValuePair<IKey, IValue>[]
            {
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x61 }),  // 'a'
                    new Integer(2)
                ),
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x62 }),  // 'b'
                    new Integer(2)
                ),
                new KeyValuePair<IKey, IValue>(
                    new Binary(new byte[] { 0x64 }),  // 'd' -- different
                    new Integer(3)
                ),
            });
            Assert.NotEqual(a, c);
            Assert.NotEqual<IValue>(a, c);

            Assert.NotEqual<IValue>(Null.Value, a);
            Assert.NotEqual<IValue>(Null.Value, b);
            Assert.NotEqual<IValue>(Null.Value, c);
        }

        [Fact]
        public void Indexers()
        {
            Assert.Equal(new Text("bar"), _textKey[(IKey)new Text("foo")]);
            Assert.Equal(new Text("bar"), _textKey[new Text("foo")]);
            Assert.Equal(new Text("bar"), _textKey["foo"]);

            Assert.Equal(new Text("bar"), _binaryKey[(IKey)new Binary("foo", Encoding.ASCII)]);
            Assert.Equal(new Text("bar"), _binaryKey[new Binary("foo", Encoding.ASCII)]);
            Assert.Equal(new Text("bar"), _binaryKey[Encoding.ASCII.GetBytes("foo")]);

            Assert.Throws<KeyNotFoundException>(
                () => _textKey[(IKey)new Binary("foo", Encoding.ASCII)]
            );
            Assert.Throws<KeyNotFoundException>(() => _textKey[new Binary("foo", Encoding.ASCII)]);
            Assert.Throws<KeyNotFoundException>(() => _textKey[Encoding.ASCII.GetBytes("foo")]);

            Assert.Throws<KeyNotFoundException>(() => _binaryKey[(IKey)new Text("foo")]);
            Assert.Throws<KeyNotFoundException>(() => _binaryKey[new Text("foo")]);
            Assert.Throws<KeyNotFoundException>(() => _binaryKey["foo"]);
        }

        [Fact]
        public void SetItem()
        {
            // NOTE: Assigned multiple times with the same values for checking syntax.
            var dictionary = Dictionary.Empty
                .SetItem("text", "foo")
                .SetItem("integer", 1337)
                .SetItem("binary", new byte[] { 0x01, 0x02, 0x03, 0x04 })
                .SetItem("boolean", true)
                .SetItem("list", new List(new IValue[] { (Text)"bar", (Integer)1337 }));

            dictionary = Dictionary.Empty
                .SetItem((Text)"text", "foo")
                .SetItem((Text)"integer", 1337)
                .SetItem((Text)"binary", new byte[] { 0x01, 0x02, 0x03, 0x04 })
                .SetItem((Text)"boolean", true)
                .SetItem((Text)"list", new List(new IValue[] { (Text)"bar", (Integer)1337 }));

            dictionary = Dictionary.Empty
                .SetItem("text", (Text)"foo")
                .SetItem("integer", (Integer)1337)
                .SetItem("binary", (Binary)new byte[] { 0x01, 0x02, 0x03, 0x04 })
                .SetItem("boolean", (Bencodex.Types.Boolean)true)
                .SetItem("list", new List(new IValue[] { (Text)"bar", (Integer)1337 }));

            dictionary = Dictionary.Empty
                .SetItem((Text)"text", (Text)"foo")
                .SetItem((Text)"integer", (Integer)1337)
                .SetItem((Text)"binary", (Binary)new byte[] { 0x01, 0x02, 0x03, 0x04 })
                .SetItem((Text)"boolean", (Bencodex.Types.Boolean)true)
                .SetItem((Text)"list", new List(new IValue[] { (Text)"bar", (Integer)1337 }));

            Assert.Equal("foo", (Text)dictionary["text"]);
            Assert.Equal("foo", dictionary.GetValue<Text>("text"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>("text"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>("text"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>("text"));

            Assert.Equal((Integer)1337, (Integer)dictionary["integer"]);
            Assert.Equal(
                (Integer)1337,
                dictionary.GetValue<Integer>("integer"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>("integer"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>("integer"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>("integer"));

            Assert.Equal(
                new byte[] { 0x01, 0x02, 0x03, 0x04 },
                (Binary)dictionary["binary"]);
            Assert.Equal(
                new byte[] { 0x01, 0x02, 0x03, 0x04 },
                dictionary.GetValue<Binary>("binary"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>("binary"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>("binary"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>("binary"));

            Assert.Equal(
                (Bencodex.Types.Boolean)true,
                (Bencodex.Types.Boolean)dictionary["boolean"]);
            Assert.Equal(
                (Bencodex.Types.Boolean)true,
                dictionary.GetValue<Bencodex.Types.Boolean>("boolean"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>("boolean"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>("boolean"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>("boolean"));

            Assert.Equal(
                new IValue[] { (Text)"bar", (Integer)1337 },
                (Bencodex.Types.List)dictionary["list"]);
            Assert.Equal(
                new IValue[] { (Text)"bar", (Integer)1337 },
                dictionary.GetValue<Bencodex.Types.List>("list"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>("list"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>("list"));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>("list"));
        }

        [Fact]
        public void Add()
        {
            string sTextKey = "text";
            string sText = "foo";
            string sShortKey = "short";
            short sShort = 123;
            string sIntKey = "integer";
            int sInt = 123_456;
            string sBinaryKey = "binary";
            byte[] sBinary = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            string sBooleanKey = "boolean";
            bool sBoolean = true;
            string sListKey = "list";
            List sList = new List(new IValue[] { (Text)"bar", (Integer)1337 });
            byte[] bTextKey = new byte[] { 0x00 };
            string bText = "baz";
            byte[] bShortKey = new byte[] { 0x01 };
            short bShort = 321;
            byte[] bIntKey = new byte[] { 0x02 };
            int bInt = 654_321;
            byte[] bBinaryKey = new byte[] { 0x03 };
            byte[] bBinary = new byte[] { 0x05, 0x06, 0x07, 0x08 };
            byte[] bBooleanKey = new byte[] { 0x04 };
            bool bBoolean = false;
            byte[] bListKey = new byte[] { 0x05 };
            List bList = new List(new IValue[] { (Text)"qux", (Integer)2020 });

            // NOTE: Assigned multiple times with the same values for checking syntax.
            var dictionary = Dictionary.Empty
                .Add(sTextKey, sText)
                .Add(sShortKey, sShort)
                .Add(sIntKey, sInt)
                .Add(sBinaryKey, sBinary)
                .Add(sBooleanKey, sBoolean)
                .Add(sListKey, sList)
                .Add(bTextKey, bText)
                .Add(bIntKey, bInt)
                .Add(bShortKey, bShort)
                .Add(bBinaryKey, bBinary)
                .Add(bBooleanKey, bBoolean)
                .Add(bListKey, bList);

            dictionary = Dictionary.Empty
                .Add((Text)sTextKey, sText)
                .Add((Text)sShortKey, sShort)
                .Add((Text)sIntKey, sInt)
                .Add((Text)sBinaryKey, sBinary)
                .Add((Text)sBooleanKey, sBoolean)
                .Add((Text)sListKey, sList)
                .Add((Binary)bTextKey, bText)
                .Add((Binary)bIntKey, bInt)
                .Add((Binary)bShortKey, bShort)
                .Add((Binary)bBinaryKey, bBinary)
                .Add((Binary)bBooleanKey, bBoolean)
                .Add((Binary)bListKey, bList);

            dictionary = Dictionary.Empty
                .Add(sTextKey, (Text)sText)
                .Add(sShortKey, (Integer)sShort)
                .Add(sIntKey, (Integer)sInt)
                .Add(sBinaryKey, (Binary)sBinary)
                .Add(sBooleanKey, (Bencodex.Types.Boolean)sBoolean)
                .Add(sListKey, sList)
                .Add(bTextKey, (Text)bText)
                .Add(bIntKey, (Integer)bInt)
                .Add(bShortKey, (Integer)bShort)
                .Add(bBinaryKey, (Binary)bBinary)
                .Add(bBooleanKey, (Bencodex.Types.Boolean)bBoolean)
                .Add(bListKey, bList);

            dictionary = Dictionary.Empty
                .Add((Text)sTextKey, (Text)sText)
                .Add((Text)sShortKey, (Integer)sShort)
                .Add((Text)sIntKey, (Integer)sInt)
                .Add((Text)sBinaryKey, (Binary)sBinary)
                .Add((Text)sBooleanKey, (Bencodex.Types.Boolean)sBoolean)
                .Add((Text)sListKey, sList)
                .Add((Binary)bTextKey, (Text)bText)
                .Add((Binary)bIntKey, (Integer)bInt)
                .Add((Binary)bShortKey, (Integer)bShort)
                .Add((Binary)bBinaryKey, (Binary)bBinary)
                .Add((Binary)bBooleanKey, (Bencodex.Types.Boolean)bBoolean)
                .Add((Binary)bListKey, bList);

            // String keys
            Assert.Equal(sText, (Text)dictionary[sTextKey]);
            Assert.Equal(sText, dictionary.GetValue<Text>(sTextKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(sTextKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(sTextKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(sTextKey));

            Assert.Equal((Integer)sShort, (Integer)dictionary[sShortKey]);
            Assert.Equal(
                (Integer)sShort,
                dictionary.GetValue<Integer>(sShortKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(sShortKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(sShortKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(sShortKey));

            Assert.Equal((Integer)sInt, (Integer)dictionary[sIntKey]);
            Assert.Equal(
                (Integer)sInt,
                dictionary.GetValue<Integer>(sIntKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(sIntKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(sIntKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(sIntKey));

            Assert.Equal(
                sBinary,
                (Binary)dictionary[sBinaryKey]);
            Assert.Equal(
                sBinary,
                dictionary.GetValue<Binary>(sBinaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(sBinaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(sBinaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(sBinaryKey));

            Assert.Equal(
                (Bencodex.Types.Boolean)sBoolean,
                (Bencodex.Types.Boolean)dictionary[sBooleanKey]);
            Assert.Equal(
                (Bencodex.Types.Boolean)sBoolean,
                dictionary.GetValue<Bencodex.Types.Boolean>(sBooleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(sBooleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(sBooleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(sBooleanKey));

            Assert.Equal(
                sList,
                (Bencodex.Types.List)dictionary[sListKey]);
            Assert.Equal(
                sList,
                dictionary.GetValue<Bencodex.Types.List>(sListKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(sListKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(sListKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(sListKey));

            // Byte array keys
            Assert.Equal(bText, (Text)dictionary[bTextKey]);
            Assert.Equal(bText, dictionary.GetValue<Text>(bTextKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(bTextKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(bTextKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(bTextKey));

            Assert.Equal((Integer)bShort, (Integer)dictionary[bShortKey]);
            Assert.Equal(
                (Integer)bShort,
                dictionary.GetValue<Integer>(bShortKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(bShortKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(bShortKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(bShortKey));

            Assert.Equal((Integer)bInt, (Integer)dictionary[bIntKey]);
            Assert.Equal(
                (Integer)bInt,
                dictionary.GetValue<Integer>(bIntKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(bIntKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(bIntKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(bIntKey));

            Assert.Equal(
                bBinary,
                (Binary)dictionary[bBinaryKey]);
            Assert.Equal(
                bBinary,
                dictionary.GetValue<Binary>(bBinaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(bBinaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(bBinaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(bBinaryKey));

            Assert.Equal(
                (Bencodex.Types.Boolean)bBoolean,
                (Bencodex.Types.Boolean)dictionary[bBooleanKey]);
            Assert.Equal(
                (Bencodex.Types.Boolean)bBoolean,
                dictionary.GetValue<Bencodex.Types.Boolean>(bBooleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(bBooleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(bBooleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(bBooleanKey));

            Assert.Equal(
                bList,
                (Bencodex.Types.List)dictionary[bListKey]);
            Assert.Equal(
                bList,
                dictionary.GetValue<Bencodex.Types.List>(bListKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(bListKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(bListKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(bListKey));
        }

        [Fact]
        public void ContainsKey()
        {
            byte[] byteKey = { 0x00 };
            byte[] invalidKey = { 0x01 };
            Assert.True(_mixedKeys.ContainsKey((IKey)(Text)"stringKey"));
            Assert.True(_mixedKeys.ContainsKey((Text)"stringKey"));
            Assert.True(_mixedKeys.ContainsKey("stringKey"));
            Assert.True(_mixedKeys.ContainsKey((IKey)(Binary)byteKey));
            Assert.True(_mixedKeys.ContainsKey((Binary)byteKey));
            Assert.True(_mixedKeys.ContainsKey(byteKey));
            Assert.False(_mixedKeys.ContainsKey((IKey)(Text)"invalidKey"));
            Assert.False(_mixedKeys.ContainsKey((Text)"invalidKey"));
            Assert.False(_mixedKeys.ContainsKey("invalidKey"));
            Assert.False(_mixedKeys.ContainsKey((IKey)(Binary)invalidKey));
            Assert.False(_mixedKeys.ContainsKey((Binary)invalidKey));
            Assert.False(_mixedKeys.ContainsKey(invalidKey));
        }

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.Dictionary, Dictionary.Empty.Kind);
            Assert.Equal(ValueKind.Dictionary, _textKey.Kind);
            Assert.Equal(ValueKind.Dictionary, _binaryKey.Kind);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(2L, Dictionary.Empty.EncodingLength);
            Assert.Equal(14L, _textKey.EncodingLength);
            Assert.Equal(13L, _binaryKey.EncodingLength);
            Assert.Equal(33L, _mixedKeys.EncodingLength);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Inspect(bool loadAll)
        {
            Assert.Equal("{}", Dictionary.Empty.Inspect(loadAll));

            Assert.Equal(
                "{\n  \"foo\": \"bar\",\n}",
                _textKey.Inspect(loadAll)
            );
            Assert.Equal(
                "{\n  b\"\\x66\\x6f\\x6f\": \"bar\",\n}",
                _binaryKey.Inspect(loadAll)
            );
            Assert.Equal(
                @"{
  ""baz"": {
    ""foo"": ""bar"",
  },
  ""foo"": ""bar"",
}".NoCr(),
                _textKey.SetItem("baz", _textKey).Inspect(loadAll)
            );
        }

        [Fact]
        public void String()
        {
            Assert.Equal(
                "Bencodex.Types.Dictionary {}",
                Dictionary.Empty.ToString()
            );
            Assert.Equal(
                "Bencodex.Types.Dictionary {\n  \"foo\": \"bar\",\n}",
                _textKey.ToString()
            );
        }

        [Fact]
        public void Encode()
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

        [Fact]
        public void EncodedMustBeOrdered()
        {
            Codec codec = new Codec();
            string valid = "d3:bar4:spam3:fooi42ee";
            string invalid = "d3:fooi42e3:bar4:spame";
            byte[] validEncoded = Encoding.ASCII.GetBytes(valid);
            byte[] invalidEncoded = Encoding.ASCII.GetBytes(invalid);

            IValue decoded = codec.Decode(validEncoded);
            Assert.IsType<Dictionary>(decoded);
            Assert.Throws<DecodingException>(() => codec.Decode(invalidEncoded));
        }

        [Fact]
        public void HashCode()
        {
            Assert.Equal(
                _textKey.GetHashCode(),
                Dictionary.Empty.SetItem("foo", "bar").GetHashCode());
            Assert.Equal(
                _binaryKey.GetHashCode(),
                Dictionary.Empty.SetItem(Encoding.ASCII.GetBytes("foo"), "bar").GetHashCode());
            Assert.Equal(
                _mixedKeys.GetHashCode(),
                Dictionary.Empty
                    .Add("stringKey", "string")
                    .Add(new byte[] { 0x00 }, "byte").GetHashCode());

            var added = _mixedKeys.Add("baz", "qux");
            Assert.NotEqual(_mixedKeys.GetHashCode(), added.GetHashCode());
            Assert.Equal(added.GetHashCode(), _mixedKeys.Add("baz", "qux").GetHashCode());
            Assert.Equal(_mixedKeys.GetHashCode(), added.Remove(new Text("baz")).GetHashCode());

            Assert.NotEqual(
                Dictionary.Empty.Add("type_id", 0).Add("values", List.Empty).GetHashCode(),
                Dictionary.Empty.Add("type_id", 1).Add("values", "foo").GetHashCode()
            );
        }
    }
}
