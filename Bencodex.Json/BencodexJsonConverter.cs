namespace Bencodex.Json;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bencodex.Types;

public sealed class BencodexJsonConverter : JsonConverter<IValue>
{
    public BencodexJsonConverter()
        : this(256)
    {
    }

    public BencodexJsonConverter(int base64Threshold)
        : base()
    {
        Base64Threshold = base64Threshold;
    }

    public int Base64Threshold { get; }

    public override bool CanConvert(Type typeToConvert) =>
        typeof(IValue).IsAssignableFrom(typeToConvert) && (
            typeof(Bencodex.Types.Null).IsAssignableFrom(typeToConvert) &&
            typeof(Bencodex.Types.Boolean).IsAssignableFrom(typeToConvert) &&
            typeof(Bencodex.Types.Integer).IsAssignableFrom(typeToConvert) &&
            typeof(Bencodex.Types.Binary).IsAssignableFrom(typeToConvert) &&
            typeof(Bencodex.Types.Text).IsAssignableFrom(typeToConvert) &&
            typeof(Bencodex.Types.List).IsAssignableFrom(typeToConvert) &&
            typeof(Bencodex.Types.Dictionary).IsAssignableFrom(typeToConvert)
        );

    public override IValue? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.None)
        {
            if (!reader.Read())
            {
                throw new JsonException("Unexpected end of JSON.");
            }
        }

        switch (reader.TokenType)
        {
            case JsonTokenType.None:
                throw new JsonException("Unexpected end of JSON.");

            case JsonTokenType.Null:
                return Bencodex.Types.Null.Value;

            case JsonTokenType.True:
                return new Bencodex.Types.Boolean(true);

            case JsonTokenType.False:
                return new Bencodex.Types.Boolean(false);

            case JsonTokenType.Number:
                // In theory, this shouldn't happen, but it's for misimplemented
                // serializers:
                if (reader.TryGetInt64(out var i))
                {
                    return new Bencodex.Types.Integer(i);
                }
                else
                {
                    byte[] asciiDigits = reader.ValueSpan.ToArray();
                    return new Bencodex.Types.Integer(
                        Encoding.ASCII.GetString(asciiDigits),
                        CultureInfo.InvariantCulture
                    );
                }

            case JsonTokenType.String:
                return DecodeStringValue(reader.GetString()!);

            case JsonTokenType.StartArray:
                var list = new List<IValue>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    list.Add(
                        Read(ref reader, typeof(IValue), options) ?? Bencodex.Types.Null.Value
                    );
                }

                return new Bencodex.Types.List(list);

            case JsonTokenType.StartObject:
                var pairs = new List<KeyValuePair<IKey, IValue>>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException("Expected property name.");
                    }

                    var key = DecodeStringKey(reader.GetString()!);
                    reader.Read();
                    var value = Read(ref reader, typeof(IValue), options) ?? Bencodex.Types.Null.Value;
                    var pair = new KeyValuePair<IKey, IValue>(key, value);
                    pairs.Add(pair);
                }

                return new Bencodex.Types.Dictionary(pairs);

            default:
                throw new JsonException(
                    $"Unexpected token type: {reader.TokenType} ({reader.BytesConsumed})"
                );
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        IValue value,
        JsonSerializerOptions options)
    {
        switch (value)
        {
            case Bencodex.Types.Null _:
                writer.WriteNullValue();
                break;

            case Bencodex.Types.Boolean b:
                writer.WriteBooleanValue(b.Value);
                break;

            case Bencodex.Types.Integer i:
                writer.WriteStringValue(i.Value.ToString(CultureInfo.InvariantCulture));
                break;

            case Bencodex.Types.Binary bin:
                writer.WriteStringValue(EncodeBinary(bin));
                break;

            case Bencodex.Types.Text t:
                writer.WriteStringValue("\ufeff" + t.Value);
                break;

            case Bencodex.Types.List l:
                writer.WriteStartArray();
                foreach (var item in l)
                {
                    Write(writer, item, options);
                }

                writer.WriteEndArray();
                break;

            case Bencodex.Types.Dictionary d:
                writer.WriteStartObject();
                foreach (var pair in d)
                {
                    string key;
                    switch (pair.Key)
                    {
                        case Bencodex.Types.Binary bin:
                            key = EncodeBinary(bin);
                            break;

                        case Bencodex.Types.Text t:
                            key = "\ufeff" + t.Value;
                            break;

                        default:
                            throw new JsonException(
                                $"Dictionary key must be either {nameof(Binary)} or " +
                                $"{nameof(Text)}."
                            );
                    }

                    writer.WritePropertyName(key);
                    Write(writer, pair.Value, options);
                }

                writer.WriteEndObject();
                break;

            default:
                throw new JsonException(
                    "Unsupported Bencodex value type: " + value.GetType().FullName
                );
        }

        writer.Flush();
    }

    private IKey DecodeStringKey(string s)
    {
        if (s.Length == 0)
        {
            throw new JsonException("Empty string is not allowed.");
        }

        if (s[0] == '\ufeff')
        {
            return new Bencodex.Types.Text(s.Substring(1));
        }
        else if (s.StartsWith("0x"))
        {
            return Bencodex.Types.Binary.FromHex(s, 2);
        }
        else if (s.StartsWith("b64:"))
        {
#if NETSTANDARD2_0
            return Bencodex.Types.Binary.FromBase64(s.Substring(4));
#else
            return Bencodex.Types.Binary.FromBase64(s.AsSpan(4));
#endif
        }

        throw new JsonException($"Invalid string format: {s}");
    }

    private IValue DecodeStringValue(string s)
    {
        if (s.Length > 0 && (
                s[0] == '-' ||
                (s[0] >= '1' && s[0] <= '9') ||
                (s[0] == '0' && (s.Length < 2 || s[1] != 'x'))
            ))
        {
            try
            {
                return new Bencodex.Types.Integer(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new JsonException("Unexpected number format: " + s, e);
            }
        }

        return DecodeStringKey(s);
    }

    private string EncodeBinary(Binary bin)
    {
        if (bin.ByteArray.Length < Base64Threshold)
        {
            var builder = new StringBuilder();
            builder.Append("0x");
            bin.ToHex(builder);
            return builder.ToString();
        }

        // TODO: This could lead to memcpy overhead.  We should implement
        // Binary.ToBase64(StringBuilder) method to avoid it.
        return $"b64:{bin.ToBase64()}";
    }
}
