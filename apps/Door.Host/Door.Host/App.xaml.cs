using System;
using System.Windows;
using Common.Can;

namespace Door.Host
{
    public partial class App : Application
    {
        private Common.Transport.Ipc.IpcCanBus? _bus;
        private DoorApp? _doorApp;
        private bool _disposed;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.IO.File.AppendAllText("args.log", DateTime.Now.ToString("s") + " | " + string.Join(" ", e.Args) + Environment.NewLine);

            var options = CliOptions.Parse(
                e.Args,
                new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "mode", "gui" },
                    { "id", "1" }
                });

            string mode = options.GetValue("mode");

            int doorId = 1;
            string idStr = options.GetValue("id");
            if (!string.IsNullOrEmpty(idStr))
            {
                int.TryParse(idStr, out doorId);
                if (doorId <= 0) doorId = 1;
            }

            _bus = new Common.Transport.Ipc.IpcCanBus("RailCanBus", Common.Transport.Ipc.IpcCanBusRole.Client);
            _bus.Start();
            ICanBus canBus = _bus;

            _doorApp = new DoorApp(canBus, doorId);

            if (mode.Equals("console", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleUi.ConsoleHost.EnsureConsoleAttached();
                var r = new ConsoleUi.ConsoleRenderer();

                r.RenderHeader("Door Console");
                r.RenderStatus($"Door ID = {doorId}");
                r.Log("Console mode active.");

                _doorApp.Start();
            }

            if (mode.Equals("gui", StringComparison.OrdinalIgnoreCase))
            {
                var win = new MainWindow();
                win.Show();
            }
            else if (mode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                var win = new MainWindow();
                win.Show();
            }
            else
            {
                // console-only
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();

                DisposeResources();
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DisposeResources();
            base.OnExit(e);
        }

        private void DisposeResources()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _doorApp?.Dispose();
            _bus?.Dispose();
        }
    }
}
