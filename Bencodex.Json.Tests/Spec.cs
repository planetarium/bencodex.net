namespace Bencodex.Json.Tests;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Bencodex;
using Bencodex.Types;
using Xunit;

public class Spec
{
    private Codec _codec = new Codec();

    public Spec(string bencodexPath, string jsonReprPath)
    {
        BencodexPath = bencodexPath;
        JsonReprPath = jsonReprPath;
    }

    public string BencodexPath { get; }

    public string JsonReprPath { get; }

    public IValue BencodexValue
    {
        get
        {
            Encoding encoding = System.Text.Encoding.UTF8;
            using Stream f = OpenBencodexFile();
            return _codec.Decode(f);
        }
    }

    public void GetJsonReprReader(out Utf8JsonReader reader)
    {
        byte[] bytes = File.ReadAllBytes(JsonReprPath);
        Assert.NotEmpty(bytes);
        reader = new Utf8JsonReader(bytes);
    }

    public Stream OpenBencodexFile()
    {
        return new FileStream(BencodexPath, FileMode.Open, FileAccess.Read);
    }

    public override string ToString()
    {
        return string.Format("{0}\u2013{1}", BencodexPath, JsonReprPath);
    }
}
