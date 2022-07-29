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

    public override async void Invoke(FileSystemPath filePath)
    {
        try
        {
            // Show the Patcher
            await Services.UI.ShowPatcherAsync(filePath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Runing Patcher from file launch");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred in the patcher and it had to close");
        }
    }
}