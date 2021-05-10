using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Bencodex.Declarative;
using Bencodex.Types;
using Xunit;
using Xunit.Sdk;

namespace Bencodex.Tests.Declarative
{
    public class BencodexSerializerTest
    {
        private static readonly Dictionary<string, InnerStruct>
            DictionaryFixture = new Dictionary<string, InnerStruct>
            {
                ["first"] = new InnerStruct { String = "first" },
                ["second"] = new InnerStruct { String = "second" },
            };

        private static readonly List<InnerStruct> ListFixture = new List<InnerStruct>
        {
            new InnerStruct { String = "first" },
            new InnerStruct { String = "second" },
        };

        private static readonly Bencodex.Types.Dictionary BencodexDictionaryFixture =
            Bencodex.Types.Dictionary.Empty
                .Add(
                    "first",
                    Bencodex.Types.Dictionary.Empty.Add(
                        "string",
                        "first"))
                .Add(
                    "second",
                    Bencodex.Types.Dictionary.Empty.Add(
                        "string",
                        "second"));

        private static readonly Bencodex.Types.List BencodexListFixture =
            new Bencodex.Types.List(
                new IValue[]
                {
                    Bencodex.Types.Dictionary.Empty
                        .Add("string", "first"),
                    Bencodex.Types.Dictionary.Empty
                        .Add("string", "second"),
                }
            );

        private static readonly Struct Fixture = new Struct
        {
            String = "string",
            ByteArray = new byte[] { 0x01, 0x02, 0x03, 0x04 },
            Bool = true,
            Int16 = short.MaxValue,
            Int32 = int.MaxValue,
            Int64 = long.MaxValue,
            UInt16 = ushort.MaxValue,
            UInt32 = uint.MaxValue,
            UInt64 = ulong.MaxValue,

            Dictionary = DictionaryFixture,
            ImmutableDictionary = DictionaryFixture.ToImmutableDictionary(),
            List = ListFixture,
            ImmutableList = ListFixture.ToImmutableList(),

            Text = "text",
            Binary = new byte[] { 0x05, 0x06, 0x07, 0x08 },
            Boolean = true,
            Integer = ulong.MaxValue,
            BencodexDictionary = Bencodex.Types.Dictionary.Empty,
            BencodexList = default(Bencodex.Types.List),

            InnerStruct = new InnerStruct
            {
                String = "string",
            },

            NotAnnotatedField = "string",
        };

        private static readonly Bencodex.Types.Dictionary SerializedFixture =
            Bencodex.Types.Dictionary.Empty
                .SetItem("string", "string")
                .SetItem("byte_array", new byte[] { 0x01, 0x02, 0x03, 0x04 })
                .SetItem("bool", true)
                .SetItem("int16", short.MaxValue)
                .SetItem("int32", int.MaxValue)
                .SetItem("int64", long.MaxValue)
                .SetItem("uint16", ushort.MaxValue)
                .SetItem("uint32", uint.MaxValue)
                .SetItem("uint64", ulong.MaxValue)
                .SetItem("dictionary", BencodexDictionaryFixture)
                .SetItem("immutable_dictionary", BencodexDictionaryFixture)
                .SetItem("list", (IValue)BencodexListFixture)
                .SetItem("immutable_list", (IValue)BencodexListFixture)
                .SetItem("bencodex.text", (IValue)new Text("text"))
                .SetItem(
                    "bencodex.binary",
                    (IValue)new Binary(new byte[] { 0x05, 0x06, 0x07, 0x08 }))
                .SetItem(
                    "bencodex.boolean",
                    (IValue)new Bencodex.Types.Boolean(true))
                .SetItem(
                    "bencodex.integer",
                    (IValue)new Integer(ulong.MaxValue))
                .SetItem(
                    "bencodex.dictionary",
                    Bencodex.Types.Dictionary.Empty)
                .SetItem("bencodex.list", (IValue)default(Bencodex.Types.List))
                .SetItem(
                    "inner_struct",
                    Bencodex.Types.Dictionary.Empty.SetItem(
                        "string",
                        "string"));

        [Fact]
        public void SerializeStruct()
        {
            var serialized = BencodexSerializer<Struct>.Serialize(Fixture);

            Assert.Equal(SerializedFixture, serialized);
        }

        [Fact]
        public void DeserializeStruct()
        {
            var deserialized =
                BencodexSerializer<Struct>.Deserialize(SerializedFixture);

            Assert.Equal(Fixture.String, deserialized.String);
            Assert.Equal(Fixture.ByteArray, deserialized.ByteArray);
            Assert.Equal(Fixture.Bool, deserialized.Bool);
            Assert.Equal(Fixture.Int16, deserialized.Int16);
            Assert.Equal(Fixture.Int32, deserialized.Int32);
            Assert.Equal(Fixture.Int64, deserialized.Int64);
            Assert.Equal(Fixture.UInt16, deserialized.UInt16);
            Assert.Equal(Fixture.UInt32, deserialized.UInt32);
            Assert.Equal(Fixture.UInt64, deserialized.UInt64);
            Assert.Equal(Fixture.Text, deserialized.Text);
            Assert.Equal(Fixture.Binary, deserialized.Binary);
            Assert.Equal(Fixture.Boolean, deserialized.Boolean);
            Assert.Equal(Fixture.Integer, deserialized.Integer);
            Assert.Equal(Fixture.Dictionary, deserialized.Dictionary);
            Assert.Equal(Fixture.List, deserialized.List);
            Assert.Equal(Fixture.ImmutableList, deserialized.ImmutableList);
            Assert.Equal(Fixture.InnerStruct.String, deserialized.InnerStruct.String);
        }

        [Fact]
        public void SerializeClass()
        {
            var serialized = BencodexSerializer<Class>.Serialize(new Class
            {
                Property = "property",
            });

            Assert.Equal(
                serialized,
                Bencodex.Types.Dictionary.Empty.Add("property", "property"));
        }

        [Fact]
        public void DeserializeClass()
        {
            var expected = new Class
            {
                Property = "property",
            };

            var deserialized = BencodexSerializer<Class>.Deserialize(
                Bencodex.Types.Dictionary.Empty.Add("property", "property"));

            Assert.Equal(
                expected,
                deserialized);
        }

        [Fact]
        public void DetectNotAnnotatedStruct()
        {
            Assert.Throws<BencodexSerializationException>(
                () => BencodexSerializer<NotMarkedStruct>.Serialize(default));
            Assert.Throws<BencodexSerializationException>(
                () => BencodexSerializer<NotMarkedClass>.Serialize(default));
        }

        [BencodexObject]
        private struct InnerStruct : IComparable<InnerStruct>
        {
            [BencodexProperty("string")]
            public string String;

            public int CompareTo(InnerStruct other)
            {
                return String.CompareTo(other.String);
            }
        }

        [BencodexObject]
        private struct Struct
        {
            [BencodexProperty("string")]
            public string String;

            [BencodexProperty("byte_array")]
            public byte[] ByteArray;

            [BencodexProperty("bool")]
            public bool Bool;

            [BencodexProperty("int16")]
            public short Int16;

            [BencodexProperty("int32")]
            public int Int32;

            [BencodexProperty("int64")]
            public long Int64;

            [BencodexProperty("uint16")]
            public ushort UInt16;

            [BencodexProperty("uint32")]
            public uint UInt32;

            [BencodexProperty("uint64")]
            public ulong UInt64;

            [BencodexProperty("dictionary")]
            public Dictionary<string, InnerStruct> Dictionary;

            [BencodexProperty("immutable_dictionary")]
            public ImmutableDictionary<string, InnerStruct> ImmutableDictionary;

            [BencodexProperty("list")]
            public List<InnerStruct> List;

            [BencodexProperty("immutable_list")]
            public ImmutableList<InnerStruct> ImmutableList;

            [BencodexProperty("bencodex.text")]
            public Bencodex.Types.Text Text;

            [BencodexProperty("bencodex.binary")]
            public Bencodex.Types.Binary Binary;

            [BencodexProperty("bencodex.boolean")]
            public Bencodex.Types.Boolean Boolean;

            [BencodexProperty("bencodex.integer")]
            public Bencodex.Types.Integer Integer;

            [BencodexProperty("bencodex.dictionary")]
            public Bencodex.Types.Dictionary BencodexDictionary;

            [BencodexProperty("bencodex.list")]
            public Bencodex.Types.List BencodexList;

            [BencodexProperty("inner_struct")]
            public InnerStruct InnerStruct;

            public string NotAnnotatedField;
        }

        private struct NotMarkedStruct
        {
        }

        [BencodexObject]
        private class Class : IComparable<Class>
        {
            [BencodexProperty("property")]
            public string Property { get; internal set; }

            public int CompareTo(Class other)
            {
                return string.Compare(
                    Property,
                    other.Property,
                    StringComparison.Ordinal);
            }
        }

        private class NotMarkedClass
        {
        }
    }
}
