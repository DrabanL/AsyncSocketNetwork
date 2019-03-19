using RabanSoft.AsyncSocketNetwork.Default;
using System;
using System.Threading.Tasks;

namespace RabanSoft.AsyncSocketNetwork.Example.Server {
    internal class Program {
        private static async Task Main(string[] args) {
            Console.WriteLine("Chat server starting..");
            var chatServer = new ChatServer();
            
            // start listening for connections on tcp port 44485 with up to 100 pending connections
            var listenerTask = chatServer.StartAsync(44485, 100);

            Console.WriteLine("Chat server running.");
            Console.WriteLine("Press any key to abort.");

            // block thread to allow chat server to continue running
            Console.ReadKey(true);

            Console.WriteLine("Chat server stopping..");
            // close the listener
            chatServer.Stop();

            // must await the async operation, because it may have been faulted, so need to observe it
            await listenerTask;

            Console.WriteLine("Chat server closed.");
            Console.WriteLine("Press any key to exit..");
            Console.ReadKey(true);
        }
    }
}
