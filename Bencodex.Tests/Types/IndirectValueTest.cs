using System;
using System.Text;
using Bencodex.Types;
using Xunit;

namespace Bencodex.Tests.Types
{
    public class IndirectValueTest
    {
        private static IValue _list = new List(
            Null.Value,
            (Text)"foo",
            (Text)"bar",
            new Binary("baz", Encoding.ASCII)
        );

        private static IValue _dict = Dictionary.Empty
            .Add("foo", "bar")
            .Add("baz", "qux");

        private IndirectValue _loaded = new IndirectValue(_list);
        private IndirectValue _unloaded = new IndirectValue(_dict.Fingerprint);
        private IndirectValue _default = default;

        [Fact]
        public void DefaultConstructor()
        {
            IndirectValue d = default;
            Assert.False(d.Loaded);
            Assert.Throws<InvalidOperationException>(() => d.Fingerprint);
            Assert.Throws<ArgumentNullException>(() => d.GetValue(null));
            Assert.Throws<InvalidOperationException>(() => d.GetValue(_ => _list));
        }

        [Fact]
        public void Constructors()
        {
            var loaded = new IndirectValue(_list);
            Assert.True(loaded.Loaded);
            Assert.Equal(_list, loaded.GetValue(null));

            var unloaded = new IndirectValue(_dict.Fingerprint);
            Assert.False(unloaded.Loaded);
            Assert.Equal(_dict.Fingerprint, unloaded.Fingerprint);
        }

        [Fact]
        public void Fingerprint()
        {
            Assert.Equal(_list.Fingerprint, _loaded.Fingerprint);
            Assert.Equal(_dict.Fingerprint, _unloaded.Fingerprint);
            Assert.Throws<InvalidOperationException>(() => _default.Fingerprint);
        }

        [Fact]
        public void Loaded()
        {
            Assert.True(_loaded.Loaded);
            Assert.False(_unloaded.Loaded);
            Assert.False(_default.Loaded);
        }

        [Fact]
        public void GetValueWithLoader()
        {
            Assert.Equal(_list, _loaded.GetValue(_ => _list));
            Assert.Equal(_dict, _unloaded.GetValue(_ => _dict));
            Assert.Throws<InvalidOperationException>(() => _default.GetValue(_ => _dict));
        }

        [Fact]
        public void GetValueWithWrongLoader()
        {
            Assert.Equal(_list, _loaded.GetValue(_ => _dict));
            Assert.Throws<InvalidOperationException>(() => _unloaded.GetValue(_ => _list));
        }

        [Fact]
        public void GetValueWithoutLoader()
        {
            Assert.Equal(_list, _loaded.GetValue(null));
            Assert.Throws<ArgumentNullException>(() => _unloaded.GetValue(null));
            Assert.Throws<ArgumentNullException>(() => _default.GetValue(null));
        }
    }
}
