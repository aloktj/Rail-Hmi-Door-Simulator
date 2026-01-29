using System;
using Common.Can;

namespace Door.Host
{
    internal sealed class DummyBus : ICanBus
    {
        public bool IsConnected => true;
        public event Action<bool> ConnectionStateChanged;
        public event Action<CanFrame> FrameReceived;

        public void Send(CanFrame frame)
        {
            Console.WriteLine("[Door->BUS] " + frame);
        }

        public void Dispose() { }
    }
}
