namespace AsyncSocketNetwork.Default {
    /// <summary>
    /// Includes functions related to encryption of a <see cref="AsyncSocketNetwork.Internals.SocketBase"/> message.
    /// </summary>
    internal static class MessageEncryption {
        /// <summary>
        /// The key used for obfuscation of the data.
        /// </summary>
        private static byte[] _key = new byte[] { 0xFA, 0x01, 0xC5 };

        /// <summary>
        /// Obfuscate or Deobfuscate the data.
        /// </summary>
        /// <param name="data"></param>
        public static void Xor(byte[] data) {
            for (int i = 0; i < data.Length; ++i)
                data[i] ^= _key[i % _key.Length];
        }
    }
}
