using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests
{
    public class OffloadInspectorTest
    {
        [Fact]
        public void List()
        {
            var allLoaded = new List(new Text("foo"), new Text("bar"));
            IndirectValue[] values = allLoaded.EnumerateIndirectValues(out IndirectValue.Loader loader)
                .ToArray();
            Assert.Null(loader);
            Assert.Equal(2, values.Length);
            Assert.Equal(new Text("foo"), values[0].LoadedValue);
            Assert.Equal(new Text("bar"), values[1].LoadedValue);

            var offloadValue = new Text("offloaded");
            var someOffloaded = new List(
                new[]
                {
                    new IndirectValue(new Text("loaded")),
                    new IndirectValue(offloadValue.Fingerprint),
                    new IndirectValue(new Binary("loaded", Encoding.ASCII)),
                },
                _ => offloadValue
            );
            values = someOffloaded.EnumerateIndirectValues(out loader).ToArray();
            Assert.NotNull(loader);
            Assert.Equal(offloadValue, loader(offloadValue.Fingerprint));
            Assert.Equal(3, values.Length);
            Assert.Equal(new Text("loaded"), values[0].LoadedValue);
            Assert.Null(values[1].LoadedValue);
            Assert.Equal(offloadValue.Fingerprint, values[1].Fingerprint);
            Assert.Equal(new Binary("loaded", Encoding.ASCII), values[2].LoadedValue);
        }

        [Fact]
        public void Dictionary()
        {
            var allLoaded = new Dictionary(new[]
            {
                new KeyValuePair<IKey, IValue>(new Binary("foo", Encoding.ASCII), new Text("bar")),
                new KeyValuePair<IKey, IValue>(new Text("baz"), new Text("qux")),
            });
            KeyValuePair<IKey, IndirectValue>[] pairs =
                allLoaded.EnumerableIndirectPairs(out IndirectValue.Loader loader).ToArray();
            Assert.Null(loader);
            Assert.Equal(2, pairs.Length);
            Assert.Equal(new Binary("foo", Encoding.ASCII), pairs[0].Key);
            Assert.Equal(new Text("bar"), pairs[0].Value.LoadedValue);
            Assert.Equal(new Text("baz"), pairs[1].Key);
            Assert.Equal(new Text("qux"), pairs[1].Value.LoadedValue);

            var offloadValue = new Text("offloaded");
            var someOffloaded = new Dictionary(
                new[]
                {
                    new KeyValuePair<IKey, IndirectValue>(
                        new Binary("loaded", Encoding.ASCII),
                        new IndirectValue(new Integer(123))
                    ),
                    new KeyValuePair<IKey, IndirectValue>(
                        new Binary("unloaded", Encoding.ASCII),
                        new IndirectValue(offloadValue.Fingerprint)
                    ),
                    new KeyValuePair<IKey, IndirectValue>(
                        new Text("anotherLoaded"),
                        new IndirectValue(new Integer(456))
                    ),
                },
                _ => offloadValue
            );
            pairs = someOffloaded.EnumerableIndirectPairs(out loader).ToArray();
            Assert.NotNull(loader);
            Assert.Equal(offloadValue, loader(offloadValue.Fingerprint));
            Assert.Equal(3, pairs.Length);
            Assert.Equal(new Binary("loaded", Encoding.ASCII), pairs[0].Key);
            Assert.Equal(new Integer(123), pairs[0].Value.LoadedValue);
            Assert.Equal(new Binary("unloaded", Encoding.ASCII), pairs[1].Key);
            Assert.Null(pairs[1].Value.LoadedValue);
            Assert.Equal(offloadValue.Fingerprint, pairs[1].Value.Fingerprint);
            Assert.Equal(new Text("anotherLoaded"), pairs[2].Key);
            Assert.Equal(new Integer(456), pairs[2].Value.LoadedValue);
        }
    }
}
