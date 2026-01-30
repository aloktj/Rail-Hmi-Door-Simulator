using System;
using System.Windows;
using Common.Can;

namespace Door.Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DoorApp _doorApp;
        private readonly int _doorId;

        public MainWindow(DoorApp doorApp, int doorId)
        {
            _doorApp = doorApp ?? throw new ArgumentNullException(nameof(doorApp));
            _doorId = doorId;
            InitializeComponent();

            DoorIdText.Text = $"Door ID: {_doorId}";
            StateText.Text = "State: Starting...";
            LastUpdateText.Text = "Last update: --";

            _doorApp.StatePublished += OnStatePublished;
            Closed += (_, __) => _doorApp.StatePublished -= OnStatePublished;
        }

        private void OnStatePublished(DoorState state)
        {
            Dispatcher.Invoke(() =>
            {
                StateText.Text = $"State: {state}";
                LastUpdateText.Text = $"Last update: {DateTime.Now:HH:mm:ss}";
            });
        }
    }
}
