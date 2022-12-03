using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.UbiArt;
using Newtonsoft.Json;
using NLog;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanOrigins : GameProgressionManager
{
    public GameProgressionManager_RaymanOrigins(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins", SearchOption.AllDirectories, "*", "0", 0)
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        // Get the save data directory
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins" + "Savegame", SearchOption.TopDirectoryOnly, "*"));

        if (saveDir is null)
            yield break;

        // Get the level configuration
        ROLevelConfig? lvlConfig = JsonConvert.DeserializeObject<ROLevelConfig>(Files.RO_LevelConfig);

        if (lvlConfig == null)
        {
            Logger.Error("Rayman Origins level config is null");
            yield break;
        }

        using RCPContext context = new(saveDir.DirPath);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanOrigins, Platform.PC);
        context.AddSettings(settings);

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            string fileName = $"Savegame_{saveIndex}";

            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, saveIndex);

            // Deserialize the data
            Origins_SaveData? saveFileData = await context.ReadFileDataAsync<Origins_SaveData>(fileName, endian: Endian.Big, removeFileWhenComplete: false);

            if (saveFileData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            Origins_SaveData.Ray_PersistentGameData_Universe saveData = saveFileData.SaveData;

            Logger.Info("Slot has been deserialized");

            int completed = 0;
            int cageMaps = 0;
            int lumAttack1 = 0;
            int lumAttack2 = 0;
            int lumAttack3 = 0;
            int timeAttack1 = 0;
            int timeAttack2 = 0;

            // Get number of levels where each mission has been completed
            foreach (var lvl in saveData.Levels)
            {
                // Get the configuration for the level
                ROLevelConfig.Level? level = lvlConfig.GetLevel(lvl.Key.ID);
                ROLevelConfig.Mission? mission = lvlConfig.GetMission(lvl.Key.ID);

                // Make sure it's a normal level, i.e. it has a medallion
                if (level == null || mission == null)
                    continue;

                // Check if the level has been completed
                if (lvl.Value.Object.LevelState == Origins_SaveData.SPOT_STATE.COMPLETED)
                    completed++;

                // Get the number of completed cage maps (between 0-2)
                cageMaps += lvl.Value.Object.CageMapPassedDoors.Length;

                // Get the best time attack score
                uint timeAttack = lvl.Value.Object.BestTimeAttack;

                // Compare the time attack score with the targets
                if (timeAttack <= level.Time1)
                    timeAttack1++;
                if (timeAttack <= level.Time2)
                    timeAttack2++;

                // Get the best lum attack score
                uint lumAttack = lvl.Value.Object.BestLumAttack;

                // Compare the lum attack score with the targets
                if (lumAttack >= mission.LumAttack1)
                    lumAttack1++;
                if (lumAttack >= mission.LumAttack2)
                    lumAttack2++;
                if (lumAttack >= mission.LumAttack3)
                    lumAttack3++;
            }

            // Create the collection with items for each time trial level + general information
            List<GameProgressionDataItem> progressItems = new();

            // Get the number of Electoons
            int electoons =
                // Cages
                cageMaps +
                // Levels completed
                completed +
                // Lum attack 1
                lumAttack1 +
                // Lum attack 2
                lumAttack2 +
                // Time attack 1
                timeAttack1;

            int teeth = saveData.Levels.Select(x => x.Value.Object.ISDs.Select(y => y.Value.Object.TakenTooth.Length)).SelectMany(x => x).Sum();

            // Add general progress info
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RO_Electoon,
                header: new ResourceLocString(nameof(Resources.Progression_Electoons)),
                value: electoons, 
                max: 246));
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RO_RedTooth, 
                header: new ResourceLocString(nameof(Resources.Progression_Teeth)),
                value: teeth, 
                max: 10));
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RO_Medal, 
                header: new ResourceLocString(nameof(Resources.Progression_ROLumMedals)),
                value: lumAttack3, 
                max: 51));
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RO_Trophy,
                header: new ResourceLocString(nameof(Resources.Progression_ROSpeedTrophies)),
                value: timeAttack2, 
                max: 31));

            yield return new SerializableGameProgressionSlot<Origins_SaveData>(
                name: null, 
                index: saveIndex, 
                collectiblesCount: electoons + teeth + lumAttack3 + timeAttack2, 
                totalCollectiblesCount: 246 + 10 + 51 + 31, 
                dataItems: progressItems, 
                context: context, 
                serializable: saveFileData, 
                fileName: fileName,
                canImport: false)
            {
                // TODO: Allow importing. Current issue is the game fails to load modified saves - checksum in header?
                //GetExportObject = x => x.SaveData,
                //SetImportObject = (x, o) => x.SaveData = (Origins_SaveData.Ray_PersistentGameData_Universe)o,
                //ExportedType = typeof(Origins_SaveData.Ray_PersistentGameData_Universe),
            };

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }

    /// <summary>
    /// Data for Rayman Origins level configuration
    /// </summary>
    protected class ROLevelConfig
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="levels">The levels</param>
        /// <param name="missions">The mission types</param>
        public ROLevelConfig(Level[] levels, Mission[] missions)
        {
            Levels = levels;
            Missions = missions;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The levels
        /// </summary>
        public Level[] Levels { get; }

        /// <summary>
        /// The mission types
        /// </summary>
        public Mission[] Missions { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the level which matches the tag
        /// </summary>
        /// <param name="tag">The level tag</param>
        /// <returns>The level, or null if not found</returns>
        public Level? GetLevel(uint tag) => Levels.FirstOrDefault(x => x.Tag == tag);

        /// <summary>
        /// Gets the mission which matches the level tag
        /// </summary>
        /// <param name="tag">The level tag</param>
        /// <returns>The mission, or null if not found</returns>
        public Mission? GetMission(uint tag) => Missions.FirstOrDefault(x => x.Type == GetLevel(tag)?.Type);

        #endregion

        public class Level
        {
            public Level(int time1, int time2, uint tag, uint type)
            {
                Time1 = time1;
                Time2 = time2;
                Tag = tag;
                Type = type;
            }

            public int Time1 { get; }

            public int Time2 { get; }

            public uint Tag { get; }

            public uint Type { get; }
        }

        public class Mission
        {
            public Mission(uint type, int lumAttack1, int lumAttack2, int lumAttack3)
            {
                Type = type;
                LumAttack1 = lumAttack1;
                LumAttack2 = lumAttack2;
                LumAttack3 = lumAttack3;
            }

            public uint Type { get; }

            public int LumAttack1 { get; }

            public int LumAttack2 { get; }

            public int LumAttack3 { get; }
        }
    }
}