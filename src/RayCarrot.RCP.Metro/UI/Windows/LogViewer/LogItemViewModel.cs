namespace RayCarrot.RCP.Metro;

public class LogItemViewModel : BaseRCPViewModel
{
    public LogItemViewModel(LogLevel logLevel, Exception exception, DateTime logTime, string loggerName, string logMessage)
    {
        LogLevel = logLevel;
        Exception = exception;
        LogTime = logTime;
        LoggerName = loggerName;
        LogMessage = logMessage;
    }

    public LogLevel LogLevel { get; }
    public Exception Exception { get; }
    public DateTime LogTime { get; }
    public string LoggerName { get; }
    public string LogMessage { get; }

    public bool IsVisible { get; set; }
}