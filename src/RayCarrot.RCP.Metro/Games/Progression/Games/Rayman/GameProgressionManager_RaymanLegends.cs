using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.UbiArt;
using NLog;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanLegends : GameProgressionManager
{
    public GameProgressionManager_RaymanLegends(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends", SearchOption.AllDirectories, "*", "0", 0)
    };

    /// <summary>
    /// Gets the level ID's for each level
    /// </summary>
    protected Dictionary<uint, string> GetLevelIDs => new Dictionary<uint, string>
    {
        { 4068998406, "1-1" },
        { 962968486, "1-2" },
        { 2917370473, "1-3" },
        { 2930639107, "1-4" },
        { 1761301024, "1-5" },
        { 3890960063, "1-6" },
        { 1461226270, "2-1" },
        { 60047882, "2-2" },
        { 1069670671, "2-3" },
        { 4107829653, "2-4" },
        { 1997164622, "2-5" },
        { 2720538894, "3-1" },
        { 1753149557, "3-2" },
        { 3767660219, "3-3" },
        { 1403748227, "3-4" },
        { 3610402831, "3-5" },
        { 3244269653, "4-1" },
        { 2523282745, "4-2" },
        { 193580080, "4-3" },
        { 80857532, "4-4" },
        { 532378801, "4-5" },
        { 304308657, "4-6" },
        { 3703754575, "5-1" },
        { 576210007, "5-2" },
        { 897150152, "5-3" },
        { 2207233233, "5-4" },
        { 941235443, "5-5" },
        { 3600674311, "5-6" }
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends", SearchOption.AllDirectories, "RaymanSave_0"));

        if (saveDir is null)
            yield break;

        using RCPContext context = new(saveDir.DirPath);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanLegends, Platform.PC);
        context.AddSettings(settings);

        foreach (FileSystemPath saveFile in saveDir.GetFiles())
        {
            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.Id, saveFile.Parent.Name);

            string saveFileName = saveFile - saveDir.DirPath;

            // Deserialize the data
            Legends_SaveData? saveFileData = await context.ReadFileDataAsync<Legends_SaveData>(saveFileName, endian: Endian.Big, removeFileWhenComplete: false);

            if (saveFileData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.Id);
                continue;
            }

            Legends_SaveData.RO2_PersistentGameData_Universe saveData = saveFileData.SaveData;

            Logger.Info("Slot has been deserialized");

            // Create the collection with items for each time trial level + general information
            List<GameProgressionDataItem> progressItems = new();

            // Get the total amount of freed teensies
            int teensies = saveData.Levels.Select(x => x.Value.Object.FreedPrisoners.Length).Sum() + saveData.LuckyTicketRewardList.Count(x => x.Type == 5);

            // Add general progress info
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RL_Teensy, 
                header: new ResourceLocString(nameof(Resources.Progression_Teensies)), 
                value: teensies, 
                max: 700));
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false, 
                icon: ProgressionIcon.RL_Lum,
                header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                value: saveData.Score.LocalLumsCount));

            // Add rank
            var rankIcon = (ProgressionIcon)Enum.Parse(typeof(ProgressionIcon), $"RL_Rank{saveData.Profile.StatusIcon}");
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: rankIcon,
                header: new ResourceLocString(nameof(Resources.Progression_Rank)),
                value: (int)saveData.Profile.StatusIcon, 
                max: 11));

            // Add cups
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false, 
                icon: ProgressionIcon.RL_Bronze,
                header: new ResourceLocString(nameof(Resources.Progression_RLBronzeCups)),
                value: (int)saveData.Profile.BronzeMedals));
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false, 
                icon: ProgressionIcon.RL_Silver,
                header: new ResourceLocString(nameof(Resources.Progression_RLSilverCups)),
                value: (int)saveData.Profile.SilverMedals));
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false, 
                icon: ProgressionIcon.RL_Gold,
                header: new ResourceLocString(nameof(Resources.Progression_RLGoldCups)),
                value: (int)saveData.Profile.GoldMedals));
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false, 
                icon: ProgressionIcon.RL_Diamond,
                header: new ResourceLocString(nameof(Resources.Progression_RLDiamondCups)),
                value: (int)saveData.Profile.DiamondMedals));

            // Get the level IDs
            Dictionary<uint, string> lvlIds = GetLevelIDs;

            // Add invasion times
            progressItems.AddRange(saveData.Levels.
                Select(x => x.Value.Object).
                Where(x => x.BestTime > 0).
                Select(x => (lvlIds[x.Id.ID], x.BestTime)).
                OrderBy(x => x.Item1).
                Select(x => new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: Enum.Parse(typeof(ProgressionIcon), $"RL_Inv_{x.Item1.Replace("-", "_")}").CastTo<ProgressionIcon>(),
                    header: new ResourceLocString($"RL_LevelName_{x.Item1.Replace("-", "_")}"),
                    text: $"{TimeSpan.FromMilliseconds(x.BestTime * 1000):mm\\:ss\\.fff}")));

            yield return new SerializableGameProgressionSlot<Legends_SaveData>(saveData.Profile.Name, 0, teensies, 700, progressItems, context, saveFileData, saveFileName)
            {
                //GetExportObject = x => x.SaveData,
                //SetImportObject = (x, o) => x.SaveData = (Legends_SaveData.RO2_PersistentGameData_Universe)o,
                //ExportedType = typeof(Legends_SaveData.RO2_PersistentGameData_Universe)
            };

            Logger.Info("{0} slot has been loaded", GameInstallation.Id);
        }
    }
}