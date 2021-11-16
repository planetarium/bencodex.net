using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Bencodex.Types;
using Xunit;
using static Bencodex.Misc.ImmutableByteArrayExtensions;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests.Types
{
    public class ListTest
    {
        private static readonly Codec _codec;
        private static List _zero;
        private static List _one;
        private static List _two;
        private static List _nest;

        static ListTest()
        {
            _codec = new Codec();
            _zero = List.Empty;
            _one = new List(Null.Value);
            _two = new List(new Text[] { "hello", "world" }.Cast<IValue>());
            _nest = new List(Null.Value, _zero, _one, _two);
        }

        [Fact]
        public void Constructors()
        {
            Assert.Equal(
                _one.Add(_zero).Add(_one).Add(_two),
                new List(ImmutableArray.Create<IValue>(Null.Value, _zero, _one, _two))
            );

            Assert.Equal(_zero, new List(Enumerable.Empty<IValue>())
            );
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
        public void Type()
        {
            Assert.Equal(ValueType.List, _zero.Type);
            Assert.Equal(ValueType.List, _one.Type);
            Assert.Equal(ValueType.List, _two.Type);
            Assert.Equal(ValueType.List, _nest.Type);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(
                new Fingerprint(ValueType.List, 2L),
                _zero.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueType.List,
                    3L,
                    ParseHex("d14952314d5de233ef0dd0a178617f7f07ea082c")
                ),
                _one.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueType.List,
                    18L,
                    ParseHex("16a855873ac787b7c7f2d2d0360119ca4cbb66fe")
                ),
                _two.Fingerprint
            );
            Assert.Equal(
                new Fingerprint(
                    ValueType.List,
                    26L,
                    ParseHex("82daa9e2ff9f01393b718e09ab9fddd9f8c04e2b")
                ),
                _nest.Fingerprint
            );
        }

        [Fact]
        public void EncodingLength()
        {
            Assert.Equal(2L, _zero.EncodingLength);
            Assert.Equal(3L, _one.EncodingLength);
            Assert.Equal(18L, _two.EncodingLength);
            Assert.Equal(26L, _nest.EncodingLength);
        }

        [Fact]
        public void Inspect()
        {
            Assert.Equal("[]", _zero.Inspection);
            Assert.Equal("[null]", _one.Inspection);
            Assert.Equal("[\n  \"hello\",\n  \"world\",\n]", _two.Inspection);

            var expected = @"[
  null,
  [],
  [null],
  [
    ""hello"",
    ""world"",
  ],
]".NoCr();
            Assert.Equal(expected, _nest.Inspection);

            // If any element is a list/dict it should be indented
            Assert.Equal("[\n  [],\n]", new List(new IValue[] { _zero }).Inspection);
            Assert.Equal("[\n  {},\n]", new List(Dictionary.Empty).Inspection);
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
        }

        [Fact]
        public void Indexer()
        {
            Assert.Equal(default(Null), _one[0]);
            Assert.Equal((Text)"hello", _two[0]);
            Assert.Equal((Text)"world", _two[1]);
        }

        [Fact]
        public void Count()
        {
            Assert.Equal(Enumerable.Count(_zero), _zero.Count);
            Assert.Equal(Enumerable.Count(_one), _one.Count);
            Assert.Equal(Enumerable.Count(_two), _two.Count);
        }

        [Fact]
        public void Add()
        {
            var list = List.Empty
                .Add("foo")
                .Add(Encoding.UTF8.GetBytes("bar"))
                .Add(0xbeef)
                .Add(true)
                .Add(List.Empty)
                .Add(Dictionary.Empty);

            Assert.Equal((Text)"foo", list[0]);
            Assert.Equal((Binary)Encoding.UTF8.GetBytes("bar"), list[1]);
            Assert.Equal((Integer)0xbeef, list[2]);
            Assert.Equal((Boolean)true, list[3]);
            Assert.Equal(List.Empty, list[4]);
            Assert.Equal(Dictionary.Empty, list[5]);
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
        }
    }
}
