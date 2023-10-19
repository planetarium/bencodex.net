using System;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Bencodex.Types
{
    public struct Text :
        IKey,
        IEquatable<Text>,
        IComparable<Text>,
        IComparable
    {
        private int? _utf8Length;
        private string? _value;

        public Text(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _utf8Length = null;
        }

        public string Value => _value ?? (_value = string.Empty);

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.Text;

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength =>
            1L +
            Utf8Length.ToString(CultureInfo.InvariantCulture).Length +
            CommonVariables.Separator.LongLength +
            Utf8Length;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

        [Pure]
        internal int Utf8Length =>
            _utf8Length is { } l ? l : (
                _utf8Length = _value is { } v
                    ? Encoding.UTF8.GetByteCount(v)
                    : 0
                ).Value;

        public static implicit operator string(Text t)
        {
            return t.Value;
        }

        public static implicit operator Text(string s)
        {
            return new Text(s);
        }

        public static bool operator ==(Text left, Text right) => left.Equals(right);

        public static bool operator !=(Text left, Text right) => !left.Equals(right);

        public static bool operator ==(Text left, string right) => left.Equals(right);

        public static bool operator !=(Text left, string right) => !left.Equals(right);

        public static bool operator ==(string left, Text right) => left.Equals(right.Value);

        public static bool operator !=(string left, Text right) => !left.Equals(right.Value);

        public bool Equals(IValue other) => other is Text t && Equals(t);

        public bool Equals(Text other) => Value.Equals(other);

        public override bool Equals(object obj) => obj is Text t && Equals(t);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public int CompareTo(Text other)
        {
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            if (obj is Text t)
            {
                return CompareTo(t);
            }

            throw new ArgumentException($"Object must be of type {nameof(Text)}");
        }

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll)
        {
            string contents = Value
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\"", "\\\"");
            return $"\"{contents}\"";
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(Text)} {Inspect(false)}";
    }
}
