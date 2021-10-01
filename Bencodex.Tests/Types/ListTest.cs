using System.Linq;
using System.Text;
using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests.Types
{
    public class ListTest
    {
        private static List _zero;
        private static List _one;
        private static List _two;
        private static List _nest;

        static ListTest()
        {
            _zero = default(List);
            _one = new List(new IValue[] { default(Null) });
            _two = new List(new Text[] { "hello", "world" }.Cast<IValue>());
            _nest = new List(
                new IValue[]
                {
                    default(Null),
                    _zero,
                    _one,
                    _two,
                }
            );
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
            Assert.Equal("[\n  {},\n]", new List(new IValue[] { Dictionary.Empty }).Inspection);
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
            // Compare with inner implementation, to avoid Xunit check.
            Assert.Equal(_zero.Value.Length, _zero.Count);
            Assert.Equal(_one.Value.Length, _one.Count);
            Assert.Equal(_two.Value.Length, _two.Count);
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
    }
}
