namespace Bencodex.Json.Tests;

using System.IO;
using System.Text.Json;
using System.Text.Json.JsonDiffPatch.Xunit;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Bencodex.Types;
using Xunit;
using Xunit.Abstractions;

public class BencodexJsonConverterTest
{
    private ITestOutputHelper _output;
    private BencodexJsonConverter _converter;

    public BencodexJsonConverterTest(ITestOutputHelper output)
    {
        _output = output;
        _converter = new BencodexJsonConverter(base64Threshold: 64);
    }

    [Theory]
    [ClassData(typeof(SpecTheoryData))]
    public void SpecTestSuite_Read(Spec spec)
    {
        _output.WriteLine("JSON: {0}", spec.JsonReprPath);
        _output.WriteLine("Data: {0}", spec.BencodexPath);
        spec.GetJsonReprReader(out Utf8JsonReader reader);
        var options = new JsonSerializerOptions();
        IValue? read = _converter.Read(ref reader, typeof(IValue), options);
        IValue expected = spec.BencodexValue;
        Assert.Equal(expected, read);
    }

    [Theory]
    [ClassData(typeof(SpecTheoryData))]
    public void SpecTestSuite_Write(Spec spec)
    {
        _output.WriteLine("JSON: {0}", spec.JsonReprPath);
        _output.WriteLine("Data: {0}", spec.BencodexPath);
        spec.GetJsonReprReader(out Utf8JsonReader reader);
        JsonNode? expectedNode = JsonNode.Parse(ref reader);
        var options = new JsonSerializerOptions();
        using var buffer = new MemoryStream();
        using var writer = new Utf8JsonWriter(buffer);
        _converter.Write(writer, spec.BencodexValue, options);
        buffer.Seek(0, SeekOrigin.Begin);
        JsonNode? actualNode = JsonNode.Parse(buffer);
        JsonAssert.Equal(expectedNode, actualNode, true);
    }
}
