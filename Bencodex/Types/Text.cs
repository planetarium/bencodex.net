using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace Bencodex.Types
{
    public struct Text :
        IKey,
        IEquatable<Text>,
        IComparable<string>,
        IComparable<Text>,
        IEquatable<string>,
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

        public static bool operator ==(Text left, Text right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Text left, Text right)
        {
            return !left.Equals(right);
        }

        bool IEquatable<string>.Equals(string other)
        {
            return other != null && Value.Equals(other);
        }

        bool IEquatable<Text>.Equals(Text other) => Value.Equals(other);

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Text o && ((IEquatable<Text>)this).Equals(o);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Text txt:
                    return ((IEquatable<Text>)this).Equals(txt);
                case string str:
                    return ((IEquatable<string>)this).Equals(str);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        int IComparable<string>.CompareTo(string other)
        {
            return string.Compare(Value, other, StringComparison.Ordinal);
        }

        int IComparable<Text>.CompareTo(Text other)
        {
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Text txt:
                    return ((IComparable<Text>)this).CompareTo(txt);
                case string str:
                    return ((IComparable<string>)this).CompareTo(str);
                default:
                    throw new ArgumentException(
                        "the argument is neither Text nor String",
                        nameof(obj)
                    );
            }
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
