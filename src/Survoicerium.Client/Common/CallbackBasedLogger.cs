using System;
using Survoicerium.Logging;

namespace Survoicerium.Client.Common
{
    public class CallbackBasedLogger : ILogger
    {
        private readonly Action<string> _callback;

        public Severity MinLogLevel { get; set; }

        public CallbackBasedLogger(Action<string> OnNewItemLogged)
        {
            _callback = OnNewItemLogged;
        }

        public void Log(Severity severity, string message)
        {
            if (severity < MinLogLevel)
            {
                return;
            }

            string timestampedMessage = $"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")} - [{severity}] {message}";
            _callback(timestampedMessage);
        }
    }
}
