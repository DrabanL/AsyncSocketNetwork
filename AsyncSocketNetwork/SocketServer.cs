using AsyncSocketNetwork.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AsyncSocketNetwork {
    /// <summary>
    /// Socket server implementation based on <see cref="SocketBase"/>.
    /// </summary>
    public class SocketServer {

        private TcpListener _listener;

        /// <summary>
        /// Siglans if the tcp listener has been requested to teminate.
        /// </summary>
        protected bool _terminationSwitch;

        public IServerHandler ServerHandler;

        public SocketServer() { }

        /// <summary>
        /// Binds and asynchronous starts a listener on <paramref name="port"/> with a limit of <paramref name="backlog"/> connections.
        /// </summary>
        public async Task StartAsync(int port, int backlog) {
            // stop if listener is already bound
            if (_listener?.Server.IsBound ?? false)
                Stop();

            // reset invoke switch, because this class can be reused
            _terminationSwitch = false;

            _listener = TcpListener.Create(port);
            _listener.Start(backlog);

            await acceptAsync();
        }

        /// <summary>
        /// Start accepting connections on <see cref="Socket"/>
        /// </summary>
        private async Task acceptAsync() {
            // indefinitely process new connections, until exception is thrown on AcceptSocketAsync() when the listener is stopped
            while (true) {
                Socket socket;

                try {
                    // wait for new connection
                    socket = await _listener.AcceptSocketAsync()
                        .ConfigureAwait(false); // to avoid deadlock, continue the execution on any free thread at the time
                } catch (Exception ex) {
                    // report error only when the exception is not intentional
                    if (!_terminationSwitch)
                        // invoke the client handler to process the error in the implementation
                        ServerHandler?.OnAcceptError(ex);

                    // exception on AcceptSocketAsync means there is an error with the listener itself, so must terminate  
                    break;
                }

                // invoke the connection handler to process the socket connection in implementation
                ServerHandler?.OnSocketAccepted(socket);
            }
        }

        /// <summary>
        /// Terminates the listener. (<see cref="SocketServer"/> cannot be reused afterwards)
        /// </summary>
        public virtual void Stop() {
            // signal that any exceptions on Accept loop are intentional
            _terminationSwitch = true;

            _listener?.Stop();
        }
    }
}
