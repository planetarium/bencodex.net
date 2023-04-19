using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using static Bencodex.Tests.TestUtils;
using IEquatableValues =
    System.IEquatable<System.Collections.Immutable.IImmutableList<Bencodex.Types.IValue>>;

namespace Bencodex.Tests.Types
{
    public class ListTest
    {
        private static readonly Codec _codec = new Codec();
        private readonly List _zero;
        private readonly List _one;
        private readonly List _two;
        private readonly List _nest;
        private readonly IndirectValue[] _partiallyLoadedContents;
        private readonly IValue[] _loadedValues;
        private readonly List _partiallyLoaded;
        private readonly List<Fingerprint> _loadLog;

        public ListTest()
        {
            _zero = List.Empty;
            _one = new List(Null.Value);
            _two = new List(new Text[] { "hello", "world" }.Cast<IValue>());
            _nest = new List(Null.Value, _zero, _one, _two);
            _partiallyLoadedContents = new[]
            {
                new IndirectValue((Text)"loaded value"),
                new IndirectValue(_one.Fingerprint),
                new IndirectValue((Text)"loaded value2"),
                new IndirectValue(_two.Fingerprint),
                new IndirectValue((Text)"loaded value3"),
                new IndirectValue(_nest.Fingerprint),
            };
            _loadedValues = new IValue[]
            {
                (Text)"loaded value",
                _one,
                (Text)"loaded value2",
                _two,
                (Text)"loaded value3",
                _nest,
            };
            _partiallyLoaded = new List(_partiallyLoadedContents, loader: Loader);
            _loadLog = new List<Fingerprint>();
        }

        [Fact]
        public void Constructors()
        {
            Assert.Equal(_zero, new List(Enumerable.Empty<IValue>()));
            Assert.Equal(
                new List(ImmutableArray<IValue>.Empty.Add(Null.Value)),
                new List(Enumerable.Empty<IValue>().Append(Null.Value))
            );
            Assert.Equal(
                new List((Text)"hello", (Text)"world"),
                new List(new Text[] { "hello", "world" }.Cast<IValue>())
            );
            Assert.Equal(
                _nest,
                new List(new IValue[] { Null.Value, _zero }.Concat(new IValue[] { _one, _two }))
            );

            Assert.Equal(_zero, new List());
            Assert.Equal(_zero.Add(Null.Value), new List(Null.Value));
            Assert.Equal(_two, new List((Text)"hello", (Text)"world"));
            Assert.Equal(
                _one.Add(_zero).Add(_one).Add(_two),
                new List(Null.Value, _zero, _one, _two)
            );
        }

        [Fact]
        public void ParameterTypesForConstructors()
        {
            List<string> stringList = new List<string>() { "foo", "bar", "baz" };
            List<Text> textList = stringList.Select(s => (Text)s).ToList();
            List<int> intList = new List<int> { 0, 1, 2 };
            List<Integer> integerList = intList.Select(i => (Integer)i).ToList();
            List<byte[]> bytesList = new List<byte[]>
            {
                new byte[] { 1, 2 },
                new byte[] { 3, 4, 5 },
            };
            List<Binary> binaryList = bytesList.Select(bs => (Binary)bs).ToList();

            Assert.Equal(new List(stringList), new List(textList));
            Assert.Equal(new List(intList), new List(integerList));
            Assert.Equal(new List(bytesList), new List(binaryList));
        }

        [Fact]
        public void Kind()
        {
            Assert.Equal(ValueKind.List, _zero.Kind);
            Assert.Equal(ValueKind.List, _one.Kind);
            Assert.Equal(ValueKind.List, _two.Kind);
            Assert.Equal(ValueKind.List, _nest.Kind);
            Assert.Equal(ValueKind.List, _partiallyLoaded.Kind);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(
                new Fingerprint(ValueKind.List, 2L),
                _zero.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.List,
                    3L,
                    ParseHex("d14952314d5de233ef0dd0a178617f7f07ea082c")
                ),
                _one.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.List,
                    18L,
                    ParseHex("16a855873ac787b7c7f2d2d0360119ca4cbb66fe")
                ),
                _two.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueKind.List,
                    26L,
                    ParseHex("82daa9e2ff9f01393b718e09ab9fddd9f8c04e2b")
                ),
                _nest.Fingerprint
            );

            Assert.Equal(
                new Fingerprint(
                    ValueKind.List,
                    99L,
                    ParseHex("2b8cb630402d2507bab8484430dfbde931f4e1bc")
                ),
                _partiallyLoaded.Fingerprint
            );
            Assert.Empty(_loadLog);
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(2L, _zero.EncodingLength);
            Assert.Equal(3L, _one.EncodingLength);
            Assert.Equal(18L, _two.EncodingLength);
            Assert.Equal(26L, _nest.EncodingLength);

            Assert.Equal(99L, _partiallyLoaded.EncodingLength);
            Assert.Empty(_loadLog);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Inspect(bool loadAll)
        {
            Assert.Equal("[]", _zero.Inspect(loadAll));
            Assert.Equal("[null]", _one.Inspect(loadAll));
            Assert.Equal("[\n  \"hello\",\n  \"world\",\n]", _two.Inspect(loadAll));

            var expected = @"[
  null,
  [],
  [null],
  [
    ""hello"",
    ""world"",
  ],
]".NoCr();
            Assert.Equal(expected, _nest.Inspect(loadAll));

            // If any element is a list/dict it should be indented
            Assert.Equal("[\n  [],\n]", new List(new IValue[] { _zero }).Inspect(loadAll));
            Assert.Equal("[\n  {},\n]", new List(Dictionary.Empty).Inspect(loadAll));
        }

        [Fact]
        public void InspectPartiallyLoadedList()
        {
            var expected = @"[
  ""loaded value"",
  List d14952314d5de233ef0dd0a178617f7f07ea082c [3 B],
  ""loaded value2"",
  List 16a855873ac787b7c7f2d2d0360119ca4cbb66fe [18 B],
  ""loaded value3"",
  List 82daa9e2ff9f01393b718e09ab9fddd9f8c04e2b [26 B],
]";
            Assert.Equal(expected, _partiallyLoaded.Inspect(false));
            Assert.Empty(_loadLog);

            expected = @"[
  ""loaded value"",
  [null],
  ""loaded value2"",
  [
    ""hello"",
    ""world"",
  ],
  ""loaded value3"",
  [
    null,
    [],
    [null],
    [
      ""hello"",
      ""world"",
    ],
  ],
]";
            Assert.Equal(expected, _partiallyLoaded.Inspect(true));
            Assert.NotEmpty(_loadLog);
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.List []", _zero.ToString());
            Assert.Equal("Bencodex.Types.List [null]", _one.ToString());
            Assert.Equal(
                "Bencodex.Types.List [\n  \"hello\",\n  \"world\",\n]",
                _two.ToString()
            );

            Assert.Equal(
                $"Bencodex.Types.List {_partiallyLoaded.Inspect(false)}",
                _partiallyLoaded.ToString()
            );
            Assert.Empty(_loadLog);
        }

        [Fact]
        public void Indexer()
        {
            Assert.Equal(default(Null), _one[0]);
            Assert.Equal((Text)"hello", _two[0]);
            Assert.Equal((Text)"world", _two[1]);

            Assert.Empty(_loadLog);
            Assert.Equal((Text)"loaded value", _partiallyLoaded[0]);
            Assert.Empty(_loadLog);
            Assert.Equal(_one, _partiallyLoaded[1]);
            Assert.Single(_loadLog);
            Assert.Equal(_two, _partiallyLoaded[3]);
            Assert.Equal(2, _loadLog.Count);
        }

        [Fact]
        public void Count()
        {
            Assert.Equal(Enumerable.Count(_zero), _zero.Count);
            Assert.Equal(Enumerable.Count(_one), _one.Count);
            Assert.Equal(Enumerable.Count(_two), _two.Count);

            Assert.Equal(_loadedValues.Length, _partiallyLoaded.Count);
            Assert.Empty(_loadLog);
        }

        [Fact]
        public void Enumerate()
        {
            Assert.Empty(_zero.ToArray());
            Assert.Equal(new IValue[] { Null.Value }, _one.ToArray());
            Assert.Equal(
                new Text[] { "hello", "world" }.Cast<IValue>(),
                _two.ToArray()
            );
            Assert.Equal(
                new IValue[] { Null.Value, _zero, _one, _two },
                _nest.ToArray()
            );

            Assert.Empty(_loadLog);
            Assert.Equal(
                _loadedValues,
                _partiallyLoaded.ToArray()
            );
            Assert.NotEmpty(_loadLog);
        }

        [Fact]
        public void Equality()
        {
            Assert.True(_zero.Equals(new List()));
            Assert.True(((IEquatableValues)_zero).Equals(ImmutableArray<IValue>.Empty));
            Assert.True(_one.Equals(new List(Null.Value)));
            Assert.True(
                ((IEquatableValues)_one).Equals(ImmutableArray<IValue>.Empty.Add(Null.Value))
            );
            Assert.True(_two.Equals(new List((Text)"hello", (Text)"world")));
            Assert.True(
                ((IEquatableValues)_two).Equals(
                    ImmutableArray.Create<IValue>((Text)"hello", (Text)"world")
                )
            );
            Assert.True(_nest.Equals(new List(Null.Value, _zero, _one, _two)));
            Assert.True(
                ((IEquatableValues)_nest).Equals(
                    ImmutableArray.Create<IValue>(Null.Value, _zero, _one, _two)
                )
            );

            Assert.True(_partiallyLoaded.Equals(new List(_partiallyLoadedContents, Loader)));
            Assert.Empty(_loadLog);

            Assert.True(
                ((IEquatableValues)_partiallyLoaded).Equals(ImmutableArray.Create(_loadedValues))
            );
            Assert.Empty(_loadLog);

            Assert.False(_zero.Equals(_one));
            Assert.False(((IEquatableValues)_zero).Equals(_one));
            Assert.False(_zero.Equals(_two));
            Assert.False(((IEquatableValues)_zero).Equals(_two));
            Assert.False(_zero.Equals(_nest));
            Assert.False(((IEquatableValues)_zero).Equals(_nest));
            Assert.False(_one.Equals(_zero));
            Assert.False(((IEquatableValues)_one).Equals(_zero));
            Assert.False(_one.Equals(_two));
            Assert.False(((IEquatableValues)_one).Equals(_two));
            Assert.False(_one.Equals(_nest));
            Assert.False(((IEquatableValues)_one).Equals(_nest));
            Assert.False(_two.Equals(_zero));
            Assert.False(((IEquatableValues)_two).Equals(_zero));
            Assert.False(_two.Equals(_one));
            Assert.False(((IEquatableValues)_two).Equals(_one));
            Assert.False(_two.Equals(_nest));
            Assert.False(((IEquatableValues)_two).Equals(_nest));
            Assert.False(_nest.Equals(_one));
            Assert.False(((IEquatableValues)_nest).Equals(_one));
            Assert.False(_nest.Equals(_two));
            Assert.False(((IEquatableValues)_nest).Equals(_two));
            Assert.False(_nest.Equals(_two));
            Assert.False(((IEquatableValues)_nest).Equals(_two));

            Assert.False(_partiallyLoaded.Equals(_zero));
            Assert.False(((IEquatableValues)_partiallyLoaded).Equals(_zero));
            Assert.False(_partiallyLoaded.Equals(_one));
            Assert.False(((IEquatableValues)_partiallyLoaded).Equals(_one));
            Assert.False(_partiallyLoaded.Equals(_two));
            Assert.False(((IEquatableValues)_partiallyLoaded).Equals(_two));
            Assert.False(_partiallyLoaded.Equals(_nest));
            Assert.False(((IEquatableValues)_partiallyLoaded).Equals(_nest));
            Assert.Empty(_loadLog);
        }

        [Fact]
        public void Add()
        {
            string someString = "foo";
            byte[] someBytes = Encoding.UTF8.GetBytes("bar");
            short someShort = 123;
            int someInt = 123_456;
            bool someBool = true;
            var list = List.Empty
                .Add(someString)
                .Add(someBytes)
                .Add(someShort)
                .Add(someInt)
                .Add(someBool);

            Assert.Equal((Text)someString, list[0]);
            Assert.Equal((Binary)someBytes, list[1]);
            Assert.Equal((Integer)someShort, list[2]);
            Assert.Equal((Integer)someInt, list[3]);
            Assert.Equal((Boolean)someBool, list[4]);

            list = List.Empty
                .Add((Text)someString)
                .Add((Binary)someBytes)
                .Add((Integer)someShort)
                .Add((Integer)someInt)
                .Add((Boolean)someBool)
                .Add(List.Empty)
                .Add(Dictionary.Empty);

            Assert.Equal((Text)someString, list[0]);
            Assert.Equal((Binary)someBytes, list[1]);
            Assert.Equal((Integer)someShort, list[2]);
            Assert.Equal((Integer)someInt, list[3]);
            Assert.Equal((Boolean)someBool, list[4]);
            Assert.Equal(List.Empty, list[5]);
            Assert.Equal(Dictionary.Empty, list[6]);

            List nullAddedToPartiallyLoaded = _partiallyLoaded.Add(Null.Value);
            Assert.Equal(nullAddedToPartiallyLoaded, new List(_loadedValues).Add(Null.Value));
            Assert.Equal(
                nullAddedToPartiallyLoaded.ToArray(),
                _loadedValues.Append(Null.Value).ToArray()
            );
        }

        [Fact]
        public void Encode()
        {
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                _codec.Encode(_zero)
            );
            AssertEqual(
                new byte[] { 0x6c, 0x65 },  // "le"
                _codec.Encode(new List())
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
                _codec.Encode(_two)
            );

            AssertEqual(
                _codec.Encode(new List(_loadedValues)),
                _codec.Encode(_partiallyLoaded)
            );
        }

        [Fact]
        public void EnumerateIndirectValues()
        {
            Assert.Empty(_zero.EnumerateIndirectValues());
            Assert.All(_one.EnumerateIndirectValues(), iv => Assert.NotNull(iv.LoadedValue));
            Assert.Equal(_one.Select(v => new IndirectValue(v)), _one.EnumerateIndirectValues());
            Assert.All(_two.EnumerateIndirectValues(), iv => Assert.NotNull(iv.LoadedValue));
            Assert.Equal(_two.Select(v => new IndirectValue(v)), _two.EnumerateIndirectValues());
            Assert.All(_nest.EnumerateIndirectValues(), iv => Assert.NotNull(iv.LoadedValue));
            Assert.Equal(_nest.Select(v => new IndirectValue(v)), _nest.EnumerateIndirectValues());

            Assert.Equal(_partiallyLoadedContents, _partiallyLoaded.EnumerateIndirectValues());
            int i = 0;
            foreach (IndirectValue iv in _partiallyLoaded.EnumerateIndirectValues())
            {
                Assert.Equal(_partiallyLoadedContents[i].Fingerprint, iv.Fingerprint);
                Assert.Equal(_partiallyLoadedContents[i].LoadedValue, iv.LoadedValue);
                i++;
            }

            Assert.Empty(_loadLog);
        }

        [Fact]
        public void HashCode()
        {
            Assert.Equal(
                _zero.GetHashCode(),
                List.Empty.GetHashCode());
            Assert.Equal(
                _one.GetHashCode(),
                new List(Null.Value).GetHashCode());
            Assert.Equal(
                _two.GetHashCode(),
                new List(new Text[] { "hello", "world" }.Cast<IValue>()).GetHashCode());
            Assert.Equal(
                _nest.GetHashCode(),
                new List(Null.Value, _zero, _one, _two).GetHashCode());

            var added = _nest.Add("baz");
            Assert.NotEqual(_nest.GetHashCode(), added.GetHashCode());
            Assert.Equal(added.GetHashCode(), _nest.Add("baz").GetHashCode());
            Assert.Equal(_nest.GetHashCode(), added.Remove(new Text("baz")).GetHashCode());

            // Same length but different contents.
            Assert.NotEqual(
                new List((Integer)0, Null.Value, (Integer)2).GetHashCode(),
                new List(Null.Value, (Text)"FOO", Dictionary.Empty).GetHashCode()
            );
        }

        private IValue Loader(Fingerprint f)
        {
            _loadLog.Add(f);
            return new IValue[] { _one, _two, _nest }.First(v => v.Fingerprint.Equals(f));
        }
    }
}
