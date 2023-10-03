namespace RayCarrot.RCP.Metro;

public record BackupLocationChangedMessage
{
    public BackupLocationChangedMessage()
    {
        Logger.Trace("Created a {0}", nameof(BackupLocationChangedMessage));
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
}