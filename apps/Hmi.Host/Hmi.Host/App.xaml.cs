using System;
using System.Windows;
using Common.Can;

namespace Hmi.Host
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.IO.File.AppendAllText("args.log", DateTime.Now.ToString("s") + " | " + string.Join(" ", e.Args) + Environment.NewLine);

            string mode = GetArgValue(e.Args, "--mode") ?? "gui";

            var bus = new Common.Transport.Ipc.IpcCanBus("RailCanBus", Common.Transport.Ipc.IpcCanBusRole.Server);
            bus.Start();
            ICanBus canBus = bus;

            var hmiApp = new HmiApp(canBus);

            if (mode.Equals("console", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleUi.ConsoleHost.EnsureConsoleAttached();
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

                hmiApp.Dispose();
                bus.Dispose();
                Shutdown();
            }

        }

        private static string GetArgValue(string[] args, string key)
        {
            if (args == null) return null;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }
    }
}