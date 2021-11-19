using System.Collections.Generic;
using Bencodex.Types;

namespace Bencodex.Misc
{
    internal class IndirectValueEqualityComparer : IEqualityComparer<IndirectValue>
    {
        private readonly IEqualityComparer<IValue> _equalityComparer;
        private readonly IndirectValue.Loader? _loader;

        public IndirectValueEqualityComparer(
            IEqualityComparer<IValue> valueEqualityComparer,
            IndirectValue.Loader? loader
        )
        {
            _equalityComparer = valueEqualityComparer;
            _loader = loader;
        }

        public bool Equals(IndirectValue x, IndirectValue y) =>
            Equals(x.GetValue(_loader), y.GetValue(_loader));

        public int GetHashCode(IndirectValue obj) =>
            _equalityComparer.GetHashCode(obj.GetValue(_loader));
    }
}
