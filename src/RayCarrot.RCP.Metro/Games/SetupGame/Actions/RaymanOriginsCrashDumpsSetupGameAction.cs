using System.IO;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class RaymanOriginsCrashDumpsSetupGameAction : SetupGameAction
{
    private const string FolderName = "dumpCrash";

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_RaymanOriginsCrashDumps_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_RaymanOriginsCrashDumps_Info));

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Remove;
    public override LocalizedString FixActionDisplayName => "Remove crash dumps";

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        try
        {
            FileSystemPath crashDumpFolder = gameInstallation.InstallLocation.Directory + FolderName;
            return crashDumpFolder.DirectoryExists && 
                   Directory.EnumerateFiles(crashDumpFolder, "*.dmp", SearchOption.TopDirectoryOnly).Any();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking for crash dump files");

            // Return false if there was an error since we can't fix it then anyway
            return false;
        }
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        return false;
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        try
        {
            FileSystemPath crashDumpFolder = gameInstallation.InstallLocation.Directory + FolderName;

            // Get the size
            long size = crashDumpFolder.GetSize();

            // Delete the folder
            Services.File.DeleteDirectory(crashDumpFolder);

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(
                String.Format(Resources.SetupGameAction_RaymanOriginsCrashDumps_Success, BinaryHelpers.BytesToString(size)));

            Services.Messenger.Send(new FixedSetupGameActionMessage(gameInstallation));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Removing crash dumps");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.SetupGameAction_RaymanOriginsCrashDumps_Error);
        }
    }
}