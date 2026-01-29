using Hmi.Host;
using System;
using System.Windows;

namespace Hmi.Host   // <-- MUST match App.xaml x:Class namespace (Hmi.Host.App)
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string mode = GetArgValue(e.Args, "--mode") ?? "gui";

            if (mode.Equals("console", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                // If you haven't created ConsoleUi classes yet, comment next line for now
                Hmi.Host.ConsoleUi.ConsoleHost.EnsureConsoleAttached();
                var r = new Hmi.Host.ConsoleUi.ConsoleRenderer();

                r.RenderHeader("HMI Console");
                r.RenderStatus("Placeholder - no CAN yet");
                r.Log("Console mode active.");
            }

            if (mode.Equals("gui", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                var win = new MainWindow();
                win.Show();
            }
            else
            {
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
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