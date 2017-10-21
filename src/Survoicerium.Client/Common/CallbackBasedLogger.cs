using System;
using Survoicerium.Logging;

namespace Survoicerium.Client.Common
{
    public class CallbackBasedLogger : ILogger
    {
        private readonly Action<string> _callback;

        public CallbackBasedLogger(Action<string> OnNewItemLogged)
        {
            _callback = OnNewItemLogged;
        }

        public void Log(Severity severity, string message)
        {
            string timestampedMessage = $"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")} - [{severity}] {message}";
            _callback(timestampedMessage);
        }
    }
}
