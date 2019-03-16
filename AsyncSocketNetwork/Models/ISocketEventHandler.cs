using System.Net.Sockets;

namespace AsyncSocketNetwork.Models {
    /// <summary>
    /// Defines a method for managing the AsyncEvent objects.
    /// </summary>
    public interface ISocketEventHandler {
        /// <summary>
        /// Returns a SocketAsyncEventArgs to be used by the Socket for communication.
        /// </summary>
        SocketAsyncEventArgs GetSocketEvent();

        /// <summary>
        /// Provides a SocketAsyncEventArgs object that was previously received by <see cref="GetSocketEvent"/> once it is not used anymore.
        /// </summary>
        void ReturnSocketEvent(SocketAsyncEventArgs e);
    }
}
