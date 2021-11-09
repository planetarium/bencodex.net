namespace Bencodex.Types
{
    internal static class CommonVariables
    {
        internal static readonly byte[] Separator = new byte[1] { 0x3a };  // ':'

        internal static readonly byte[] Suffix = new byte[1] { 0x65 };  // 'e'
    }
}
