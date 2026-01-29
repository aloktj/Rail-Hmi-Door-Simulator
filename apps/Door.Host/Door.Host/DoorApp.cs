using System;
using System.Timers;
using Common.Can;

namespace Door.Host
{
    internal sealed class DoorApp : IDisposable
    {
        private readonly ICanBus _bus;
        private readonly int _doorId;
        private readonly Timer _timer;

        private byte _state = 0; // 0=Closed, 1=Open, 2=Obstructed (placeholder)

        public DoorApp(ICanBus bus, int doorId)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _doorId = doorId;

            // Periodic status publish (placeholder rate)
            _timer = new Timer(500);
            _timer.Elapsed += (_, __) => PublishStatus();
            _timer.AutoReset = true;
        }

        public void Start()
        {
            _timer.Start();
        }

        private void PublishStatus()
        {
            // Simple demo: cycle states each tick
            _state = (byte)((_state + 1) % 3);

            var frame = new CanFrame
            {
                Id = 0x200u + (uint)_doorId,     // example CAN ID per door
                Dlc = 2,
                Data = new byte[] { (byte)_doorId, _state },
                Timestamp = DateTime.UtcNow
            };

            _bus.Send(frame);
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}