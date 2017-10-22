namespace Survoicerium.Logging
{
    public interface ILogger
    {
        Severity MinLogLevel { get; set; }

        void Log(Severity severity, string message);
    }
}
