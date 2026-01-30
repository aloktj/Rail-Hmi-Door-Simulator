using System;
using System.Collections.ObjectModel;
using System.Windows;
using Common.Can;

namespace Gateway.Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GatewayApp _gatewayApp;
        private readonly ObservableCollection<FrameViewModel> _frames = new ObservableCollection<FrameViewModel>();

        public MainWindow(GatewayApp gatewayApp)
        {
            _gatewayApp = gatewayApp ?? throw new ArgumentNullException(nameof(gatewayApp));
            InitializeComponent();

            FrameList.ItemsSource = _frames;
            ConnectionStatusText.Text = "Connection: --";

            _gatewayApp.FrameReceived += OnFrameReceived;
            Closed += (_, __) => _gatewayApp.FrameReceived -= OnFrameReceived;
        }

        public void UpdateConnectionStatus(bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                ConnectionStatusText.Text = connected ? "Connection: Connected" : "Connection: Disconnected";
            });
        }

        private void OnFrameReceived(CanFrame frame)
        {
            Dispatcher.Invoke(() =>
            {
                _frames.Insert(0, new FrameViewModel(frame));
                if (_frames.Count > 200)
                {
                    _frames.RemoveAt(_frames.Count - 1);
                }
            });
        }

        private sealed class FrameViewModel
        {
            public FrameViewModel(CanFrame frame)
            {
                Timestamp = frame.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff");
                CanId = $"0x{frame.Id:X}";
                Dlc = frame.Dlc.ToString();
                Data = frame.Data == null ? string.Empty : BitConverter.ToString(frame.Data);
            }

            public string Timestamp { get; }
            public string CanId { get; }
            public string Dlc { get; }
            public string Data { get; }
        }
    }
}
