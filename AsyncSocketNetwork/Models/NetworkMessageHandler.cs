namespace AsyncSocketNetwork.Models {
    /// <summary>
    /// Method for handling partial send/receive operations and transforming to/from full network messages.
    /// </summary>
    public abstract class NetworkMessageHandler {
        /// <summary>
        /// Temporary buffer used in send/receive operation.
        /// </summary>
        internal byte[] Buffer { get; set; }

        /// <summary>
        /// Specifies the position at which to start the send/receive operation on the <see cref="Buffer"/>.
        /// </summary>
        internal int Offset { get; set; }

        /// <summary>
        /// Specifies the length of to send/receive from/to <see cref="Buffer"/>.
        /// </summary>
        internal int Length { get; set; }

        /// <summary>
        /// Advances <see cref="Offset"/> based on <paramref name="len"/>, and returns true if all the message has been sent. otherwise False.
        /// </summary>
        internal abstract bool CompleteSend(int len);

        /// <summary>
        /// Processes data from <see cref="Buffer"/> based on <paramref name="len"/>, and returns false if there is still data needed to complete the message. Returns null if the message size exeeds the maximum. otherwise False.
        /// </summary>
        internal abstract bool? CompleteReceive(int len);

        /// <summary>
        /// Resets all objects to their initial state to prepare for re-use.
        /// </summary>
        internal abstract void Reset();

        /// <summary>
        /// Returns the final data to be used in the protocol implementation after the send operation is completed.
        /// </summary>
        public abstract byte[] GetFinalized();

        /// <summary>
        /// Initializes <see cref="Buffer"/>, <see cref="Offset"/> and <see cref="Length"/> to prepare to be used in a send operation based on the data provided.
        /// </summary>
        public abstract void SetFinalized(byte[] data);
    }
}
