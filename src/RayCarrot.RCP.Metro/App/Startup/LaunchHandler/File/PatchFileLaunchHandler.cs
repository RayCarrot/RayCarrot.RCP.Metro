using System;
using NLog;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class PatchFileLaunchHandler : FileLaunchHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool DisableFullStartup => true;

    public override bool IsValid(FileSystemPath filePath)
    {
        return filePath.FileExtension.PrimaryFileExtension.Equals(PatchFile.FileExtension,
            StringComparison.InvariantCultureIgnoreCase);
    }

    public override async void Invoke(FileSystemPath filePath, State state)
    {
        try
        {
            // Show the Patcher
            await Services.UI.ShowPatcherAsync(filePath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Runing Patcher from file launch");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_CriticalError);
        }
    }
}