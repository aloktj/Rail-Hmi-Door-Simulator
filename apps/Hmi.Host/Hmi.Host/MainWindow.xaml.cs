using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Common.Can;

namespace Hmi.Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HmiApp _hmiApp;
        private readonly ObservableCollection<DoorStatusViewModel> _doors = new ObservableCollection<DoorStatusViewModel>();

        public MainWindow(HmiApp hmiApp)
        {
            _hmiApp = hmiApp ?? throw new ArgumentNullException(nameof(hmiApp));
            InitializeComponent();

            DoorList.ItemsSource = _doors;
            _hmiApp.DoorStateReceived += OnDoorStateReceived;
            Closed += (_, __) => _hmiApp.DoorStateReceived -= OnDoorStateReceived;
        }

        private void OnDoorStateReceived(byte doorId, DoorState state, CanFrame frame)
        {
            Dispatcher.Invoke(() =>
            {
                var existing = FindDoor(doorId);
                if (existing == null)
                {
                    existing = new DoorStatusViewModel { DoorId = doorId };
                    _doors.Add(existing);
                }

                existing.State = state.ToString();
                existing.LastUpdated = DateTime.Now.ToString("HH:mm:ss");
            });
        }

        private DoorStatusViewModel? FindDoor(byte doorId)
        {
            foreach (var door in _doors)
            {
                if (door.DoorId == doorId)
                {
                    return door;
                }
            }

            return null;
        }

        private sealed class DoorStatusViewModel : INotifyPropertyChanged
        {
            private byte _doorId;
            private string _state = "Unknown";
            private string _lastUpdated = "--";

            public event PropertyChangedEventHandler? PropertyChanged;

            public byte DoorId
            {
                get => _doorId;
                set => SetField(ref _doorId, value);
            }

            public string State
            {
                get => _state;
                set => SetField(ref _state, value);
            }

            public string LastUpdated
            {
                get => _lastUpdated;
                set => SetField(ref _lastUpdated, value);
            }

            private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
            {
                if (Equals(field, value))
                {
                    return;
                }

                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
