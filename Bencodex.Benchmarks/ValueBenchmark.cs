using System.Text;
using BenchmarkDotNet.Attributes;
using Bencodex.Types;

namespace Bencodex.Benchmarks
{
    public class ValueBenchmark
    {
        private static readonly Binary _binFoo = new Binary("foo", Encoding.ASCII);
        private static readonly Text _txtFoo = (Text)"foo";
        private static readonly List _list = List.Empty
            .Add("foo")
            .Add("bar")
            .Add(true)
            .Add(Null.Value)
            .Add(1234)
            .Add(Dictionary.Empty.Add("foo", 123).Add("bar", 456))
            .Add(List.Empty.Add("a").Add("b").Add("c"));
        private static readonly List _listCopyA = _list.Add(1);
        private static readonly List _listCopyB = _list.Add(1);
        private static readonly Dictionary _dict = Dictionary.Empty
            .Add("foo", "bar")
            .Add("baz", "qux")
            .Add("bool", true)
            .Add("null", Null.Value)
            .Add("int", 1234)
            .Add("dict", Dictionary.Empty.Add("foo", 123).Add("bar", 456))
            .Add("list", new List(new IValue[] { (Text)"a", (Text)"b", (Text)"c" }));
        private static readonly Dictionary _dictCopyA = _dict.Add("z", 1);
        private static readonly Dictionary _dictCopyB = _dict.Add("z", 1);

        [Benchmark]
        public void List_AddBinFromEmpty()
        {
            IValue _ = List.Empty.Add((IValue)_binFoo);
        }

        [Benchmark]
        public void List_AddTxtFromEmpty()
        {
            IValue _ = List.Empty.Add((IValue)_txtFoo);
        }

        [Benchmark]
        public void List_Add()
        {
            IValue _ = _list.Add(_dict);
        }

        [Benchmark]
        public void List_Equal()
        {
            bool _ = _listCopyA.Equals(_listCopyB);
        }

        [Benchmark]
        public void List_NotEqual()
        {
            bool _ = _listCopyA.Equals(_list);
        }

        [Benchmark]
        public void Dict_AddBinKeyFromEmpty()
        {
            IValue _ = (Dictionary)Dictionary.Empty.Add((IKey)_binFoo, _txtFoo);
        }

        [Benchmark]
        public void Dict_AddTxtKeyFromEmpty()
        {
            IValue _ = (Dictionary)Dictionary.Empty.Add((IKey)_txtFoo, _binFoo);
        }

        [Benchmark]
        public void Dict_AddKey()
        {
            IValue _ = (Dictionary)_dict.Add((IKey)_binFoo, _list);
        }

        [Benchmark]
        public void Dict_LookUpExist()
        {
            _dict.TryGetValue(_txtFoo, out IValue _);
        }

        [Benchmark]
        public void Dict_LookUpNonExist()
        {
            _dict.TryGetValue(_binFoo, out IValue _);
        }

        [Benchmark]
        public void Dict_Equal()
        {
            bool _ = _dictCopyA.Equals(_dictCopyB);
        }

        [Benchmark]
        public void Dict_NotEqual()
        {
            bool _ = _dictCopyA.Equals(_dict);
        }
    }
}
