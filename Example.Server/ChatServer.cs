using AsyncSocketNetwork.Example.Utilities.Models;
using AsyncSocketNetwork.Models;
using SocketNetwork.Example.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AsyncSocketNetwork.Example.Server {
    /// <summary>
    /// Manages chat server by extending <see cref="SocketServer"/> and handles its connection/protocol with clients by implementing <see cref="IServerHandler"/> and <seealso cref="IClientHandler"/>.
    /// </summary>
    internal class ChatServer : SocketServer, IServerHandler, IClientHandler {
        /// <summary>
        /// Stores all managed connections (participants in the chat conversation).
        /// </summary>
        private readonly ThreadSafeList<ChatMember> _clients = new ThreadSafeList<ChatMember>();

        /// <summary>
        /// Initializes the server object and registers handlers.
        /// </summary>
        public ChatServer() : base() {
            _clients.Clear();

            // register the server handler (process new connections) to be managed in local class
            ServerHandler = this;
        }

        /// <summary>
        /// Handle error on async accept operation.
        /// </summary>
        void IServerHandler.OnAcceptError(Exception ex) {
            Console.WriteLine($"Server accept error! {ex}");
        }

        /// <summary>
        /// Handle received connection on the socket object.
        /// </summary>
        async void IServerHandler.OnSocketAccepted(Socket socket) {
            Console.WriteLine($"(Server) client connected {socket.RemoteEndPoint}");

            // create new chat member from received socket
            using (var client = new ChatMember(socket)) {
                // add the new connection to managed list
                _clients.Add(client);

                // register the client handler (process chat messages) to be managed in local class
                client.ClientHandler = this;

                // use the chat system serializer
                client.SerializationHandler = ChatMessageSerializer.Instance;

                // begin receving data from chat member, in a ChatMessage object that extends NetworkMessageHandler
                await client.BeginReceiveAsync<ChatMessage>();

                // remove the member from managed connections
                _clients.Remove(client);
            }
        }

        /// <summary>
        /// Sends a chat message to a specific chat member.
        /// </summary>
        private async Task sendAllMessage(string sender, string message, params string[] exclusions) {
           await Task.WhenAll(_clients
                .Where(v => v.Name != null && !exclusions.Contains(v.Name)) // all connected clients excluding specified members by name
                .Select(client => client.SendAsync(new ChatMessage() { // construct a new ChatMessage object that extends NetworkMessageHandler
                    OpCode = (byte)OpCodes.ConversationMessage,
                    Sender = sender,
                    Text = message
                })));
        }

        /// <summary>
        /// Sends a chat message to a specific chat member.
        /// </summary>
        private async Task sendMessage(string to, string sender, string message) {
            await _clients.Where(v => v.Name?.Equals(to) ?? false) // find by name (fails if null)
                .First() // should only be one result
                .SendAsync(new ChatMessage() { // construct a new ChatMessage object that extends NetworkMessageHandler
                    OpCode = (byte)OpCodes.ConversationMessage,
                    Sender = sender,
                    Text = message
                });
        }

        /// <summary>
        /// Processes the closed chat member connection.
        /// </summary>
        private async Task processClosedConnectionAsync(ChatMember client) {
            Console.WriteLine($"(Server) client disconnected {client.RemoteEndPoint}");

            // notify all participants of the leave (excluding the member that left)
            await sendAllMessage("Officer", $"{client.Name} has left the conversation.", client.Name);
        }

        /// <summary>
        /// Processes the error that occured in the async operation.
        /// </summary>
        private void processReceiveError(ChatMember client, Exception ex) {
            Console.WriteLine($"(Server) client receive error: {ex}");
        }

        /// <summary>
        /// Processes chat system protocol received from chat member.
        /// </summary>
        private async Task processChatMessageAsync(ChatMember client, ChatMessage message) {
            switch ((OpCodes)message.OpCode) {
                case OpCodes.ConversationJoin:
                    // identify the chat member with a nickname
                    client.Name = message.Sender;

                    // notify all chat participants of the new joined member (excluding the joined member)
                    await sendAllMessage("Officer", $"{message.Sender} has joined the conversation.", message.Sender);

                    // send a welcome message to the new participant
                    await sendMessage(message.Sender, $"Officer", $"Welcome {message.Sender}!");
                    break;
                case OpCodes.ConversationMessage:
                    // forward the chat message to all participants in the conversation
                    await sendAllMessage(message.Sender, message.Text);
                    break;
            }
        }

        public override void Stop() {
            base.Stop();

            // make sure all connections are notified of server close
            foreach (var client in _clients)
                // signal the cleint to end the connection on its end
                client.Shutdown();
        }

        /// <summary>
        /// Handles closed socket client connection.
        /// </summary>
        Task IClientHandler.OnConnectionClosedAsync(SocketClient client)
            // SocketClient is actually ChatMember so it can be casted to our own type
            => processClosedConnectionAsync(client as ChatMember);

        /// <summary>
        /// Handles socket client async receive operation error.
        /// </summary>
        void IClientHandler.OnReceiveError(SocketClient client, Exception ex)
            // SocketClient is actually ChatMember so it can be casted to our own type
            => processReceiveError(client as ChatMember, ex);

        /// <summary>
        /// Handles socket client message.
        /// </summary>
        Task IClientHandler.OnMessageAsync(SocketClient client, NetworkMessageHandler message)
            // SocketClient is actually ChatMember so it can be casted to our own type
            => processChatMessageAsync(client as ChatMember, message as ChatMessage);
    }
}
