#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The result for an updater check
/// </summary>
public class UpdaterCheckResult
{
    #region Constructors

    /// <summary>
    /// Constructor for a result where a new update is not available
    /// </summary>
    public UpdaterCheckResult()
    {
        IsNewUpdateAvailable = false;
    }

    /// <summary>
    /// Constructor for a result where the update check failed
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="exception">The exception, if any</param>
    public UpdaterCheckResult(string errorMessage, Exception exception)
    {
        ErrorMessage = errorMessage;
        Exception = exception;
        IsNewUpdateAvailable = false;
    }

    /// <summary>
    /// Constructor for a result where a new update was found
    /// </summary>
    /// <param name="latestVersion">The latest found version</param>
    /// <param name="downloadUrl">The download URL</param>
    /// <param name="displayNews">The news to display for the latest found version</param>
    /// <param name="isBetaUpdate">Indicates if the update is a beta update</param>
    public UpdaterCheckResult(Version latestVersion, string downloadUrl, string displayNews, bool isBetaUpdate)
    {
        LatestVersion = latestVersion;
        DownloadURL = downloadUrl;
        DisplayNews = displayNews;
        IsBetaUpdate = isBetaUpdate;
        IsNewUpdateAvailable = true;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The error message if the check failed
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// The exception, if any
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// The latest found version
    /// </summary>
    public Version LatestVersion { get; }

    /// <summary>
    /// The download URL
    /// </summary>
    public string DownloadURL { get; }

    /// <summary>
    /// The news to display for the latest found version
    /// </summary>
    public string DisplayNews { get; }

    /// <summary>
    /// Indicates if the update is a beta update
    /// </summary>
    public bool IsBetaUpdate { get; }

    /// <summary>
    /// Indicates if a new update was found
    /// </summary>
    public bool IsNewUpdateAvailable { get; }

    #endregion
}