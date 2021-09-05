using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Designer progression
    /// </summary>
    public class GameProgression_RaymanDesigner_ViewModel : GameProgression_BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The RayKit game</param>
        public GameProgression_RaymanDesigner_ViewModel(Games game) : base(game)
        {
            // Get the save data directory
            SaveDir = game.GetInstallDir(false);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the progression slot view model for the save data from the specified directory
        /// </summary>
        /// <param name="dirPath">The save data path</param>
        /// <returns>The progression slot view model</returns>
        protected GameProgression_BaseSlotViewModel GetProgressionSlotViewModel(FileSystemPath dirPath)
        {
            RL.Logger?.LogInformationSource($"Rayman Designer saves from {dirPath.Name} is being loaded...");

            // Make sure the directory exists
            if (!dirPath.DirectoryExists)
            {
                RL.Logger?.LogInformationSource($"Saves were not loaded due to not being found");

                return null;
            }

            var shortWorldNames = new string[]
            {
                "",
                "JUN",
                "MUS",
                "MON",
                "IMA",
                "CAV",
                "CAK"
            };
            var longWorldNames = new string[]
            {
                "",
                "Jungle",
                "Music",
                "Mountain",
                "Image",
                "Cave",
                "Cake"
            };

            var progressItems = new List<GameProgression_InfoItemViewModel>();

            // Find every .sct file
            foreach (var save in Directory.GetFiles(dirPath, "*.sct", SearchOption.TopDirectoryOnly).Select(sct =>
            {
                var fileName = ((FileSystemPath)sct).RemoveFileExtension().Name;

                if (fileName.Length != 5)
                    return null;

                var worldStr = fileName.Substring(0, 3);
                var levStr = fileName.Substring(3, 2);

                var world = shortWorldNames.FindItemIndex(x => x == worldStr);
                var lev = Int32.TryParse(levStr, out int parsedLev) ? parsedLev : -1;

                if (world < 1 || lev < 1)
                    return null;

                return new
                {
                    FilePath = (FileSystemPath)sct,
                    World = world,
                    Level = lev
                };
            }).Where(x => x != null).OrderBy(x => x.World).ThenBy(x => x.Level))
            {
                // Open the file in a stream
                using var fileStream = File.Open(save.FilePath, FileMode.Open, FileAccess.Read);

                // Deserialize the data
                var saveData = BinarySerializableHelpers.ReadFromStream<RaymanDesignerSaveData>(fileStream, Ray1Settings.GetDefaultSettings(Ray1Game.RayKit, Platform.PC), RCPServices.App.GetBinarySerializerLogger(save.FilePath.Name));

                // Get the save value
                var value = saveData.GetDecodedValue(save.World, save.Level, Game == Games.RaymanDesigner ? RaymanDesignerSaveData.SaveRevision.KIT : RaymanDesignerSaveData.SaveRevision.FAN_60N);

                if (value == -1)
                {
                    RL.Logger?.LogWarningSource($"Invalid save value for {save.FilePath.Name}");
                    continue;
                }

                // Get the time
                var time = new TimeSpan((long)(value / (3600d / TimeSpan.TicksPerSecond)));

                progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.R1_Flag, new LocalizedString(() => $"{longWorldNames[save.World]} {save.Level}: {time:ss\\:fff}")));
            }

            RL.Logger?.LogInformationSource($"General progress info has been set");

            var levelsCount = Game switch
            {
                Games.RaymanDesigner => 24,
                Games.RaymanByHisFans => 40,
                Games.Rayman60Levels => 60,
                _ => -1 
            };

            // Calculate the percentage
            var percentage = ((progressItems.Count / (double)levelsCount * 100)).ToString("0.##");

            RL.Logger?.LogInformationSource($"Slot percentage is {percentage}%");

            // Return the data with the collection
            return new GameProgression_RaymanDesigner_SlotViewModel(new LocalizedString(() => $"{Resources.Progression_GenericSave} ({percentage}%)"), progressItems.ToArray(), this);
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        protected override void LoadData()
        {
            // Read and set slot data
            ProgressionSlots.Add(GetProgressionSlotViewModel(SaveDir + "PCMAP"));
        }

        #endregion
    }
}