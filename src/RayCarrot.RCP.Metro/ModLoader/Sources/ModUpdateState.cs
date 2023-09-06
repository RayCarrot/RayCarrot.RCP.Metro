namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public enum ModUpdateState
{
    None,
    UpdateAvailable,
    UpToDate,
    CheckingForUpdates,
    UnableToCheckForUpdates,
    ErrorCheckingForUpdates,
}