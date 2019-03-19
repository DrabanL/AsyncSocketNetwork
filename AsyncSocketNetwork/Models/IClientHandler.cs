using System;
using System.Threading.Tasks;

namespace RabanSoft.AsyncSocketNetwork.Models {
    /// <summary>
    /// Handles socket client event operations.
    /// </summary>
    public interface IClientHandler {
        /// <summary>
        /// Handles closed socket client connection.
        /// </summary>
        Task OnConnectionClosedAsync(SocketClient client);

        /// <summary>
        /// Handles socket client async operation error.
        /// </summary>
        void OnReceiveError(SocketClient client, Exception ex);

        /// <summary>
        /// Handles socket client message.
        /// </summary>
        Task OnMessageAsync(SocketClient client, NetworkMessageHandler message);
    }
}
