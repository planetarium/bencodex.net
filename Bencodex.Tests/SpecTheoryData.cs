using System.Collections;
using System.Collections.Generic;

namespace Bencodex.Tests
{
    public class SpecTheoryData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var specData = new SpecData();
            foreach (Spec spec in specData)
            {
                yield return new object[] { spec };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}