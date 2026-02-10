using System.Diagnostics.CodeAnalysis;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The result for an updater check
/// </summary>
public class UpdaterCheckResult
{
    #region Constructors

    /// <summary>
    /// Constructor for a result where no new version is available
    /// </summary>
    public UpdaterCheckResult()
    {
        HasError = false;
        NewVersionAvailable = false;
    }

    /// <summary>
    /// Constructor for a result where the update check failed
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="errorException">The exception, if any</param>
    public UpdaterCheckResult(string errorMessage, Exception? errorException)
    {
        HasError = true;
        NewVersionAvailable = false;

        ErrorMessage = errorMessage;
        ErrorException = errorException;
    }

    /// <summary>
    /// Constructor for a result where a new version was found
    /// </summary>
    /// <param name="newVersion">The latest found version</param>
    /// <param name="newVersionUrl">The download URL</param>
    /// <param name="newVersionChangelog">The changelog for the latest found version</param>
    /// <param name="isNewVersionBeta">Indicates if the new version is a beta version</param>
    public UpdaterCheckResult(Version newVersion, string newVersionUrl, string newVersionChangelog, bool isNewVersionBeta)
    {
        HasError = false;
        NewVersionAvailable = true;

        NewVersion = newVersion;
        NewVersionUrl = newVersionUrl;
        NewVersionChangelog = newVersionChangelog;
        IsNewVersionBeta = isNewVersionBeta;
    }

    #endregion

    #region Public Properties

    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool HasError { get; }

    [MemberNotNullWhen(true, nameof(NewVersion), nameof(NewVersionUrl), nameof(NewVersionChangelog))]
    public bool NewVersionAvailable { get; }

    public string? ErrorMessage { get; }
    public Exception? ErrorException { get; }

    public Version? NewVersion { get; }
    public string? NewVersionUrl { get; }
    public string? NewVersionChangelog { get; }
    public bool IsNewVersionBeta { get; }

    #endregion
}