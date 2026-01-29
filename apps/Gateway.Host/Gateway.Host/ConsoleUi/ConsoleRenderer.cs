using System;

namespace Gateway.Host.ConsoleUi
{
    internal sealed class ConsoleRenderer
    {
        public void RenderHeader(string title)
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine(title);
            Console.WriteLine("==================================================");
        }

        public void RenderStatus(string statusLine)
        {
            Console.WriteLine("STATUS: " + statusLine);
            Console.WriteLine("--------------------------------------------------");
        }

        public void Log(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
