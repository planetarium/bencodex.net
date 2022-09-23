namespace Bencodex.Json.Tests;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public class SpecData : IEnumerable<Spec>
{
    public SpecData(string? testSuitePath = null)
    {
        TestSuitePath = testSuitePath ?? DefaultTestSuitePath();
    }

    public string TestSuitePath { get; }

    public static string DefaultTestSuitePath([CallerFilePath] string? path = null)
    {
        path = path is { } s
            ? Path.GetDirectoryName(s)
            : "Bencodex.Json.Tests";
        return Path.Join(path, "..", "Bencodex.Tests", "spec", "testsuite");
    }

    public IEnumerator<Spec> GetEnumerator()
    {
        foreach (string datPath in Directory.GetFiles(TestSuitePath))
        {
            if (Path.GetExtension(datPath).ToLower() != ".dat")
            {
                continue;
            }

            string jsonReprPath = Path.ChangeExtension(datPath, ".repr.json");
            if (File.Exists(jsonReprPath))
            {
                yield return new Spec(datPath, jsonReprPath);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
