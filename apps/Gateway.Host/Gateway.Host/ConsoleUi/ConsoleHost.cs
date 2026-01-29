using System;
using System.Runtime.InteropServices;

namespace Gateway.Host.ConsoleUi
{
    internal static class ConsoleHost
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        private const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

        public static void EnsureConsoleAttached()
        {
            // If launched from an existing console, attach to it; otherwise allocate a new console.
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                AllocConsole();
            }

            Console.Title = "Gateway Console";
        }

        public static void DetachConsole()
        {
            FreeConsole();
        }
    }
}
