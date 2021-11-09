using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Bencodex.Misc;

namespace Bencodex.Types
{
    public readonly struct Dictionary :
        IValue,
        IEquatable<IImmutableDictionary<IKey, IValue>>,
        IImmutableDictionary<IKey, IValue>
    {
        private static readonly byte[] _dictionaryPrefix = new byte[1] { 0x64 };  // 'd'

        private static readonly byte[] _unicodeKeyPrefix = new byte[1] { 0x75 };  // 'u'

        private static IComparer<ValueTuple<byte?, byte[]>> keyPairComparer =
            new CompositeComparer<byte?, byte[]>(
                Comparer<byte?>.Default,
                default(ByteArrayComparer)
            );

        private readonly ValueUnion? _value;

        public Dictionary(IEnumerable<KeyValuePair<IKey, IValue>> value)
        {
            KeyValuePair<IKey, IValue>[] pairs = value.ToArray();
            _value = pairs.Any()
                ? new ValueUnion { Pairs = pairs }
                : new ValueUnion { Dict = ImmutableDictionary<IKey, IValue>.Empty };
        }

        public static Dictionary Empty => default;

        public int Count => Value.Count;

        public IEnumerable<IKey> Keys =>
            Value.Keys;

        public IEnumerable<IValue> Values =>
            Value.Values;

        /// <inheritdoc cref="IValue.Type"/>
        [Pure]
        public ValueType Type => ValueType.Dictionary;

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public int EncodingLength =>
            _value is { } v
                ? v.EncodingLength >= 0
                    ? v.EncodingLength
                    : v.EncodingLength = _dictionaryPrefix.Length
                        + Value.Sum(kv => kv.Key.EncodingLength + kv.Value.EncodingLength)
                        + CommonVariables.Suffix.Length
                : 2;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Pure]
        public string Inspection
        {
            get
            {
                if (_value is null || !Value.Any())
                {
                    return "{}";
                }

                IEnumerable<string> pairs = this.Select(kv =>
                    $"  {kv.Key.Inspection}: {kv.Value.Inspection.Replace("\n", "\n  ")},\n"
                ).OrderBy(s => s);
                string pairsString = string.Join(string.Empty, pairs);
                return $"{{\n{pairsString}}}";
            }
        }

        private ImmutableDictionary<IKey, IValue> Value
        {
            get
            {
                if (!(_value is { } union))
                {
                    return ImmutableDictionary<IKey, IValue>.Empty;
                }

                if (!(union.Dict is { } dict))
                {
                    dict = union.Pairs.ToImmutableDictionary();
                    union.Dict = dict;
                    union.Pairs = null;
                }

                return dict;
            }
        }

        public IValue this[IKey key] => Value is null
            ? throw new KeyNotFoundException("The dictionary is empty.")
            : Value[key];

        public IValue this[string key] => this[(IKey)new Text(key)];

        public IValue this[ImmutableArray<byte> key] => this[(IKey)new Binary(key)];

        public IValue this[byte[] key] => this[(IKey)new Binary(key)];

        public IEnumerator<KeyValuePair<IKey, IValue>> GetEnumerator() => Value?.GetEnumerator()
            ?? Enumerable.Empty<KeyValuePair<IKey, IValue>>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            (IEnumerator<KeyValuePair<IKey, IValue>>)GetEnumerator();

        public bool ContainsKey(IKey key) => !(Value is null) && Value.ContainsKey(key);

        public bool ContainsKey(string key) => ContainsKey((IKey)new Text(key));

        public bool ContainsKey(ImmutableArray<byte> key) => ContainsKey((IKey)new Binary(key));

        public bool ContainsKey(byte[] key) => ContainsKey((IKey)new Binary(key));

        public bool TryGetValue(IKey key, out IValue value)
        {
            if (Value is null)
            {
#pragma warning disable SA1129
                value = new Null();
#pragma warning restore SA1129
                return false;
            }

            return Value.TryGetValue(key, out value);
        }

        public IImmutableDictionary<IKey, IValue> Add(IKey key, IValue value) =>
            new Dictionary(Value.Add(key, value));

        public Dictionary Add(string key, IValue value) =>
            (Dictionary)Add((IKey)new Text(key), value);

        public Dictionary Add(string key, string value) => Add(key, (IValue)new Text(value));

        public Dictionary Add(string key, long value) => Add(key, (IValue)new Integer(value));

        public Dictionary Add(string key, ulong value) => Add(key, (IValue)new Integer(value));

        public Dictionary Add(string key, ImmutableArray<byte> value) =>
            Add(key, (IValue)new Binary(value));

        public Dictionary Add(string key, byte[] value) => Add(key, (IValue)new Binary(value));

        public Dictionary Add(string key, bool value) => Add(key, (IValue)new Boolean(value));

        public Dictionary Add(string key, IEnumerable<IValue> value) =>
            Add(key, (IValue)new List(value));

        public Dictionary Add(ImmutableArray<byte> key, IValue value) =>
            (Dictionary)Add((IKey)new Binary(key), value);

        public Dictionary Add(ImmutableArray<byte> key, string value) => Add(key, (IValue)new Text(value));

        public Dictionary Add(ImmutableArray<byte> key, long value) => Add(key, (IValue)new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, ulong value) => Add(key, (IValue)new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, ImmutableArray<byte> value) =>
            Add(key, (IValue)new Binary(value));

        public Dictionary Add(ImmutableArray<byte> key, byte[] value) => Add(key, (IValue)new Binary(value));

        public Dictionary Add(ImmutableArray<byte> key, bool value) => Add(key, (IValue)new Boolean(value));

        public Dictionary Add(ImmutableArray<byte> key, IEnumerable<IValue> value) =>
            Add(key, (IValue)new List(value));

        public Dictionary Add(byte[] key, IValue value) =>
            (Dictionary)Add((IKey)new Binary(key), value);

        public Dictionary Add(byte[] key, string value) => Add(key, (IValue)new Text(value));

        public Dictionary Add(byte[] key, long value) => Add(key, (IValue)new Integer(value));

        public Dictionary Add(byte[] key, ulong value) => Add(key, (IValue)new Integer(value));

        public Dictionary Add(byte[] key, ImmutableArray<byte> value) => Add(key, (IValue)new Binary(value));

        public Dictionary Add(byte[] key, byte[] value) => Add(key, (IValue)new Binary(value));

        public Dictionary Add(byte[] key, bool value) => Add(key, (IValue)new Boolean(value));

        public Dictionary Add(byte[] key, IEnumerable<IValue> value) =>
            Add(key, (IValue)new List(value));

        public IImmutableDictionary<IKey, IValue> AddRange(
            IEnumerable<KeyValuePair<IKey, IValue>> pairs
        ) =>
            new Dictionary(Value.AddRange(pairs));

        public IImmutableDictionary<IKey, IValue> Clear() => default(Dictionary);

        public bool Contains(KeyValuePair<IKey, IValue> pair) => Value?.Contains(pair) ?? false;

        public IImmutableDictionary<IKey, IValue> Remove(IKey key) =>
            Value is null ? this : new Dictionary(Value.Remove(key));

        public IImmutableDictionary<IKey, IValue> RemoveRange(IEnumerable<IKey> keys) =>
            Value is null ? this : new Dictionary(Value.RemoveRange(keys));

        public IImmutableDictionary<IKey, IValue> SetItem(IKey key, IValue value) =>
            new Dictionary(Value.SetItem(key, value));

        public Dictionary SetItem(IKey key, string value) =>
            (Dictionary)SetItem(key, (IValue)new Text(value));

        public Dictionary SetItem(IKey key, ImmutableArray<byte> value) =>
            (Dictionary)SetItem(key, (IValue)new Binary(value));

        public Dictionary SetItem(IKey key, byte[] value) =>
            (Dictionary)SetItem(key, (IValue)new Binary(value));

        public Dictionary SetItem(IKey key, long value) =>
            (Dictionary)SetItem(key, (IValue)new Integer(value));

        public Dictionary SetItem(IKey key, ulong value) =>
            (Dictionary)SetItem(key, (IValue)new Integer(value));

        public Dictionary SetItem(IKey key, bool value) =>
            (Dictionary)SetItem(key, (IValue)new Boolean(value));

        public Dictionary SetItem(IKey key, IEnumerable<IValue> value) =>
            (Dictionary)SetItem(key, (IValue)new List(value));

        public Dictionary SetItem(string key, IValue value) =>
            (Dictionary)SetItem((IKey)new Text(key), value);

        public Dictionary SetItem(string key, string value) =>
            SetItem(key, (IValue)new Text(value));

        public Dictionary SetItem(string key, ImmutableArray<byte> value) =>
            SetItem(key, (IValue)new Binary(value));

        public Dictionary SetItem(string key, byte[] value) =>
            SetItem(key, (IValue)new Binary(value));

        public Dictionary SetItem(string key, long value) =>
            SetItem(key, (IValue)new Integer(value));

        public Dictionary SetItem(string key, ulong value) =>
            SetItem(key, (IValue)new Integer(value));

        public Dictionary SetItem(string key, bool value) =>
            SetItem(key, (IValue)new Boolean(value));

        public Dictionary SetItem(string key, IEnumerable<IValue> value) =>
            SetItem(key, (IValue)new List(value));

        public Dictionary SetItem(ImmutableArray<byte> key, IValue value) =>
            (Dictionary)SetItem((IKey)new Binary(key), value);

        public Dictionary SetItem(ImmutableArray<byte> key, string value) =>
            SetItem(key, (IValue)new Text(value));

        public Dictionary SetItem(ImmutableArray<byte> key, ImmutableArray<byte> value) =>
            SetItem(key, (IValue)new Binary(value));

        public Dictionary SetItem(ImmutableArray<byte> key, byte[] value) =>
            SetItem(key, (IValue)new Binary(value));

        public Dictionary SetItem(ImmutableArray<byte> key, long value) =>
            SetItem(key, (IValue)new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, ulong value) =>
            SetItem(key, (IValue)new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, bool value) =>
            SetItem(key, (IValue)new Boolean(value));

        public Dictionary SetItem(ImmutableArray<byte> key, IEnumerable<IValue> value) =>
            SetItem(key, (IValue)new List(value));

        public Dictionary SetItem(byte[] key, IValue value) =>
            (Dictionary)SetItem((IKey)new Binary(key), value);

        public Dictionary SetItem(byte[] key, string value) =>
            SetItem(key, (IValue)new Text(value));

        public Dictionary SetItem(byte[] key, ImmutableArray<byte> value) =>
            SetItem(key, (IValue)new Binary(value));

        public Dictionary SetItem(byte[] key, byte[] value) =>
            SetItem(key, (IValue)new Binary(value));

        public Dictionary SetItem(byte[] key, long value) =>
            SetItem(key, (IValue)new Integer(value));

        public Dictionary SetItem(byte[] key, ulong value) =>
            SetItem(key, (IValue)new Integer(value));

        public Dictionary SetItem(byte[] key, bool value) =>
            SetItem(key, (IValue)new Boolean(value));

        public Dictionary SetItem(byte[] key, IEnumerable<IValue> value) =>
            SetItem(key, (IValue)new List(value));

        public IImmutableDictionary<IKey, IValue> SetItems(
            IEnumerable<KeyValuePair<IKey, IValue>> items
        ) =>
            new Dictionary(Value.SetItems(items));

        public bool TryGetKey(IKey equalKey, out IKey actualKey)
        {
            if (Value is null)
            {
                actualKey = default(Binary);
                return false;
            }

            return Value.TryGetKey(equalKey, out actualKey);
        }

        public T GetValue<T>(string name)
            where T : IValue
        {
            return (T)this[name];
        }

        public T GetValue<T>(ImmutableArray<byte> name)
            where T : IValue
        =>
            (T)this[name];

        public T GetValue<T>(byte[] name)
            where T : IValue
        {
            return (T)this[name];
        }

        public override bool Equals(object obj) =>
            obj switch
            {
                null => false,
                Dictionary d => ((IEquatable<IImmutableDictionary<IKey, IValue>>)this).Equals(d),
                _ => false
            };

        bool IEquatable<IImmutableDictionary<IKey, IValue>>.Equals(
            IImmutableDictionary<IKey, IValue> other
        )
        {
            if ((Value is null && other.LongCount() > 0) ||
                Value.LongCount() != other.LongCount())
            {
                return false;
            }

            foreach (KeyValuePair<IKey, IValue> kv in this)
            {
                if (!other.ContainsKey(kv.Key))
                {
                    return false;
                }

                if (!other[kv.Key].Equals(kv.Value))
                {
                    return false;
                }
            }

            return true;
        }

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Dictionary o &&
            ((IEquatable<IImmutableDictionary<IKey, IValue>>)this).Equals(o);

        public override int GetHashCode() => Value is null ? 0 : Value.GetHashCode();

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            // FIXME: avoid duplication between this and EncodeToStream()
            int length = _dictionaryPrefix.Length;
            yield return _dictionaryPrefix;

            if (!(_value is null))
            {
                var @enum = _value.Dict
                    ?? _value.Pairs
                    ?? Enumerable.Empty<KeyValuePair<IKey, IValue>>();
                IEnumerable<ValueTuple<byte?, byte[], IValue>> rawPairs =
                    from pair in @enum
                    select (
                        pair.Key.KeyPrefix,
                        pair.Key.EncodeAsByteArray(),
                        pair.Value
                    );
                IEnumerable<ValueTuple<byte?, byte[], IValue>> orderedPairs = rawPairs.OrderBy(
                    triple => (triple.Item1, triple.Item2),
                    keyPairComparer
                );
                var byteArrayComparer = default(ByteArrayComparer);
                (byte? keyPrefix, byte[] key)? prev = null;
                foreach ((byte? keyPrefix, byte[] key, IValue value) in orderedPairs)
                {
                    // Skip duplicates
                    if (_value.Dict is null && prev is { } p)
                    {
                        if (p.keyPrefix == keyPrefix && byteArrayComparer.Compare(p.key, key) == 0)
                        {
                            continue;
                        }
                    }

                    if (keyPrefix != null)
                    {
                        yield return _unicodeKeyPrefix;
                        length += _unicodeKeyPrefix.Length;
                    }

                    byte[] keyLengthBytes = Encoding.ASCII.GetBytes(
                        key.Length.ToString(CultureInfo.InvariantCulture)
                    );
                    yield return keyLengthBytes;
                    length += keyLengthBytes.Length;
                    yield return CommonVariables.Separator;
                    length += CommonVariables.Separator.Length;
                    yield return key;
                    length += key.Length;
                    foreach (byte[] chunk in value.EncodeIntoChunks())
                    {
                        yield return chunk;
                        length += chunk.Length;
                    }

                    prev = (keyPrefix, key);
                }
            }

            yield return CommonVariables.Suffix;
            length += CommonVariables.Suffix.Length;
            if (_value is { } v)
            {
                v.EncodingLength = length;
            }
        }

        public void EncodeToStream(Stream stream)
        {
            // FIXME: avoid duplication between this and EncodeIntoChunks()
            long startPos = stream.Position;
            stream.WriteByte(_dictionaryPrefix[0]);

            if (!(_value is null))
            {
                var @enum = _value.Dict
                    ?? _value.Pairs
                    ?? Enumerable.Empty<KeyValuePair<IKey, IValue>>();
                IEnumerable<ValueTuple<byte?, byte[], IValue>> rawPairs =
                    from pair in @enum
                    select (
                        pair.Key.KeyPrefix,
                        pair.Key.EncodeAsByteArray(),
                        pair.Value
                    );
                IEnumerable<ValueTuple<byte?, byte[], IValue>> orderedPairs = rawPairs.OrderBy(
                    triple => (triple.Item1, triple.Item2),
                    keyPairComparer
                );
                var byteArrayComparer = default(ByteArrayComparer);
                (byte? keyPrefix, byte[] key)? prev = null;
                foreach ((byte? keyPrefix, byte[] key, IValue value) in orderedPairs)
                {
                    // Skip duplicates
                    if (_value.Dict is null && prev is { } p)
                    {
                        if (p.keyPrefix == keyPrefix && byteArrayComparer.Compare(p.key, key) == 0)
                        {
                            continue;
                        }
                    }

                    if (keyPrefix != null)
                    {
                        stream.WriteByte(_unicodeKeyPrefix[0]);
                    }

                    var keyLen =
                        key.Length.ToString(CultureInfo.InvariantCulture);
                    var keyLenBytes = Encoding.ASCII.GetBytes(keyLen);
                    stream.Write(keyLenBytes, 0, keyLenBytes.Length);
                    stream.WriteByte(CommonVariables.Separator[0]);
                    stream.Write(key, 0, key.Length);
                    value.EncodeToStream(stream);
                    prev = (keyPrefix, key);
                }
            }

            stream.WriteByte(CommonVariables.Suffix[0]);
            if (_value is { } v)
            {
                v.EncodingLength = (int)(stream.Position - startPos);
            }
        }

        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Bencodex.Types)}.{nameof(Dictionary)} {Inspection}";

#pragma warning disable SA1401
        private class ValueUnion
        {
            internal ImmutableDictionary<IKey, IValue>? Dict;
            internal KeyValuePair<IKey, IValue>[]? Pairs;
            internal int EncodingLength = -1;
        }
#pragma warning restore SA1401
    }
}
