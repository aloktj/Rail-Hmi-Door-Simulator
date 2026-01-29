using System;
using System.Windows;
using Common.Can;

namespace Door.Host
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.IO.File.AppendAllText("args.log", DateTime.Now.ToString("s") + " | " + string.Join(" ", e.Args) + Environment.NewLine);

            string mode = GetArgValue(e.Args, "--mode") ?? "gui";

            int doorId = 1;
            string idStr = GetArgValue(e.Args, "--id");
            if (!string.IsNullOrEmpty(idStr))
            {
                int.TryParse(idStr, out doorId);
                if (doorId <= 0) doorId = 1;
            }

            var bus = new Common.Transport.Ipc.IpcCanBus("RailCanBus", Common.Transport.Ipc.IpcCanBusRole.Client);
            bus.Start();
            ICanBus canBus = bus;

            var doorApp = new DoorApp(canBus, doorId);

            if (mode.Equals("console", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleUi.ConsoleHost.EnsureConsoleAttached();
                var r = new ConsoleUi.ConsoleRenderer();

                r.RenderHeader("Door Console");
                r.RenderStatus($"Door ID = {doorId}");
                r.Log("Console mode active.");

                doorApp.Start();
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

                doorApp.Dispose();
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