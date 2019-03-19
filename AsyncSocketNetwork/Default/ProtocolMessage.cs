using RabanSoft.AsyncSocketNetwork.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RabanSoft.AsyncSocketNetwork.Default {
    /// <summary>
    /// Simple protcol message format structured as:
    /// <para>[0..3] Length (uint)</para>
    /// <para>[4..] Message Data</para>
    /// </summary>
    public class ProtocolMessage : NetworkMessageHandler {

        /// <summary>
        /// Container for the complete network message.
        /// </summary>
        private List<byte> _messageData;

        /// <summary>
        /// The size of the complete network message.
        /// </summary>
        private uint? _messageSize;

        /// <summary>
        /// The maximum allowed size of a complete message.
        /// </summary>
        private readonly uint _maxMessageSize;

        public ProtocolMessage(int bufferSize = 512, uint maxMessageSize = 0x1024) {
            _maxMessageSize = maxMessageSize;
            _messageData = new List<byte>();

            Buffer = new byte[bufferSize];

            // make sure all the objects are initialized correctly
            Reset();
        }

        internal override void Reset() {
            // initial expected size to receive is 4 bytes that will indicate the total message size to receive
            Length = 4;

            // set the offset to 0 to start send/receive from the beginning
            Offset = 0;

            _messageData.Clear();
            _messageSize = null;
        }

        internal override bool CompleteSend(int len) {
            // advance the internal offset by the length of data that was sent
            Offset += len;

            // calculate the remaining data length to be sent (if any)
            Length = Buffer.Length - Offset;

            // the send operation is completed if the offset is the position of the last element in the buffer
            return Offset == Buffer.Length;
        }

        internal override bool? CompleteReceive(int len) {
            // add the data that was received in this operation to our internal buffer
            _messageData.AddRange(Buffer.Take(len));

            if (_messageSize == null) {
                // the object is in "Reset" state and should be read the length of total message to receive

                // make sure that at least 4 bytes have been received in the message buffer, which should be the message size to receive
                var remRecv = 4 - _messageData.Count;
                if (remRecv > 0) {
                    // continue receive until we have 4 bytes in internal buffer
                    Length = remRecv;
                    return false;
                }

                // we expect the first 4 bytes in the buffer to be the total message size to be received
                _messageSize = BitConverter.ToUInt32(_messageData.Take(4).ToArray(), 0);

                // clear the internal message buffer from the first 4 bytes
                _messageData.Clear();

                if (_messageSize > _maxMessageSize)
                    // message size is over the limit, so stop overall receive.
                    return null;
            }

            // calculate the remaining data that should be received to complete the network message
            var remSize = _messageSize - _messageData.Count;

            // make sure that the total message size to be received is not greater than our internal buffer - and receive the lowest value from both
            Length = remSize > Buffer.Length ? Buffer.Length : (int)remSize;

            // the receive operation is completed if there is no remaining data expected to be received to complete the network message
            return remSize == 0;
        }

        public override byte[] GetFinalized() {
            // return the message buffer
            return _messageData.ToArray();
        }

        public override void SetFinalized(byte[] messageData) {
            var packet = new List<byte>();

            // the first 4 bytes in the packet should be the total size of the network message
            packet.AddRange(BitConverter.GetBytes((uint)messageData.Length));

            // the rest packet is the message itself
            packet.AddRange(messageData);

            Buffer = packet.ToArray();
            Length = Buffer.Length;

            // set the offset to 0 to start send from the beginning
            Offset = 0;
        }
    }
}
