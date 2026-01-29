using Common.Can;
using Gateway.Host;
using System;
using System.Windows;

namespace Gateway.Host   // <-- MUST match App.xaml x:Class namespace (Hmi.Host.App)
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var options = CliOptions.Parse(
                e.Args,
                new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "mode", "gui" }
                });

            string mode = options.GetValue("mode");

            if (mode.Equals("console", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                // If you haven't created ConsoleUi classes yet, comment next line for now
                Gateway.Host.ConsoleUi.ConsoleHost.EnsureConsoleAttached();
                var r = new Gateway.Host.ConsoleUi.ConsoleRenderer();

                r.RenderHeader("Gateway Console");
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

    }
}
