using System.Collections.Generic;
using System.Text;
using Bencodex.Misc;
using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests.Misc
{
    public class KeyComparerTest
    {
        [Fact]
        public void Compare()
        {
            var unordered = new List<IKey>
            {
                new Binary("foo", Encoding.ASCII),
                new Binary("bar", Encoding.ASCII),
                (Text)"foo",
                (Text)"bar",
                (Text)"a",
                (Text)"\u00e1",
                (Text)"a\u0301",
            };
            IKey[] ordered =
            {
                new Binary("bar", Encoding.ASCII),
                new Binary("foo", Encoding.ASCII),
                (Text)"a",
                (Text)"a\u0301",
                (Text)"bar",
                (Text)"foo",
                (Text)"\u00e1",
            };

            unordered.Sort(KeyComparer.Instance);
            Assert.Equal(ordered, unordered);
            Assert.Equal(
                new HashSet<IKey>(unordered),
                new SortedSet<IKey>(unordered, KeyComparer.Instance)
            );
        }
    }
}
