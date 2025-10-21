using System.IO;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class RaymanOriginsCrashDumpsSetupGameAction : SetupGameAction
{
    private const string FolderName = "dumpCrash";

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // TODO-LOC
    public override LocalizedString Header => "Remove crash dumps";
    public override LocalizedString Info => "Any time the game crashes it creates a crash dump file. These are around 200 MB each and will thus use up unnecessary space. They can safely be removed to free up space on your computer.";

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

            // TODO-LOC
            await Services.MessageUI.DisplaySuccessfulActionMessageAsync($"{BinaryHelpers.BytesToString(size)} worth of crash dumps were successfully removed");

            Services.Messenger.Send(new FixedSetupGameActionMessage(gameInstallation));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Removing crash dumps");

            // TODO-LOC
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when removing the crash dumps");
        }
    }
}