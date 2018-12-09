using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    public interface IKey : IValue
    {
        [Pure]
        byte[] EncodeAsByteArray();

        [Pure]
        byte? KeyPrefix { get; }
    }
}
