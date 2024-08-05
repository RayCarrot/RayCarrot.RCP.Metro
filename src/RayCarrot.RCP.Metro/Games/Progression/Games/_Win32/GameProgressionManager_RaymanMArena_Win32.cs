using System.IO;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanMArena_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanMArena_Win32(GameInstallation gameInstallation, string progressionId, bool isRaymanMDemo) 
        : base(gameInstallation, progressionId)
    {
        IsRaymanMDemo = isRaymanMDemo;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool IsRaymanMDemo { get; }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory + "MENU" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        // Get the save data directory
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "MENU" + "SaveGame", SearchOption.TopDirectoryOnly, "*.sav"));

        if (saveDir == null)
            yield break;

        FileSystemPath saveFileName = "raymanm.sav";

        Logger.Info("{0} save file {1} is being loaded...", GameInstallation.FullId, saveFileName);

        using RCPContext context = new(saveDir.DirPath);
        GameInstallation.GetComponents<InitializeContextComponent>().InvokeAll(context);

        // Deserialize the save data
        RMSaveFile? saveData = await context.ReadFileDataAsync<RMSaveFile>(saveFileName, removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("Save file has been deserialized");

        // There is a max of 8 save slots in every version
        for (int slotIndex = 0; slotIndex < 8; slotIndex++)
        {
            // Get the save name
            string name = saveData.Items.First(x => x.Key == "sg_names").Values[slotIndex].StringValue;

            // Make sure it's valid
            if (name.Contains("No Data"))
                continue;

            IReadOnlyList<GameProgressionDataItem> dataItems = RaymanMArenaProgression.CreateProgressionItems(
                saveData, IsRaymanMDemo, slotIndex, out int collectiblesCount, out int maxCollectiblesCount);

            yield return new SerializableGameProgressionSlot<RMSaveFile>(
                name: name.TrimEnd(), 
                index: slotIndex, 
                collectiblesCount: collectiblesCount, 
                totalCollectiblesCount: maxCollectiblesCount, 
                dataItems: dataItems, 
                context: context, 
                serializable: saveData, 
                fileName: saveFileName);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}