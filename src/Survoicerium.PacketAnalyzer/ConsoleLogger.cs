using System;
using Survoicerium.Logging;

namespace Survoicerium.PacketAnalyzer
{
    public class ConsoleLogger : ILogger
    {
        public void Log(Severity severity, string message)
        {
            Console.WriteLine($"{severity} - {message}");
        }
    }
}
