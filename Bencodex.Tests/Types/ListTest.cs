using System.Linq;
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
            Assert.Equal("[\n  \"hello\",\n  \"world\"\n]", _two.Inspection);

            var expected = @"[
  null,
  [],
  [null],
  [
    ""hello"",
    ""world""
  ]
]".NoCr();
            Assert.Equal(expected, _nest.Inspection);

            // If any element is a list/dict it should be indented
            Assert.Equal("[\n  []\n]", new List(new IValue[] { _zero }).Inspection);
            Assert.Equal("[\n  {}\n]", new List(new IValue[] { Dictionary.Empty }).Inspection);
        }

        [Fact]
        public void String()
        {
            Assert.Equal("Bencodex.Types.List []", _zero.ToString());
            Assert.Equal("Bencodex.Types.List [null]", _one.ToString());
            Assert.Equal(
                "Bencodex.Types.List [\n  \"hello\",\n  \"world\"\n]",
                _two.ToString()
            );
        }
    }
}
