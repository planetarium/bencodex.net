using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace Bencodex.Types
{
    public class Text :
        IKey,
        IEquatable<Text>,
        IComparable<string>,
        IComparable<Text>,
        IEquatable<string>,
        IComparable
    {
        public static readonly Text Empty = new Text(string.Empty);

        private int _utf8Length;
        private string _value;

        public Text(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _utf8Length = -1;
        }

        public string Value => _value;

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.Text;

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength =>
            1L + // 'l'
            Utf8Length.ToString(CultureInfo.InvariantCulture).Length +
            1L + // 'e'
            Utf8Length;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

        [Pure]
        internal int Utf8Length => _utf8Length >= 0
            ? _utf8Length
            : _utf8Length = Encoding.UTF8.GetByteCount(Value);

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

        bool IEquatable<string>.Equals(string other) =>
            other is { } o && Value.Equals(o);

        bool IEquatable<Text>.Equals(Text other) => Value.Equals(other);

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Text o && ((IEquatable<Text>)this).Equals(o);

        public override bool Equals(object obj) =>
            obj is Text t
                ? Value.Equals(t)
                : obj is string s
                    ? Value.Equals(s)
                    : false;

        public override int GetHashCode() =>
            Value.GetHashCode();

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
