using System;
using System.Net.Sockets;

namespace RabanSoft.AsyncSocketNetwork.Models {
    /// <summary>
    /// Handles socket server incoming connections.
    /// </summary>
    public interface IServerHandler {
        /// <summary>
        /// Handles error on client accept operation.
        /// </summary>
        void OnAcceptError(Exception ex);

        /// <summary>
        /// Handles accepted connection.
        /// </summary>
        void OnSocketAccepted(Socket client);
    }
}
