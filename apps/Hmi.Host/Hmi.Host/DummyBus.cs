using System;
using Common.Can;

namespace Hmi.Host
{
    internal sealed class DummyBus : ICanBus
    {
        public bool IsConnected => true;
        public event Action<bool> ConnectionStateChanged;
        public event Action<CanFrame> FrameReceived;

        public void Send(CanFrame frame)
        {
            Console.WriteLine("[HMI->BUS] " + frame);
        }

        // Temporary helper so you can simulate receive
        public void Inject(CanFrame frame) => FrameReceived?.Invoke(frame);

        public void Dispose() { }
    }
}
