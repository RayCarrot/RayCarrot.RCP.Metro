using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

// NOTE: This is really only needed in the Preload edition as that's the only one on the older version which has this
//       issue, but it doesn't hurt to have it be available for all editions just in case.
public class CorruptRaymanFiestaRunSaveFileSetupGameAction : SetupGameAction
{
    public CorruptRaymanFiestaRunSaveFileSetupGameAction(WindowsPackageGameDescriptor gameDescriptor, int slotIndex)
    {
        FileSystemPath saveDir = gameDescriptor.GetLocalAppDataDirectory();
        SaveFilePath = saveDir + $"slot{slotIndex}.dat";
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public FileSystemPath SaveFilePath { get; }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_CorruptRaymanFiestaRunSaveFile_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_CorruptRaymanFiestaRunSaveFile_Info));

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Fix;
    public override LocalizedString FixActionDisplayName => new ResourceLocString(nameof(Resources.SetupGameAction_GeneralFix));

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        try
        {
            using RCPContext context = new(SaveFilePath.Parent);
            context.AddSettings(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC));

            FiestaRun_SaveData save = context.ReadRequiredFileData<FiestaRun_SaveData>(SaveFilePath.Name);

            IEnumerable<FiestaRun_SaveDataLevel> levels = save.LevelInfos_Land1;

            if (save.LevelInfos_Land2 != null)
                levels = levels.Concat(save.LevelInfos_Land2);

            return levels.Any(lvl => lvl.HasCrown && lvl.Electoons < 4);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if Rayman Fiesta Run save is corrupt");

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
            using RCPContext context = new(SaveFilePath.Parent);
            context.AddSettings(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC));

            FiestaRun_SaveData save = context.ReadRequiredFileData<FiestaRun_SaveData>(SaveFilePath.Name, removeFileWhenComplete: false);

            IEnumerable<FiestaRun_SaveDataLevel> levels = save.LevelInfos_Land1;

            if (save.LevelInfos_Land2 != null)
                levels = levels.Concat(save.LevelInfos_Land2);

            foreach (FiestaRun_SaveDataLevel lvl in levels)
            {
                if (lvl.HasCrown && lvl.Electoons < 4)
                    lvl.Electoons = 4;
            }

            FileFactory.Write(context, SaveFilePath.Name, save);

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.RFRU_SaveFixSuccess);

            Services.Messenger.Send(new FixedSetupGameActionMessage(gameInstallation));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fixing Fiesta Run save");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.RFRU_SaveFixError);
        }
    }
}