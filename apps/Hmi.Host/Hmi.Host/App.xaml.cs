using System;
using System.Windows;
using Common.Can;

namespace Hmi.Host
{
    public partial class App : Application
    {
        private Common.Transport.Ipc.IpcCanBus? _bus;
        private HmiApp? _hmiApp;
        private bool _disposed;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.IO.File.AppendAllText("args.log", DateTime.Now.ToString("s") + " | " + string.Join(" ", e.Args) + Environment.NewLine);

            var options = CliOptions.Parse(
                e.Args,
                new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "mode", "gui" }
                });

            string mode = options.GetValue("mode");
            bool consoleMode = mode.Equals("console", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase);

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
                Common.Transport.Ipc.IpcCanBusRole.Server,
                busLogger);
            _bus.Start();
            ICanBus canBus = _bus;

            _hmiApp = new HmiApp(canBus);

            if (consoleMode)
            {
                var r = new ConsoleUi.ConsoleRenderer();

                r.RenderHeader("HMI Console");
                r.RenderStatus("Waiting for door status...");
                r.Log("Console mode active.");

                // Temporary test injection (remove later)
                //if (bus is DummyBus dummy)
                //{
                //    dummy.Inject(new Common.Can.CanFrame
                //    {
                //        Id = 0x201,
                //        Dlc = 2,
                //        Data = new byte[] { 1, 1 },
                //        Timestamp = DateTime.UtcNow
                //    });
                //}
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
            _hmiApp?.Dispose();
            _bus?.Dispose();
        }
    }
}
