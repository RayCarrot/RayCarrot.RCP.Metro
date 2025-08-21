using System.IO;
using BinarySerializer.OpenSpace;
using Microsoft.WindowsAPICodePack.Shell;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_TonicTrouble_Win32 : GameProgressionManager
{
    public GameProgressionManager_TonicTrouble_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // Default location
        new(GameInstallation.InstallLocation.Directory + "gamedata" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
        new(GameInstallation.InstallLocation.Directory + "gamedata" + "Options", SearchOption.AllDirectories, "*", "1", 0),
        
        // Redirected location using Tonic Trouble Fix
        new(new FileSystemPath(KnownFolders.SavedGames.Path) + "Tonic Trouble" + "SaveGame", SearchOption.AllDirectories, "*", "2", 0),
        new(new FileSystemPath(KnownFolders.SavedGames.Path) + "Tonic Trouble" + "Options", SearchOption.AllDirectories, "*", "3", 0),
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        // Get the save game directories
        IOSearchPattern?[] saveGameDirectories =
        [
            fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "gamedata" + "SaveGame", SearchOption.AllDirectories, "Level000.sav")),
            fileSystem.GetDirectory(new IOSearchPattern(new FileSystemPath(KnownFolders.SavedGames.Path) + "Tonic Trouble" + "SaveGame", SearchOption.AllDirectories, "Level000.sav")),
        ];

        foreach (IOSearchPattern? saveDir in saveGameDirectories)
        {
            if (saveDir == null)
                continue;

            // Create the context
            using RCPContext context = new(saveDir.DirPath);

            foreach (FileSystemPath saveFile in saveDir.GetFiles())
            {
                Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, saveFile.Parent.Name);

                string saveFileName = saveFile - saveDir.DirPath;

                // Deserialize the data
                TTSaveFile? saveData = await context.ReadFileDataAsync<TTSaveFile>(saveFileName, new R2SaveEncoder(), removeFileWhenComplete: false);

                if (saveData == null)
                {
                    Logger.Info("{0} slot was not found", GameInstallation.FullId);
                    continue;
                }

                Logger.Info("Slot has been deserialized");

                // Find the inventory data. We can't identify it by the pointer as it might be different
                // between game releases. So instead we check the array length as it's always the same.
                byte[]? inventoryData = saveData.GameTable.FirstOrDefault(x => x.ArrayValueLength == 62)?.ArrayValue;

                if (inventoryData == null)
                {
                    Logger.Info("{0} slot has no inventory data", GameInstallation.FullId);
                    continue;
                }

                // Convert into 16-bit values
                short[] inventoryData16 = new short[inventoryData.Length / 2];
                for (int i = 0; i < inventoryData16.Length; i++)
                    inventoryData16[i] = BitConverter.ToInt16(inventoryData, i * 2);

                const int levelsCount = 10;
                int getValue(int index) => inventoryData16[index] != -1 ? inventoryData16[index] : 0;

                // Calculate primary collectibles
                int thermometers = 0;
                const int maxThermometers = 100;
                for (int i = 0; i < levelsCount; i++)
                    thermometers += getValue(10 + i * 2);
                int bonusPoints = 0;
                const int maxBonusPoints = 180;
                for (int i = 0; i < levelsCount; i++)
                    bonusPoints += getValue(9 + i * 2);

                // TODO-LOC
                GameProgressionDataItem[] progressItems =
                [
                    // Primary collectibles
                    new GameProgressionDataItem(
                        isPrimaryItem: true,
                        icon: ProgressionIconAsset.TT_Thermometer,
                        header: "Thermometers",
                        value: thermometers,
                        max: maxThermometers),
                    new GameProgressionDataItem(
                        isPrimaryItem: true,
                        icon: ProgressionIconAsset.TT_Bonus,
                        header: "Bonus points",
                        value: bonusPoints,
                        max: maxBonusPoints),

                    // Doc collectibles
                    new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.TT_Springs,
                        header: "Springs",
                        value: getValue(3 + 5),
                        max: 6),
                    new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.TT_PropellerBlades,
                        header: "Propeller blades",
                        value: getValue(3 + 1),
                        max: 6),
                    new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.TT_JumpingStones,
                        header: "Jumping stones",
                        value: getValue(3 + 2),
                        max: 6),
                    new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.TT_Feathers,
                        header: "Feathers",
                        value: getValue(3 + 4),
                        max: 6),
                    new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.TT_Dominoes,
                        header: "Dominoes",
                        value: getValue(3 + 0),
                        max: 6),
                    new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.TT_WildPiggybanks,
                        header: "Wild piggybanks",
                        value: getValue(3 + 3),
                        max: 6),
                ];

                yield return new SerializableGameProgressionSlot<TTSaveFile>(
                    saveFile.Parent.Name, 
                    0, 
                    thermometers + bonusPoints, 
                    maxThermometers + maxBonusPoints, 
                    progressItems, 
                    context, 
                    saveData, 
                    saveFileName);

                Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
            }
        }
    }
}