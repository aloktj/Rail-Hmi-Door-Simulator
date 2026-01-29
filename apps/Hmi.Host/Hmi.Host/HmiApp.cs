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
            if (!DoorStateFrame.TryParse(frame, out var doorId, out var state))
            {
                return;
            }

            Console.WriteLine($"[HMI] Door {doorId} state={state}  (from ID=0x{frame.Id:X})");
        }

        public void Dispose()
        {
            _bus.FrameReceived -= OnFrameReceived;
        }
    }
}
