using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Bencodex.Types;
using SharpYaml;
using SharpYaml.Serialization;

namespace Bencodex.Tests
{
    public class Spec
    {
        public Spec(string semanticsPath, string encodingPath)
        {
            SemanticsPath = semanticsPath;
            EncodingPath = encodingPath;
        }

        public string SemanticsPath { get; }

        public string EncodingPath { get; }

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

        public IValue Semantics
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
                    return TransformValue(node);
                }
            }
        }

        public Stream OpenEncodingStream()
        {
            return new FileStream(EncodingPath, FileMode.Open, FileAccess.Read);
        }

        public Stream OpenSemanticsStream()
        {
            return new FileStream(SemanticsPath, FileMode.Open, FileAccess.Read);
        }

        public override string ToString()
        {
            return string.Format("{0}\u2013{1}", SemanticsPath, EncodingPath);
        }

        private IKey TransformKey(YamlNode node)
        {
            IValue v = TransformValue(node);
            if (v is IKey key)
            {
                return key;
            }

            throw new NotImplementedException(
                $"Unsupported node type for key: {node.GetType()}"
            );
        }

        private IValue TransformValue(YamlNode node)
        {
            switch (node)
            {
                case YamlScalarNode scalar:
                    switch (scalar.Tag)
                    {
                        case "tag:yaml.org,2002:null":
                            return default(Null);
                        case "tag:yaml.org,2002:bool":
                            string bLit = scalar.Value.ToLower();
                            return new Bencodex.Types.Boolean(
                                bLit == "on" || bLit == "true" ||
                                bLit == "y" || bLit == "yes"
                            );
                        case "tag:yaml.org,2002:int":
                            return new Integer(scalar.Value);
                        case "tag:yaml.org,2002:binary":
                            return new Binary(
                                Convert.FromBase64String(scalar.Value)
                            );
                        case null:
                        case "":
                            if (scalar.Style == ScalarStyle.Plain)
                            {
                                switch (scalar.Value.ToLower())
                                {
                                    case "null":
                                    case "":
                                    case null:
                                        return default(Null);
                                    case "on":
                                    case "true":
                                    case "y":
                                    case "yes":
                                        return new Bencodex.Types.Boolean(true);
                                    case "false":
                                    case "off":
                                    case "n":
                                    case "no":
                                        return default(Bencodex.Types.Boolean);
                                }

                                BigInteger i;
                                try
                                {
                                    i = BigInteger.Parse(
                                        scalar.Value,
                                        CultureInfo.InvariantCulture
                                    );
                                }
                                catch (FormatException)
                                {
                                    return new Text(scalar.Value);
                                }

                                return new Integer(i);
                            }

                            return new Text(scalar.Value);
                        default:
                            throw new NotImplementedException(
                                $"unsupported tag: \"{scalar.Tag}\""
                            );
                    }

                case YamlMappingNode map:
                    var transformedPairs = map.Select(kv =>
                        KeyValuePair.Create(
                            TransformKey(kv.Key),
                            TransformValue(kv.Value)
                        )
                    );
                    return new Dictionary(transformedPairs);

                case YamlSequenceNode seq:
                    return new List(seq.Children.Select(TransformValue));

                default:
                    throw new NotImplementedException(
                        $"Unsupported node type for vlaue: {node.GetType()}"
                    );
            }
        }
    }
}
