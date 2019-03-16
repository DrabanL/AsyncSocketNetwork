using AsyncSocketNetwork.Example.Utilities.Models;
using AsyncSocketNetwork.Models;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AsyncSocketNetwork.Example.Client {
    /// <summary>
    /// Manages the chat client communication with server by extending <see cref="SocketClient"/> and implementing <see cref="IClientHandler"/>.
    /// </summary>
    internal class ChatClient : SocketClient, IClientHandler {
        /// <summary>
        /// The nickname that the user chose for himself.
        /// </summary>
        private readonly string _nickname;

        /// <summary>
        /// Initialize the chat client with the user's nickname.
        /// </summary>
        /// <param name="nickname"></param>
        public ChatClient(string nickname) : base() {
            _nickname = nickname;

            // implement the server communication in this class
            ClientHandler = this;

            // use the chat system serializer
            SerializationHandler = ChatMessageSerializer.Instance;
        }

        /// <summary>
        /// Sends a 'Join' protocol command to chat server.
        /// </summary>
        public async Task SendJoin() {
            await SendAsync(new ChatMessage() { // construct a new ChatMessage object that extends NetworkMessageHandler
                OpCode = (byte)OpCodes.ConversationJoin, // the 'Join' command code
                Sender = _nickname // the nickname that the user has chosen
            });
        }

        /// <summary>
        /// Sends a 'Message' protocol command to chat server. Returns false if socket is not connected. otherwise true.
        /// </summary>
        internal async Task<bool> SendMessage(string sender, string msg) {
            if (!Connected)
                // the socket may have been closed previously but it was not managed in input context
                return false;

            await SendAsync(new ChatMessage() { // construct a new ChatMessage object that extends NetworkMessageHandler
                OpCode = (byte)OpCodes.ConversationMessage,
                Sender = sender, // the nickname that the user has chosen
                Text = msg // the message text
            });

            return true;
        }

        /// <summary>
        /// Processes the closed chat client connection.
        /// </summary>
        private async Task processClosedConnectionAsync(ChatClient client) {
            // let the caller continue its execution, regardless of our implementation, because we do not execute any meaningful async operations
            await Task.Yield();

            // make sure the socket is shut down
            Shutdown();

            Console.WriteLine("server connection closed");
        }

        /// <summary>
        /// Processes the error that occured in the async operation.
        /// </summary>
        private void processReceiveError(ChatClient client, Exception ex) {
            Console.WriteLine($"receive error! {ex}");

            if (ex is SocketException) {
                if ((ex as SocketException).SocketErrorCode == SocketError.MessageSize) {
                    Console.WriteLine("Message size limit reached.");

                    client.Shutdown();
                }
            }
        }

        /// <summary>
        /// Processes chat system protocol received from server.
        /// </summary>
        private async Task processChatMessageAsync(ChatClient client, ChatMessage message) {
            // let the caller continue its execution, regardless of our implementation, because we do not execute any meaningful async operations
            await Task.Yield();

            switch ((OpCodes)message.OpCode) {
                case OpCodes.ConversationMessage:
                    // write the chat message received from server
                    Console.WriteLine($"[{message.Sender}]: {message.Text}");
                    break;
            }
        }

        /// <summary>
        /// Handles closed socket client connection.
        /// </summary>
        Task IClientHandler.OnConnectionClosedAsync(SocketClient client)
            // SocketClient is actually ChatClient so it can be casted to our own type
            => processClosedConnectionAsync(client as ChatClient);
         
        /// <summary>
        /// Handles socket client message.
        /// </summary>
        Task IClientHandler.OnMessageAsync(SocketClient client, NetworkMessageHandler message)
            // SocketClient is actually ChatClient so it can be casted to our own type
            => processChatMessageAsync(client as ChatClient, message as ChatMessage);

        /// <summary>
        /// Handles socket client async operation error.
        /// </summary>
        void IClientHandler.OnReceiveError(SocketClient client, Exception ex)
            // SocketClient is actually ChatClient so it can be casted to our own type
            => processReceiveError(client as ChatClient, ex);
    }
}
