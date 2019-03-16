using AsyncSocketNetwork.Models;
using System.IO;
using System.Text;

namespace AsyncSocketNetwork.Example.Utilities.Models {
    /// <summary>
    /// Chat System Protocol message serialization implementation that extends <see cref="INetworkMessageSerializationHandler"/>.
    /// The message structure is as follows:
    /// [0] OperationCode
    /// [1..X] Sender (X is null terminated string)
    /// [X..Y] *Text (Y is null terminated string) *Optional
    /// </summary>
    public class ChatMessageSerializer : INetworkMessageSerializationHandler {
        /// <summary>
        /// Reference for the serializer of data.
        /// </summary>
        public static readonly ChatMessageSerializer Instance = new ChatMessageSerializer();

        /// <summary>
        /// Transforms raw data received from socket to chat message object.
        /// </summary>
        private ChatMessage onDeserialize(byte[] message) {
            // initialize the stream with unicode encoding to support nickname/text with more languages
            using (var reader = new BinaryReader(new MemoryStream(message), Encoding.Unicode)) {
                var msg = new ChatMessage {
                    OpCode = reader.ReadByte(),
                    Sender = reader.ReadString()
                };

                if ((OpCodes)msg.OpCode == OpCodes.ConversationMessage)
                    // message text is only relevent for this specific operation
                    msg.Text = reader.ReadString();

                return msg;
            }
        }

        /// <summary>
        /// Transforms chat message object to raw data to be processed in socket.
        /// </summary>
        private byte[] onSerialize(ChatMessage message) {
            // initialize the stream with unicode encoding to support nickname/text with more languages
            using (var writer = new BinaryWriter(new MemoryStream(), Encoding.Unicode)) {
                // write all the chat message fields to the stream

                writer.Write(message.OpCode);
                writer.Write(message.Sender);

                if ((OpCodes)message.OpCode == OpCodes.ConversationMessage)
                    // message text is only relevent for this specific operation
                    writer.Write(message.Text);

                // flush the data to the stream before returning stream contents
                writer.Flush();

                return (writer.BaseStream as MemoryStream).ToArray();
            }
        }
        
        NetworkMessageHandler INetworkMessageSerializationHandler.Deserialize(byte[] message)
            // cast return object to the base class
            => onDeserialize(message);

        byte[] INetworkMessageSerializationHandler.Serialize(NetworkMessageHandler message)
            // cast message object to chat message
            => onSerialize(message as ChatMessage);
    }
}
