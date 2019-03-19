namespace RabanSoft.AsyncSocketNetwork.Models {
    /// <summary>
    /// Handles transformation of raw data to and from network message object.
    /// </summary>
    public interface INetworkMessageSerializationHandler {
        /// <summary>
        /// Transforms raw data received from socket to managed network message object.
        /// </summary>
        NetworkMessageHandler Deserialize(byte[] message);

        /// <summary>
        /// Transforms managed network message object to raw data to be sent in socket.
        /// </summary>
        byte[] Serialize(NetworkMessageHandler message);
    }
}
