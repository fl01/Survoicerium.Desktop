namespace Survoicerium.Logging
{
    public interface ILogger
    {
        void Log(Severity severity, string message);
    }
}
