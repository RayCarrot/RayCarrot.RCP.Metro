using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a log viewer
/// </summary>
public class LogViewerViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public LogViewerViewModel()
    {
        // Set properties
        LogItems = new ObservableCollection<LogItemViewModel>();
        ShowLogLevel = LogLevel.Info;

        // Create commands
        ClearLogCommand = new RelayCommand(ClearLog);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Clears the log
    /// </summary>
    public void ClearLog()
    {
        lock (this)
            LogItems.Clear();
    }

    #endregion

    #region Commands

    public ICommand ClearLogCommand { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<LogItemViewModel> LogItems { get; }

    /// <summary>
    /// The log level to show
    /// </summary>
    public LogLevel ShowLogLevel
    {
        get => _showLogLevel;
        set
        {
            if (value == _showLogLevel)
                return;

            _showLogLevel = value;

            foreach (LogItemViewModel item in LogItems)
                item.IsVisible = item.LogLevel >= value;
        }
    }

    #endregion

    #region Private Properties

    private LogLevel _showLogLevel = LogLevel.Trace;

    #endregion
}