using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using static Bencodex.Tests.TestUtils;
using ValueType = Bencodex.Types.ValueType;

namespace Bencodex.Tests.Types
{
    public class DictionaryTest
    {
        private readonly Codec _codec = new Codec();
        private readonly Dictionary _textKey = Dictionary.Empty.SetItem("foo", "bar");

        private readonly Dictionary _binaryKey = Dictionary.Empty
            .SetItem(Encoding.ASCII.GetBytes("foo"), "bar");

        private readonly Dictionary _mixedKeys = Dictionary.Empty
            .Add("stringKey", "string")
            .Add(new byte[] { 0x00 }, "byte");

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
            byte[] textKey = new byte[] { 0x00 };
            byte[] integerKey = new byte[] { 0x01 };
            byte[] binaryKey = new byte[] { 0x02 };
            byte[] booleanKey = new byte[] { 0x03 };
            byte[] listKey = new byte[] { 0x04 };
            var dictionary = Dictionary.Empty
                .Add("text", "foo")
                .Add("integer", 1337)
                .Add("binary", new byte[] { 0x01, 0x02, 0x03, 0x04 })
                .Add("boolean", true)
                .Add("list", new IValue[] { (Text)"bar", (Integer)1337 })
                .Add(textKey, "baz")
                .Add(integerKey, 2020)
                .Add(binaryKey, new byte[] { 0x05, 0x06, 0x07, 0x08 })
                .Add(booleanKey, false)
                .Add(listKey, new IValue[] { (Text)"qux", (Integer)2020 });

            // String keys
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

            // Byte array keys
            Assert.Equal("baz", (Text)dictionary[textKey]);
            Assert.Equal("baz", dictionary.GetValue<Text>(textKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(textKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(textKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(textKey));

            Assert.Equal((Integer)2020, (Integer)dictionary[integerKey]);
            Assert.Equal(
                (Integer)2020,
                dictionary.GetValue<Integer>(integerKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(integerKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(integerKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(integerKey));

            Assert.Equal(
                new byte[] { 0x05, 0x06, 0x07, 0x08 },
                (Binary)dictionary[binaryKey]);
            Assert.Equal(
                new byte[] { 0x05, 0x06, 0x07, 0x08 },
                dictionary.GetValue<Binary>(binaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(binaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(binaryKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Bencodex.Types.Boolean>(binaryKey));

            Assert.Equal(
                (Bencodex.Types.Boolean)false,
                (Bencodex.Types.Boolean)dictionary[booleanKey]);
            Assert.Equal(
                (Bencodex.Types.Boolean)false,
                dictionary.GetValue<Bencodex.Types.Boolean>(booleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(booleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(booleanKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(booleanKey));

            Assert.Equal(
                new IValue[] { (Text)"qux", (Integer)2020 },
                (Bencodex.Types.List)dictionary[listKey]);
            Assert.Equal(
                new IValue[] { (Text)"qux", (Integer)2020 },
                dictionary.GetValue<Bencodex.Types.List>(listKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Integer>(listKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Binary>(listKey));
            Assert.Throws<InvalidCastException>(
                () => dictionary.GetValue<Text>(listKey));
        }

        [Fact]
        public void ContainsKey()
        {
            byte[] byteKey = { 0x00 };
            byte[] invalidKey = { 0x01 };
            Assert.True(_mixedKeys.ContainsKey((IKey)(Text)"stringKey"));
            Assert.True(_mixedKeys.ContainsKey("stringKey"));
            Assert.True(_mixedKeys.ContainsKey((IKey)(Binary)byteKey));
            Assert.True(_mixedKeys.ContainsKey(byteKey));
            Assert.False(_mixedKeys.ContainsKey((IKey)(Text)"invalidKey"));
            Assert.False(_mixedKeys.ContainsKey("invalidKey"));
            Assert.False(_mixedKeys.ContainsKey((IKey)(Binary)invalidKey));
            Assert.False(_mixedKeys.ContainsKey(invalidKey));
        }

        [Fact]
        public void Type()
        {
            Assert.Equal(ValueType.Dictionary, Dictionary.Empty.Type);
            Assert.Equal(ValueType.Dictionary, _textKey.Type);
            Assert.Equal(ValueType.Dictionary, _binaryKey.Type);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(
                new Fingerprint(ValueType.Dictionary, 2),
                Dictionary.Empty.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueType.Dictionary,
                    14L,
                    ParseHex("bb1bbb4428e03722aa5e5ad2e0d70657e328dae1")
                ),
                _textKey.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueType.Dictionary,
                    13L,
                    ParseHex("0a4571a67289be466635ecc577ac136452d8d532")
                ),
                _binaryKey.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueType.Dictionary,
                    33,
                    ParseHex("83f7620e739e4dd9c6443a93eae4ff9132580ff3")
                ),
                _mixedKeys.Fingerprint
            );
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(2L, Dictionary.Empty.EncodingLength);
            Assert.Equal(14L, _textKey.EncodingLength);
            Assert.Equal(13L, _binaryKey.EncodingLength);
            Assert.Equal(33L, _mixedKeys.EncodingLength);
        }

        [Fact]
        public void Inspection()
        {
            Assert.Equal("{}", Dictionary.Empty.Inspection);

            Assert.Equal(
                "{\n  \"foo\": \"bar\",\n}",
                _textKey.Inspection
            );
            Assert.Equal(
                "{\n  b\"\\x66\\x6f\\x6f\": \"bar\",\n}",
                _binaryKey.Inspection
            );
            Assert.Equal(
                @"{
  ""baz"": {
    ""foo"": ""bar"",
  },
  ""foo"": ""bar"",
}".NoCr(),
                _textKey.SetItem("baz", _textKey).Inspection
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
    }
}
