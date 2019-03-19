using RabanSoft.AsyncSocketNetwork.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RabanSoft.AsyncSocketNetwork {
    /// <summary>
    /// Socket client implementation based on <see cref="SocketBase"/>.
    /// </summary>
    public class SocketClient : IDisposable {

        public EndPoint RemoteEndPoint => _client?.Client.RemoteEndPoint;
        public bool Connected => _client?.Connected ?? false;

        private TcpClient _client;
        private NetworkStream _stream;
        private readonly CancellationTokenSource _cancellationSource;

        public IClientHandler ClientHandler;
        public INetworkMessageSerializationHandler SerializationHandler;

        /// <summary>
        /// Initializes <see cref="Socket"/> with default options to prepare it for first use.
        /// </summary>
        public SocketClient() {
            _cancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Initializes <see cref="Socket"/> with an existing object.
        /// Usually should be used when the <see cref="Socket"/> is being managed externally (for example once Server receives a Socket connection).
        /// </summary>
        public SocketClient(Socket socket) {
            _cancellationSource = new CancellationTokenSource();
            _client = new TcpClient() { Client = socket };
            _stream = _client.GetStream();
        }

        /// <summary>
        /// Begins a connect operation on <see cref="Socket"/>.
        /// </summary>
        public async Task ConnectAsync(string host, int port) {
            _client = new TcpClient();

            await _client.ConnectAsync(host, port)
                .ConfigureAwait(false); // to avoid deadlock, continue the execution on any free thread at the time

            _stream = _client.GetStream();
        }

        /// <summary>
        /// Begins a receive operation on <see cref="Socket"/> using the <typeparamref name="T"/> message handler implementation
        /// </summary>
        public async Task BeginReceiveAsync<T>(CancellationToken? cancellationToken = null) where T : NetworkMessageHandler {
            try {
                if (SerializationHandler == null)
                    // SerializationHandler must be implemented to transform the network message from binary data to T
                    throw new ArgumentNullException(nameof(SerializationHandler));

                var messageHandler = Activator.CreateInstance<T>();
                while (true) {
                    // try receive data from connected socket
                    int bytesTransferred;
                    try {
                        bytesTransferred = await _stream.ReadAsync(messageHandler.Buffer, messageHandler.Offset, messageHandler.Length, cancellationToken ?? _cancellationSource.Token)
                            .ConfigureAwait(false); // to avoid deadlock, continue the execution on any free thread at the time
                    } catch (Exception ex) {
                        if (ex.InnerException is SocketException) {
                            var socketEx = ex.InnerException as SocketException;
                            if (socketEx.SocketErrorCode == SocketError.ConnectionReset ||
                                socketEx.SocketErrorCode == SocketError.ConnectionAborted) {
                                // 'ConnectionReset' will happen when the other party has closed the connection unexpectedly
                                ClientHandler?.OnConnectionClosedAsync(this);
                                break;
                            }
                        }

                        // unhandled
                        throw;
                    }

                    if (bytesTransferred == 0) {
                        // zero bytes means the connection has been closed
                        await ClientHandler?.OnConnectionClosedAsync(this);
                        break;
                    }

                    // process the received data in the message handler
                    var isReceiveCompleted = messageHandler.CompleteReceive(bytesTransferred);
                    if (isReceiveCompleted == null) {
                        // invoke the client handler to process the error in the implementation
                        ClientHandler?.OnReceiveError(this, new SocketException((int)SocketError.MessageSize));
                        continue;
                    }

                    if ((bool)isReceiveCompleted) {
                        // finalize (deobfuscation etc), deserialize to complete message, and process in protocol
                        await ClientHandler?.OnMessageAsync(this, SerializationHandler.Deserialize(messageHandler.GetFinalized()));

                        // message has been processed, so handler can be resetted
                        messageHandler.Reset();
                    }
                }
            } catch (Exception ex) {
                ClientHandler?.OnReceiveError(this, ex);
            }
        }

        /// <summary>
        /// Begins a send operation on <see cref="Socket"/> using the <typeparamref name="T"/> message handler implementation
        /// </summary>
        public async Task SendAsync<T>(T message, CancellationToken? cancellationToken = null) where T : NetworkMessageHandler {
            if (SerializationHandler == null)
                // SerializationHandler must be implemented to transform the network message into binary data
                throw new ArgumentNullException(nameof(SerializationHandler));

            // transform the network message into raw data and apply finalization (for encryption of data etc)
            message.SetFinalized(SerializationHandler.Serialize(message));

            // write to the stream asyncronously with a cancellation token
            await _stream.WriteAsync(message.Buffer, message.Offset, message.Length, cancellationToken ?? _cancellationSource.Token)
                .ConfigureAwait(false); // to avoid deadlock, continue the execution on any free thread at the time

            // make sure the data is fully written before continueing execution
            await _stream.FlushAsync(cancellationToken ?? _cancellationSource.Token)
                .ConfigureAwait(false); // to avoid deadlock, continue the execution on any free thread at the time
        }

        /// <summary>
        /// Signal both connected ends if socket closure
        /// </summary>
        public void Shutdown() {
            _client.Client.Shutdown(SocketShutdown.Both);
        }

        /// <summary>
        /// Cleans up locally managed objects.
        /// </summary>
        private void disposeManagedObjects() {
            try {
                using (_cancellationSource)
                    // cancel any running asyncronous operations
                    _cancellationSource?.Cancel(false);
            } catch {
                // catch all errors because we cannot break flow when being called from Dispose method.
            }

            try {
                // make sure the network stream is closed
                _stream?.Close();
                _stream = null;
            } catch {
                // catch all errors because we cannot break flow when being called from Dispose method.
            }

            try {
                using (_client)
                    _client?.Close();
            } catch {
                // catch all errors because we cannot break flow when being called from Dispose method.
            }
        }

        #region IDisposable Support
        protected bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {

                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                    disposeManagedObjects();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SocketBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
