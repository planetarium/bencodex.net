using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using SharpYaml;
using SharpYaml.Serialization;

namespace Bencodex.Tests
{
    public class Spec
    {
        public string SemanticsPath { get; }
        public string EncodingPath { get; }

        public Spec(string semanticsPath, string encodingPath)
        {
            SemanticsPath = semanticsPath;
            EncodingPath = encodingPath;
        }

        public Stream OpenEncodingStream()
        {
            return new FileStream(EncodingPath, FileMode.Open);
        }

        public Stream OpenSemanticsStream()
        {
            return new FileStream(SemanticsPath, FileMode.Open);
        }

        public byte[] Encoding
        {
            get
            {
                using (Stream f = OpenEncodingStream())
                {
                    var buffer = new byte[f.Length];
                    f.Read(buffer);
                    return buffer;
                }
            }
        }

        public object Semantics
        {
            get
            {
                Encoding encoding = System.Text.Encoding.UTF8;
                using (Stream f = OpenSemanticsStream())
                using (TextReader reader = new StreamReader(f, encoding))
                {
                    var yamlStream = new YamlStream();
                    yamlStream.Load(reader);
                    YamlNode node = yamlStream.Documents[0].RootNode;
                    return TransformNode(node);
                }
            }
        }

        private object TransformNode(YamlNode node)
        {
            switch (node)
            {
                case YamlScalarNode scalar:
                    switch (scalar.Tag)
                    {
                        case "tag:yaml.org,2002:null":
                            return null;
                        case "tag:yaml.org,2002:bool":
                            string bLit = scalar.Value.ToLower();
                            return bLit == "on" || bLit == "true" ||
                                   bLit == "y" || bLit == "yes";
                        case "tag:yaml.org,2002:int":
                            return BigInteger.Parse(scalar.Value);
                        case "tag:yaml.org,2002:binary":
                            return Convert.FromBase64String(scalar.Value);
                        case null:
                        case "":
                            if (scalar.Style == ScalarStyle.Plain)
                            {
                                switch (scalar.Value.ToLower())
                                {
                                    case "null":
                                    case "":
                                    case null:
                                        return null;
                                    case "on":
                                    case "true":
                                    case "y":
                                    case "yes":
                                        return true;
                                    case "false":
                                    case "off":
                                    case "n":
                                    case "no":
                                        return false;
                                }
                                try
                                {
                                    return BigInteger.Parse(scalar.Value);
                                }
                                catch (FormatException)
                                {
                                    return scalar.Value;
                                }
                            }
                            return scalar.Value;
                        default:
                            throw new NotImplementedException(
                                $"unsupported tag: \"{scalar.Tag}\""
                            );
                    }

                case YamlMappingNode map:
                    var transformedPairs = map.Select(kv =>
                        KeyValuePair.Create(
                            TransformNode(kv.Key),
                            TransformNode(kv.Value)
                        )
                    );
                    return new Dictionary<object, object>(transformedPairs);

                case YamlSequenceNode seq:
                    return seq.Children.Select(TransformNode).ToImmutableList();

                default:
                    throw new NotImplementedException(
                        $"Unsupported node type: {node.GetType()}"
                    );
            }
        }

        public override string ToString()
        {
            return String.Format("{0}\u2013{1}", SemanticsPath, EncodingPath);
        }
    }

    public class SpecData : IEnumerable<Spec>
    {
        public string TestSuitePath { get; }

        public SpecData(string testSuitePath)
        {
            TestSuitePath = testSuitePath;
        }

        public IEnumerator<Spec> GetEnumerator()
        {
            foreach (string datPath in Directory.GetFiles(TestSuitePath))
            {
                if (Path.GetExtension(datPath).ToLower() != ".dat") continue;
                string yamlPath = Path.ChangeExtension(datPath, ".yaml");
                if (!File.Exists(yamlPath) || Directory.Exists(yamlPath))
                {
                    yamlPath = Path.ChangeExtension(yamlPath, ".yml");
                    if (!File.Exists(yamlPath)) continue;
                    if (Directory.Exists(yamlPath)) continue;
                }
                yield return new Spec(yamlPath, datPath);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static string DefaultTestSuitePath(
            [CallerFilePath] string path = null
        )
        {
            path = path == null
                ? "Bencodex.Tests"
                : Path.GetDirectoryName(path);
            return Path.Join(path, "spec", "testsuite");
        }

        public static SpecData GetInstance()
        {
            return new SpecData(DefaultTestSuitePath());
        }
    }
}
