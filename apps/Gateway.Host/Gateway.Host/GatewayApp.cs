using System;
using Common.Can;

namespace Gateway.Host
{
    internal sealed class GatewayApp : IDisposable
    {
        private readonly ICanBus _bus;

        public event Action<CanFrame> FrameReceived;

        public GatewayApp(ICanBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bus.FrameReceived += OnFrameReceived;
        }

        private void OnFrameReceived(CanFrame frame)
        {
            FrameReceived?.Invoke(frame);
        }

        public void Dispose()
        {
            _bus.FrameReceived -= OnFrameReceived;
        }
    }
}
