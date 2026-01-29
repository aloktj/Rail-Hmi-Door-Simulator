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

        private DoorState _state = DoorState.Closed;

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
            _state = (DoorState)(((int)_state + 1) % 3);

            var frame = DoorStateFrame.Create((byte)_doorId, _state);
            _bus.Send(frame);
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
