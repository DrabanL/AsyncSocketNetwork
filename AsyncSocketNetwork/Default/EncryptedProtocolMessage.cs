namespace AsyncSocketNetwork.Default {
    /// <summary>
    /// Simple XOR obfuscation implementation on top of <see cref="ProtocolMessage"/>
    /// </summary>
    public class EncryptedProtocolMessage : ProtocolMessage {
        /// <summary>
        /// Deobfuscates the network message before it forwarded to <see cref="ProtocolMessage"/> and protocol implementation.
        /// </summary>
        public override byte[] GetFinalized() {
            var data = base.GetFinalized();

            // deobfuscate the data before the finalized packet is being processed
            MessageEncryption.Xor(data);

            return data;
        }

        /// <summary>
        /// Obfuscates the data before it is forwarded to <see cref="ProtocolMessage"/>.
        /// </summary>
        public override void SetFinalized(byte[] messageData) {
            // obfuscate the data before its being finalized to be sent
            MessageEncryption.Xor(messageData);

            base.SetFinalized(messageData);
        }
    }
}
