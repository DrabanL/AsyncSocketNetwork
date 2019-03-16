using AsyncSocketNetwork.Default;

namespace AsyncSocketNetwork.Example.Utilities.Models {
    /// <summary>
    /// Chat System Protocol message object that extends <see cref="EncryptedProtocolMessage"/> with properites relevant for the chat system.
    /// </summary>
    public class ChatMessage : EncryptedProtocolMessage {
        /// <summary>
        /// The chat protocol operation code.
        /// </summary>
        public byte OpCode;

        /// <summary>
        /// The chat message sender.
        /// </summary>
        public string Sender;

        /// <summary>
        /// The chat message text.
        /// </summary>
        public string Text;
    }
}
