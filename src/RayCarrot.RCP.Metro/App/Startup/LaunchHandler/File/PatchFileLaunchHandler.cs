namespace RayCarrot.RCP.Metro;

public class PatchFileLaunchHandler : FileLaunchHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool DisableFullStartup => true;

    public override bool IsValid(FileSystemPath filePath)
    {
        // TODO-UPDATE: Check if the file has a valid archive extension for a patch. Allow .gp as well for legacy support.

        return false;
    }

    public override async void Invoke(FileSystemPath filePath, State state)
    {
        try
        {
            // Show the Patcher
            await Services.UI.ShowPatcherAsync(new[] { filePath });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Runing Patcher from file launch");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_CriticalError);
        }
    }
}