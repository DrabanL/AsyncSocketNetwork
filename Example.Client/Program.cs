using AsyncSocketNetwork.Default;
using AsyncSocketNetwork.Example.Utilities.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSocketNetwork.Example.Client {
    internal class Program {
        private static string _nickname;
        private static ChatClient _chatClient;

        private static async Task Main(string[] args) {
            // join the conversation with a nickname
            Console.WriteLine("Nickname: ");
            Console.Title = _nickname = Console.ReadLine();

            using (_chatClient = new ChatClient(_nickname)) {
                Console.WriteLine("connecting to server..");

                // we expect the chat server to run locally in port 44485
                await _chatClient.ConnectAsync("localhost", 44485);

                // now that we are connected to server, can send the 'Join' command
                await _chatClient.SendJoin();

                // get chat input on different thread, not to block our execution when doing Console.ReadLine
                var chatInputJob = acceptChatInputAsync();

                // begin receving data from server, in a ChatMessage object that extends NetworkMessageHandler
                // block execution until socket is disconnected/faulted
                await _chatClient.BeginReceiveAsync<ChatMessage>();

                Console.WriteLine("disconnected, press Enter to exit");

                // wait and observe exceptions on the chat input job
                await chatInputJob;
            }
        }

        private static async Task acceptChatInputAsync() {
            // let the caller continue its execution
            await Task.Yield();

            Console.WriteLine("Write msg: ");
            while (true) {
                var msg = await Console.In.ReadLineAsync()
                    .ConfigureAwait(false); // to avoid deadlock, continue the execution on any free thread at the time

                // continue processing input until user writes 'exit'
                if (msg.Equals("exit", StringComparison.InvariantCultureIgnoreCase)) {
                    // signal the server to end the connection on its end
                    _chatClient.Shutdown();
                    break;
                }

                if (!await _chatClient.SendMessage(_nickname, msg))
                    // socket is not connected, so end input loop
                    break;
            }
        }
    }
}
