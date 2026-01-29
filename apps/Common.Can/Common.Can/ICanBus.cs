using System;

namespace Common.Can
{
    /// <summary>
    /// Abstraction for a CAN transport. Can be backed by PCAN, TCP gateway, named pipes, etc.
    /// </summary>
    public interface ICanBus : IDisposable
    {
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