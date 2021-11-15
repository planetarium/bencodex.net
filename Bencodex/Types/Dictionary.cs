using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Bencodex.Misc;

namespace Bencodex.Types
{
    /// <summary>
    /// Represents Bencodex dictionaries.
    /// </summary>
    public class Dictionary :
        IValue,
        IEquatable<IImmutableDictionary<IKey, IValue>>,
        IImmutableDictionary<IKey, IValue>
    {
        /// <summary>
        /// The empty dictionary.
        /// </summary>
        public static readonly Dictionary Empty =
            new Dictionary(ImmutableDictionary<IKey, IValue>.Empty);

        /// <summary>
        /// The singleton fingerprint for empty dictionaries.
        /// </summary>
        public static readonly Fingerprint EmptyFingerprint =
            new Fingerprint(ValueType.Dictionary, 2);

        private static readonly byte[] _dictionaryPrefix = new byte[1] { 0x64 };  // 'd'

        private static readonly byte[] _unicodeKeyPrefix = new byte[1] { 0x75 };  // 'u'

        private static IComparer<ValueTuple<byte?, byte[]>> keyPairComparer =
            new CompositeComparer<byte?, byte[]>(
                Comparer<byte?>.Default,
                default(ByteArrayComparer)
            );

        private ImmutableDictionary<IKey, IValue>? _dict;
        private KeyValuePair<IKey, IValue>[]? _pairs;
        private ImmutableArray<byte>? _hash;
        private long _encodingLength = -1;

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<IKey, IValue>> pairs)
        {
            _pairs = pairs.ToArray();
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with the <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The dictionary content.</param>
        public Dictionary(in ImmutableDictionary<IKey, IValue> content)
        {
            _dict = content;
        }

        public int Count => Value.Count;

        public IEnumerable<IKey> Keys =>
            Value.Keys;

        public IEnumerable<IValue> Values =>
            Value.Values;

        /// <inheritdoc cref="IValue.Type"/>
        [Pure]
        public ValueType Type => ValueType.Dictionary;

        /// <inheritdoc cref="IValue.Fingerprint"/>
        [Pure]
        public Fingerprint Fingerprint
        {
            get
            {
                if (_dict is { } d && d.IsEmpty)
                {
                    return EmptyFingerprint;
                }
                else if (_pairs is { } p && p.Length < 1)
                {
                    return EmptyFingerprint;
                }

                var pairs = _dict ?? _pairs ?? Enumerable.Empty<KeyValuePair<IKey, IValue>>();
                var fDict = new SortedDictionary<Fingerprint, Fingerprint>(
                    new FingerprintComparer()
                );
                foreach (KeyValuePair<IKey, IValue> kv in pairs)
                {
                    Fingerprint keyF = kv.Key.Fingerprint;
                    if (!fDict.ContainsKey(keyF))
                    {
                        fDict[keyF] = kv.Value.Fingerprint;
                    }
                }

                if (!fDict.Any())
                {
                    return EmptyFingerprint;
                }

                if (!(_hash is { } hash))
                {
                    long encLength = 2L;
                    SHA1 sha1 = SHA1.Create();
                    sha1.Initialize();
                    foreach (KeyValuePair<Fingerprint, Fingerprint> pair in fDict)
                    {
                        byte[] fp = pair.Key.Serialize();
                        sha1.TransformBlock(fp, 0, fp.Length, null, 0);
                        fp = pair.Value.Serialize();
                        sha1.TransformBlock(fp, 0, fp.Length, null, 0);
                        encLength += pair.Key.EncodingLength +
                                     pair.Value.EncodingLength;
                    }

                    sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    hash = ImmutableArray.Create(sha1.Hash);
                    _hash = hash;
                    if (_encodingLength < 0)
                    {
                        _encodingLength = encLength;
                    }
                }

                return new Fingerprint(Type, EncodingLength, hash);
            }
        }

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength =>
            _encodingLength >= 0
                ? _encodingLength
                : _encodingLength = _dictionaryPrefix.LongLength
                    + Value.Sum(kv => kv.Key.EncodingLength + kv.Value.EncodingLength)
                    + CommonVariables.Suffix.LongLength;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Pure]
        public string Inspection
        {
            get
            {
                if (_dict is { } d && d.IsEmpty)
                {
                    return "{}";
                }
                else if (_pairs is { } p && p.Length < 1)
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

        [Pure]
        private ImmutableDictionary<IKey, IValue> Value
        {
            get
            {
                if (!(_dict is { } dict))
                {
                    dict = _pairs.ToImmutableDictionary();
                    _dict = dict;
                    _pairs = null;
                }

                return dict;
            }
        }

        /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.this[TKey]"/>
        [Pure]
        public IValue this[IKey key] => Value[key];

        public IValue this[string key] => this[(IKey)new Text(key)];

        public IValue this[ImmutableArray<byte> key] => this[(IKey)new Binary(key)];

        public IValue this[byte[] key] => this[(IKey)new Binary(key)];

        public IEnumerator<KeyValuePair<IKey, IValue>> GetEnumerator() => Value?.GetEnumerator()
            ?? Enumerable.Empty<KeyValuePair<IKey, IValue>>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            (IEnumerator<KeyValuePair<IKey, IValue>>)GetEnumerator();

        /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.ContainsKey(TKey)"/>
        [Pure]
        public bool ContainsKey(IKey key) => Value.ContainsKey(key);

        public bool ContainsKey(string key) => ContainsKey((IKey)new Text(key));

        public bool ContainsKey(ImmutableArray<byte> key) => ContainsKey((IKey)new Binary(key));

        public bool ContainsKey(byte[] key) => ContainsKey((IKey)new Binary(key));

        /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.TryGetValue(TKey, out TValue)"/>
        [Pure]
        public bool TryGetValue(IKey key, out IValue value) =>
            Value.TryGetValue(key, out value);

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

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.Clear()"/>
        [Pure]
        public IImmutableDictionary<IKey, IValue> Clear() => Empty;

        /// <inheritdoc
        /// cref="IImmutableDictionary{TKey,TValue}.Contains(KeyValuePair{TKey, TValue})"/>
        [Pure]
        public bool Contains(KeyValuePair<IKey, IValue> pair) => Value.Contains(pair);

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.Remove(TKey)"/>
        [Pure]
        public IImmutableDictionary<IKey, IValue> Remove(IKey key) =>
            Value.IsEmpty ? this : new Dictionary(Value.Remove(key));

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.RemoveRange(IEnumerable{TKey})"/>
        [Pure]
        public IImmutableDictionary<IKey, IValue> RemoveRange(IEnumerable<IKey> keys) =>
            Value.IsEmpty ? this : new Dictionary(Value.RemoveRange(keys));

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

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.TryGetKey(TKey, out TKey)"/>
        [Pure]
        public bool TryGetKey(IKey equalKey, out IKey actualKey) =>
            Value.TryGetKey(equalKey, out actualKey);

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

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        [Pure]
        bool IEquatable<IImmutableDictionary<IKey, IValue>>.Equals(
            IImmutableDictionary<IKey, IValue> other
        )
        {
            if (Value.LongCount() != other.LongCount())
            {
                return false;
            }

            foreach (KeyValuePair<IKey, IValue> kv in this)
            {
                if (!other.TryGetValue(kv.Key, out IValue v) ||
                    !v.Equals(kv.Value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        [Pure]
        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Dictionary o &&
            ((IEquatable<IImmutableDictionary<IKey, IValue>>)this).Equals(o);

        /// <inheritdoc cref="object.GetHashCode()"/>
        [Pure]
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc cref="IValue.EncodeIntoChunks()"/>
        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            // FIXME: avoid duplication between this and EncodeToStream()
            long length = _dictionaryPrefix.LongLength;
            yield return _dictionaryPrefix;

            var @enum = _dict ?? _pairs ?? Enumerable.Empty<KeyValuePair<IKey, IValue>>();
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
                if (_dict is null && prev is { } p)
                {
                    if (p.keyPrefix == keyPrefix && byteArrayComparer.Compare(p.key, key) == 0)
                    {
                        continue;
                    }
                }

                if (keyPrefix != null)
                {
                    yield return _unicodeKeyPrefix;
                    length += _unicodeKeyPrefix.LongLength;
                }

                byte[] keyLengthBytes = Encoding.ASCII.GetBytes(
                    key.Length.ToString(CultureInfo.InvariantCulture)
                );
                yield return keyLengthBytes;
                length += keyLengthBytes.LongLength;
                yield return CommonVariables.Separator;
                length += CommonVariables.Separator.LongLength;
                yield return key;
                length += key.LongLength;
                foreach (byte[] chunk in value.EncodeIntoChunks())
                {
                    yield return chunk;
                    length += chunk.LongLength;
                }

                prev = (keyPrefix, key);
            }

            yield return CommonVariables.Suffix;
            length += CommonVariables.Suffix.Length;
            _encodingLength = length;
        }

        /// <inheritdoc cref="IValue.EncodeToStream(Stream)"/>
        [Pure]
        public void EncodeToStream(Stream stream)
        {
            // FIXME: avoid duplication between this and EncodeIntoChunks()
            long startPos = stream.Position;
            stream.WriteByte(_dictionaryPrefix[0]);

            var @enum = _dict ?? _pairs ?? Enumerable.Empty<KeyValuePair<IKey, IValue>>();
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
                if (_dict is null && prev is { } p)
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

            stream.WriteByte(CommonVariables.Suffix[0]);
            _encodingLength = stream.Position - startPos;
        }

        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Bencodex.Types)}.{nameof(Dictionary)} {Inspection}";
    }
}
