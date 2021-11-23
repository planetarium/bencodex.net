using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    /// <summary>
    /// Represents Bencodex values which can be keys of a Bencodex <see cref="Dictionary"/>.
    /// <para>It does not have extra ability over <see cref="IValue"/>, but just purposes to
    /// group types can be keys of a Bencodex <see cref="Dictionary"/>.</para>
    /// </summary>
    /// <seealso cref="Binary"/>
    /// <seealso cref="Text"/>
    public interface IKey : IValue
    {
    }
}
