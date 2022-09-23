namespace Bencodex.Json.Tests;

using System.Collections;
using System.Collections.Generic;

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
