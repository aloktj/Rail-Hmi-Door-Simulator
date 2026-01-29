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
            bool consoleMode = mode.Equals("console", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase);

            int doorId = 1;
            string idStr = options.GetValue("id");
            if (!string.IsNullOrEmpty(idStr))
            {
                int.TryParse(idStr, out doorId);
                if (doorId <= 0) doorId = 1;
            }

            if (consoleMode)
            {
                ConsoleUi.ConsoleHost.EnsureConsoleAttached();
            }

            Action<string, Exception> busLogger = consoleMode
                ? (message, exception) =>
                {
                    if (exception == null)
                    {
                        Console.WriteLine(message);
                        return;
                    }

                    Console.WriteLine($"{message} {exception.GetType().Name}: {exception.Message}");
                }
                : null;

            _bus = new Common.Transport.Ipc.IpcCanBus(
                "RailCanBus",
                Common.Transport.Ipc.IpcCanBusRole.Client,
                busLogger);
            _bus.Start();
            ICanBus canBus = _bus;

            _doorApp = new DoorApp(canBus, doorId);

            if (consoleMode)
            {
                var r = new ConsoleUi.ConsoleRenderer();

                r.RenderHeader("Door Console");
                r.RenderStatus($"Door ID = {doorId}");
                r.RenderConnectionStatus(_bus.IsConnected);
                r.Log("Console mode active.");

                _bus.ConnectionStateChanged += r.RenderConnectionStatus;

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
