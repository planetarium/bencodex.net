namespace Bencodex.Types
{
    /// <summary>
    /// The value to identify types of Bencodex values.
    /// </summary>
    public enum ValueType : byte
    {
        /// <summary>
        /// Null (<c>n</c>).
        /// </summary>
        Null = 0,

        /// <summary>
        /// True (<c>t</c>) or false (<c>f</c>).
        /// </summary>
        Boolean = 1,

        /// <summary>
        /// Integers (<c>i...e</c>).
        /// </summary>
        Integer = 2,

        /// <summary>
        /// Byte strings (<c>N:...</c>).
        /// </summary>
        Binary = 3,

        /// <summary>
        /// Unicode strings (<c>uN:...</c>).
        /// </summary>
        Text = 4,

        /// <summary>
        /// Lists (<c>l...e</c>).
        /// </summary>
        List = 5,

        /// <summary>
        /// Dictionaries (<c>d...e</c>).
        /// </summary>
        Dictionary = 6,
    }
}
