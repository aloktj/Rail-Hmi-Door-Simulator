using System;
using Common.Can;

namespace Hmi.Host
{
    internal sealed class HmiApp : IDisposable
    {
        private readonly ICanBus _bus;

        public HmiApp(ICanBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bus.FrameReceived += OnFrameReceived;
        }

        private void OnFrameReceived(CanFrame frame)
        {
            // Placeholder decode: Data[0]=doorId, Data[1]=state
            if (frame.Data == null || frame.Data.Length < 2) return;

            int doorId = frame.Data[0];
            byte state = frame.Data[1];

            Console.WriteLine($"[HMI] Door {doorId} state={state}  (from ID=0x{frame.Id:X})");
        }

        public void Dispose()
        {
            _bus.FrameReceived -= OnFrameReceived;
        }
    }
}