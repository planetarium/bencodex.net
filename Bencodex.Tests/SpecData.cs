using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Bencodex.Tests
{
    public class SpecData : IEnumerable<Spec>
    {
        public SpecData(string testSuitePath = null)
        {
            TestSuitePath = testSuitePath ?? DefaultTestSuitePath();
        }

        public string TestSuitePath { get; }

        public static string DefaultTestSuitePath(
            [CallerFilePath] string path = null
        )
        {
            path = path == null
                ? "Bencodex.Tests"
                : Path.GetDirectoryName(path);
            return Path.Join(path, "spec", "testsuite");
        }

        public IEnumerator<Spec> GetEnumerator()
        {
            foreach (string datPath in Directory.GetFiles(TestSuitePath))
            {
                if (Path.GetExtension(datPath).ToLower() != ".dat")
                {
                    continue;
                }

                string yamlPath = Path.ChangeExtension(datPath, ".yaml");
                if (!File.Exists(yamlPath) || Directory.Exists(yamlPath))
                {
                    yamlPath = Path.ChangeExtension(yamlPath, ".yml");
                    if (!File.Exists(yamlPath))
                    {
                        continue;
                    }

                    if (Directory.Exists(yamlPath))
                    {
                        continue;
                    }
                }

                yield return new Spec(yamlPath, datPath);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
