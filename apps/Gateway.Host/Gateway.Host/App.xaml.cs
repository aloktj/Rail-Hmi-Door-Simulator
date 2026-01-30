using System;
using System.Collections.Generic;
using System.Windows;
using Common.Can;
using Common.Transport.Ipc;

namespace Gateway.Host
{
    public partial class App : Application
    {
        private IpcCanBus? _bus;
        private GatewayApp? _gatewayApp;
        private bool _disposed;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var options = CliOptions.Parse(
                e.Args,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

            _bus = new IpcCanBus("RailCanBus", IpcCanBusRole.Client, busLogger);
            _bus.Start();

            _gatewayApp = new GatewayApp(_bus);

            if (consoleMode)
            {
                var r = new ConsoleUi.ConsoleRenderer();
                r.RenderHeader("Gateway Console");
                r.RenderStatus("Monitoring CAN frames...");
                r.RenderConnectionStatus(_bus.IsConnected);
                r.Log("Console mode active.");
                _bus.ConnectionStateChanged += r.RenderConnectionStatus;
                _gatewayApp.FrameReceived += frame => Console.WriteLine($"[Gateway] {frame}");
            }

            if (mode.Equals("gui", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                var win = new MainWindow(_gatewayApp ?? throw new InvalidOperationException("Gateway app not initialized."));
                win.UpdateConnectionStatus(_bus.IsConnected);
                _bus.ConnectionStateChanged += win.UpdateConnectionStatus;
                win.Show();
            }
            else
            {
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
            _gatewayApp?.Dispose();
            _bus?.Dispose();
        }
    }
}
