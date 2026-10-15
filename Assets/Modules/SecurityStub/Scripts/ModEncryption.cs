using System;
using System.IO;

namespace Security
{
    public static class ModEncryption
    {
        public const int WrappedKeySize = 384;
        public const int NonceSize = 12;
        public const int AuthenticationTagSize = 16;

        public static byte[] Decrypt(
            byte[] wrappedKey,
            byte[] nonce,
            byte[] authenticationTag,
            byte[] encryptedData,
            byte[] associatedData)
        {
            throw new NotSupportedException(
                "Authenticated packed mods require the commercial Security module.");
        }
    }
}
