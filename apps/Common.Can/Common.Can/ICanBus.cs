using System;

namespace Common.Can
{
    /// <summary>
    /// Abstraction for a CAN transport. Can be backed by PCAN, TCP gateway, named pipes, etc.
    /// </summary>
    public interface ICanBus : IDisposable
    {
        /// <summary>
        /// Indicates if the transport is currently connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Raised when the connection state changes.
        /// </summary>
        event Action<bool> ConnectionStateChanged;

        /// <summary>
        /// Raised when a CAN frame is received from the transport.
        /// </summary>
        event Action<CanFrame> FrameReceived;

        /// <summary>
        /// Sends a CAN frame via the transport.
        /// </summary>
        void Send(CanFrame frame);
    }
}
