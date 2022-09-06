using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Bencodex.Misc;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using static Bencodex.Tests.TestUtils;
using IEquatableDict = System.IEquatable<System.Collections.Immutable.IImmutableDictionary<
    Bencodex.Types.IKey,
    Bencodex.Types.IValue>>;

namespace Bencodex.Tests.Types
{
    public class DictionaryTest
    {
        private readonly Codec _codec = new Codec();
        private readonly Dictionary _textKey;
        private readonly Dictionary _binaryKey;
        private readonly Dictionary _mixedKeys;
        private readonly ImmutableArray<IValue> _offloadedValues;
        private readonly Dictionary<IKey, IndirectValue> _partiallyLoadedPairs;
        private readonly Dictionary _partiallyLoaded;
        private readonly Dictionary _loaded;
        private readonly List<Fingerprint> _loadLog;

        public DictionaryTest()
        {
            _textKey = Dictionary.Empty.SetItem("foo", "bar");
            _binaryKey = Dictionary.Empty
                .SetItem(Encoding.ASCII.GetBytes("foo"), "bar");
            _mixedKeys = Dictionary.Empty
                .Add("stringKey", "string")
                .Add(new byte[] { 0x00 }, "byte");
            _offloadedValues = ImmutableArray.Create<IValue>(
                Null.Value,
                new Integer(1234),
                _mixedKeys
            );
            _partiallyLoadedPairs = new Dictionary<IKey, IndirectValue>
            {
                [(Text)"unload0"] = new IndirectValue(_offloadedValues[0].Fingerprint),
                [(Text)"unload1"] = new IndirectValue(_offloadedValues[1].Fingerprint),
                [(Text)"unload2"] = new IndirectValue(_offloadedValues[2].Fingerprint),
                [(Text)"a"] = new IndirectValue(new Binary("foo", Encoding.ASCII)),
                [(Text)"b"] = new IndirectValue(new Text("baz")),
            };
            _partiallyLoaded = new Dictionary(_partiallyLoadedPairs, Loader);
            _loaded = Dictionary.Empty
                .Add("unload0", _offloadedValues[0])
                .Add("unload1", _offloadedValues[1])
                .Add("unload2", _offloadedValues[2])
                .Add("a", Encoding.ASCII.GetBytes("foo"))
                .Add("b", "baz");
            _loadLog = new List<Fingerprint>();
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

            Assert.True(_partiallyLoaded.Equals(_loaded));
            Assert.False(_partiallyLoaded.Equals(_loaded.Add("zzz", 1)));
            Assert.False(_partiallyLoaded.Equals(_loaded.SetItem("b", "update")));
            Assert.Empty(_loadLog);

            Assert.True(((IEquatableDict)_partiallyLoaded).Equals(_loaded));
            Assert.False(((IEquatableDict)_partiallyLoaded).Equals(_loaded.Add("zzz", 1)));
            Assert.False(((IEquatableDict)_partiallyLoaded).Equals(_loaded.SetItem("b", "update")));
            Assert.Empty(_loadLog);

            Assert.True(((IEquatableDict)_partiallyLoaded).Equals(_loaded.ToImmutableDictionary()));
            Assert.False(
                ((IEquatableDict)_partiallyLoaded).Equals(
                    _loaded.Add("zzz", 1).ToImmutableDictionary()
                )
            );
            Assert.False(
                ((IEquatableDict)_partiallyLoaded).Equals(
                    _loaded.SetItem("b", "update").ToImmutableDictionary()
                )
            );
            Assert.Empty(_loadLog);
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

            Assert.Equal(new Text("baz"), _partiallyLoaded["b"]);
            Assert.Empty(_loadLog);
            Assert.Equal(_offloadedValues[0], _partiallyLoaded["unload0"]);
            Assert.Single(_loadLog);
            Assert.Equal(_offloadedValues[1], _partiallyLoaded["unload1"]);
            Assert.Equal(2, _loadLog.Count);
            Assert.Equal(_offloadedValues[2], _partiallyLoaded["unload2"]);
            Assert.Equal(3, _loadLog.Count);
            Assert.Throws<KeyNotFoundException>(() => _partiallyLoaded["unload3"]);
        }

        [Fact]
        public void SetItem()
        {
            var dictionary = Dictionary.Empty
                .SetItem("text", "foo")
                .SetItem("integer", 1337)
                .SetItem("binary", new byte[] { 0x01, 0x02, 0x03, 0x04 })
                .SetItem("boolean", true)
                .SetItem("list", new IValue[] { (Text)"bar", (Integer)1337 });

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
            IValue[] sList = new IValue[] { (Text)"bar", (Integer)1337 };
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
            IValue[] bList = new IValue[] { (Text)"qux", (Integer)2020 };

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
                .Add(sListKey, new List(sList))
                .Add(bTextKey, (Text)bText)
                .Add(bIntKey, (Integer)bInt)
                .Add(bShortKey, (Integer)bShort)
                .Add(bBinaryKey, (Binary)bBinary)
                .Add(bBooleanKey, (Bencodex.Types.Boolean)bBoolean)
                .Add(bListKey, new List(bList));

            dictionary = Dictionary.Empty
                .Add((Text)sTextKey, (Text)sText)
                .Add((Text)sShortKey, (Integer)sShort)
                .Add((Text)sIntKey, (Integer)sInt)
                .Add((Text)sBinaryKey, (Binary)sBinary)
                .Add((Text)sBooleanKey, (Bencodex.Types.Boolean)sBoolean)
                .Add((Text)sListKey, new List(sList))
                .Add((Binary)bTextKey, (Text)bText)
                .Add((Binary)bIntKey, (Integer)bInt)
                .Add((Binary)bShortKey, (Integer)bShort)
                .Add((Binary)bBinaryKey, (Binary)bBinary)
                .Add((Binary)bBooleanKey, (Bencodex.Types.Boolean)bBoolean)
                .Add((Binary)bListKey, new List(bList));

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

            Assert.True(_partiallyLoaded.ContainsKey("unload0"));
            Assert.True(_partiallyLoaded.ContainsKey("unload1"));
            Assert.True(_partiallyLoaded.ContainsKey("unload2"));
            Assert.False(_partiallyLoaded.ContainsKey("unload3"));
            Assert.True(_partiallyLoaded.ContainsKey("a"));
            Assert.True(_partiallyLoaded.ContainsKey("b"));
            Assert.False(_partiallyLoaded.ContainsKey("c"));
            Assert.Empty(_loadLog);
        }

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.Dictionary, Dictionary.Empty.Kind);
            Assert.Equal(ValueKind.Dictionary, _textKey.Kind);
            Assert.Equal(ValueKind.Dictionary, _binaryKey.Kind);
            Assert.Equal(ValueKind.Dictionary, _partiallyLoaded.Kind);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(
                new Fingerprint(ValueKind.Dictionary, 2),
                Dictionary.Empty.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.Dictionary,
                    14L,
                    ParseHex("bb1bbb4428e03722aa5e5ad2e0d70657e328dae1")
                ),
                _textKey.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.Dictionary,
                    13L,
                    ParseHex("0a4571a67289be466635ecc577ac136452d8d532")
                ),
                _binaryKey.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.Dictionary,
                    33,
                    ParseHex("83f7620e739e4dd9c6443a93eae4ff9132580ff3")
                ),
                _mixedKeys.Fingerprint
            );

            Assert.Equal(_loaded.Fingerprint, _partiallyLoaded.Fingerprint);
            Assert.Empty(_loadLog);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(2L, Dictionary.Empty.EncodingLength);
            Assert.Equal(14L, _textKey.EncodingLength);
            Assert.Equal(13L, _binaryKey.EncodingLength);
            Assert.Equal(33L, _mixedKeys.EncodingLength);

            Assert.Equal(91L, _partiallyLoaded.EncodingLength);
            Assert.Empty(_loadLog);
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
        public void EnumerateIndirectValues()
        {
            Assert.All(
                _textKey.EnumerateIndirectPairs(),
                kv => Assert.NotNull(kv.Value.LoadedValue)
            );
            Assert.Equal(
                new[]
                {
                    new KeyValuePair<IKey, IndirectValue>(
                        (Text)"foo",
                        new IndirectValue((Text)"bar")
                    ),
                },
                _textKey.EnumerateIndirectPairs()
            );

            Assert.All(
                _textKey.EnumerateIndirectPairs(),
                kv => Assert.NotNull(kv.Value.LoadedValue)
            );
            Assert.Equal(
                new[]
                {
                    new KeyValuePair<IKey, IndirectValue>(
                        new Binary("foo", Encoding.ASCII),
                        new IndirectValue((Text)"bar")
                    ),
                },
                _binaryKey.EnumerateIndirectPairs()
            );

            Assert.All(
                _mixedKeys.EnumerateIndirectPairs(),
                kv => Assert.NotNull(kv.Value.LoadedValue)
            );
            Assert.Equal(
                new[]
                {
                    new KeyValuePair<IKey, IndirectValue>(
                        (Binary)new byte[] { 0 },
                        new IndirectValue((Text)"byte")
                    ),
                    new KeyValuePair<IKey, IndirectValue>(
                        (Text)"stringKey",
                        new IndirectValue((Text)"string")
                    ),
                },
                _mixedKeys.EnumerateIndirectPairs()
            );

            Assert.Equal(
                _partiallyLoadedPairs.OrderBy(kv => kv.Key, KeyComparer.Instance),
                _partiallyLoaded.EnumerateIndirectPairs()
            );
            foreach (var kv in _partiallyLoaded.EnumerateIndirectPairs())
            {
                Assert.Equal(_partiallyLoadedPairs[kv.Key].Fingerprint, kv.Value.Fingerprint);
                Assert.Equal(_partiallyLoadedPairs[kv.Key].LoadedValue, kv.Value.LoadedValue);
            }

            Assert.Empty(_loadLog);
        }

        private IValue Loader(Fingerprint f)
        {
            _loadLog.Add(f);
            return _offloadedValues.First(v => v.Fingerprint.Equals(f));
        }
    }
}
