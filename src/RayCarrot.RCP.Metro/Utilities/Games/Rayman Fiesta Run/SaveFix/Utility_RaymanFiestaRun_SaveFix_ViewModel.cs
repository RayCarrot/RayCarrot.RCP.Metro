using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Fiesta Run save fix utility
/// </summary>
public class Utility_RaymanFiestaRun_SaveFix_ViewModel : BaseRCPViewModel
{
    #region Constructor

    // NOTE: This utility is really only needed in the Preload edition as that's the only one on the older version which has this
    //       issue, but it doesn't hurt to have it be available for all editions just in case.
    public Utility_RaymanFiestaRun_SaveFix_ViewModel(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation, int slotIndex)
    {
        FileSystemPath saveDir = gameDescriptor.GetLocalAppDataDirectory();
        SaveFilePath = saveDir + $"slot{slotIndex}.dat";

        RequiresFixing = CheckIfSaveRequiresFix();

        FixCommand = new AsyncRelayCommand(FixAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand FixCommand { get; }

    #endregion

    #region Public Properties

    public FileSystemPath SaveFilePath { get; }
    public bool RequiresFixing { get; set; }

    #endregion

    #region Public Methods

    public bool CheckIfSaveRequiresFix()
    {
        try
        {
            using RCPContext context = new(SaveFilePath.Parent);
            context.AddSettings(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC));

            FiestaRun_SaveData? save = context.ReadFileData<FiestaRun_SaveData>(SaveFilePath.Name);

            if (save == null)
                throw new Exception("Failed to load save data");

            IEnumerable<FiestaRun_SaveDataLevel> levels = save.LevelInfos_Land1;

            if (save.LevelInfos_Land2 != null)
                levels = levels.Concat(save.LevelInfos_Land2);

            return levels.Any(lvl => lvl.HasCrown && lvl.Electoons < 4);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking Fiesta Run save");

            // Return false if there was an error since we can't fix it then anyway
            return false;
        }
    }

    public async Task FixAsync()
    {
        if (!RequiresFixing)
            return;

        try
        {
            using RCPContext context = new(SaveFilePath.Parent);
            context.AddSettings(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC));

            FiestaRun_SaveData? save = context.ReadFileData<FiestaRun_SaveData>(SaveFilePath.Name, removeFileWhenComplete: false);

            if (save == null)
                throw new Exception("Failed to load save data");

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

            RequiresFixing = false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fixing Fiesta Run save");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.RFRU_SaveFixError);
        }
    }

    #endregion
}