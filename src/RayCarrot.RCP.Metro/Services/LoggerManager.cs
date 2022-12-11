using System.Diagnostics;
using NLog.Config;
using NLog.Targets;

namespace RayCarrot.RCP.Metro;

public class LoggerManager
{
    public bool IsLogViewerAvailable => LogViewerViewModel != null;
    public LogViewerViewModel? LogViewerViewModel { get; set; }

    public void Initialize(LaunchArguments args)
    {
        // Create a new logging configuration
        LoggingConfiguration logConfig = new();

#if DEBUG
        // On debug we default it to log trace
        LogLevel logLevel = LogLevel.Trace;
#else
        // If not on debug we default to log info
        LogLevel logLevel = LogLevel.Info;
#endif

        // Allow the log level to be specified from a launch argument
        if (args.HasArg("-loglevel", out string? argLogLevel))
            logLevel = LogLevel.FromString(argLogLevel);

        const string logLayout = "${time:invariant=true}|${level:uppercase=true}|${logger}|${message:withexception=true}";
        bool logToFile = !args.HasArg("-nofilelog");
        bool logToMemory = !args.HasArg("-nomemlog");
        bool logToViewer = args.HasArg("-logviewer");

        // Log to file
        if (logToFile)
        {
            logConfig.AddRule(logLevel, LogLevel.Fatal, new FileTarget("file")
            {
                // Archive a maximum of 5 logs. This makes it easier going back to check errors which happened on older instances of the app.
                ArchiveOldFileOnStartup = true,
                ArchiveFileName = AppFilePaths.ArchiveLogFile.FullPath,
                MaxArchiveFiles = 5,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,

                // Keep the file open and disable concurrent writes to improve performance
                // (starting with NLog 5.0 these are the default values, but let's be explicit anyway)
                KeepFileOpen = true,
                ConcurrentWrites = false,

                // Set the file path and layout
                FileName = AppFilePaths.LogFile.FullPath,
                Layout = logLayout,
            });
        }

        if (logToMemory)
        {
            logConfig.AddRule(logLevel, LogLevel.Fatal, new MemoryTarget("memory")
            {
                Layout = logLayout,
            });
        }

        // Log to log viewer
        if (logToViewer)
        {
            LogViewerViewModel = new LogViewerViewModel();

            // Always log from trace to fatal to include all logs
            logConfig.AddRuleForAllLevels(new MethodCallTarget("logviewer", async (logEvent, _) =>
            {
                // Await to avoid blocking
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    LogItemViewModel log = new(logEvent.Level, logEvent.Exception, logEvent.TimeStamp, logEvent.LoggerName, logEvent.FormattedMessage);
                    log.IsVisible = log.LogLevel >= LogViewerViewModel.ShowLogLevel;
                    LogViewerViewModel.LogItems.Add(log);
                });
            }));
        }

        // Apply config
        LogManager.Configuration = logConfig;

        // Listen to data binding logs
        PresentationTraceSources.DataBindingSource.Listeners.Add(new NLogTraceListener());
    }
}