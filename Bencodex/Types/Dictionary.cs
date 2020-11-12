using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Globalization;
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

        public int Count => Value?.Count ?? 0;

        public IEnumerable<IKey> Keys =>
            Value?.Keys ?? Enumerable.Empty<IKey>();

        public IEnumerable<IValue> Values =>
            Value?.Values ?? Enumerable.Empty<IValue>();

        [Pure]
        public string Inspection
        {
            get
            {
                if (Value is null || !this.Any())
                {
                    return "{}";
                }

                IEnumerable<string> pairs = this.Select(kv =>
                    $"{kv.Key.Inspection}: {kv.Value.Inspection.Replace("\n", "\n  ")}"
                ).OrderBy(s => s);
                string pairsString = string.Join(",\n  ", pairs);
                return $"{{\n  {pairsString}\n}}";
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

        public IValue this[byte[] key] => this[(IKey)new Binary(key)];

        public IEnumerator<KeyValuePair<IKey, IValue>> GetEnumerator() => Value?.GetEnumerator()
            ?? Enumerable.Empty<KeyValuePair<IKey, IValue>>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            (IEnumerator<KeyValuePair<IKey, IValue>>)GetEnumerator();

        public bool ContainsKey(IKey key) => !(Value is null) && Value.ContainsKey(key);

        public bool ContainsKey(string key) => ContainsKey((IKey)new Text(key));

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

        public Dictionary Add(string key, byte[] value) => Add(key, (IValue)new Binary(value));

        public Dictionary Add(string key, bool value) => Add(key, (IValue)new Boolean(value));

        public Dictionary Add(string key, IEnumerable<IValue> value) =>
            Add(key, (IValue)new List(value));

        public Dictionary Add(byte[] key, IValue value) =>
            (Dictionary)Add((IKey)new Binary(key), value);

        public Dictionary Add(byte[] key, string value) => Add(key, (IValue)new Text(value));

        public Dictionary Add(byte[] key, long value) => Add(key, (IValue)new Integer(value));

        public Dictionary Add(byte[] key, ulong value) => Add(key, (IValue)new Integer(value));

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

        public Dictionary SetItem(byte[] key, IValue value) =>
            (Dictionary)SetItem((IKey)new Binary(key), value);

        public Dictionary SetItem(byte[] key, string value) =>
            SetItem(key, (IValue)new Text(value));

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

        public override int GetHashCode() => Value is null ? 0 : Value.GetHashCode();

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            yield return _dictionaryPrefix;

            if (!(Value is null))
            {
                IEnumerable<ValueTuple<byte?, byte[], IValue>> rawPairs =
                    from pair in this
                    select (
                        pair.Key.KeyPrefix,
                        pair.Key.EncodeAsByteArray(),
                        pair.Value
                    );
                IEnumerable<ValueTuple<byte?, byte[], IValue>> orderedPairs = rawPairs.OrderBy(
                    triple => (triple.Item1, triple.Item2),
                    keyPairComparer
                );
                foreach ((byte? keyPrefix, byte[] key, IValue value) in orderedPairs)
                {
                    if (keyPrefix != null)
                    {
                        yield return _unicodeKeyPrefix;
                    }

                    yield return Encoding.ASCII.GetBytes(
                        key.Length.ToString(CultureInfo.InvariantCulture)
                    );
                    yield return CommonVariables.Separator;
                    yield return key;
                    foreach (byte[] chunk in value.EncodeIntoChunks())
                    {
                        yield return chunk;
                    }
                }
            }

            yield return CommonVariables.Suffix;
        }

        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Bencodex.Types)}.{nameof(Dictionary)} {Inspection}";

#pragma warning disable SA1401
        private class ValueUnion
        {
            internal ImmutableDictionary<IKey, IValue>? Dict;
            internal KeyValuePair<IKey, IValue>[]? Pairs;
        }
#pragma warning restore SA1401
    }
}
