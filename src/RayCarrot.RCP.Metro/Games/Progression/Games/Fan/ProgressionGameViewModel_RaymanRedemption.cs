using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanRedemption : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanRedemption() : base(Games.RaymanRedemption) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // NOTE: Not currently localized due to the game not being localized
    protected string[] GetMagicianLevelNames => new string[]
    {
        "Swinging in the Jungle",
        "Vine-Shrine Hunt",
        "The Thorn Garden",
        "Maracas Madness",
        "Electoon's Octave",
        "Ring Toss Tussle",
        "Rock Hike",
        "The Cliffhanger",
        "Perilous Spikes",
        "Pencil Case Race",
        "Little Land",
        "Pinpoint Precision",
        "Getting Over It",
        "The Four Chambers",
        "Electric Meltdown",
        "The Short Trip",
        "The Train Tunnel",
        "Door to Door",
        "Sugar Rush",
        "Candy Climber",
        "The Final Slide",
        "Dark Magician's Challenge",
        "Boss Rush",
        "True Boss Rush",
    };

    private static byte[] StringToByteArray(string hex)
    {
        if (hex.Length % 2 == 1)
            throw new Exception("The hex string cannot have an odd number of characters");

        static int GetHexVal(char hex)
        {
            int val = hex;
            // For uppercase A-F letters:
            // return val - (val < 58 ? 48 : 55);
            // For lowercase a-f letters:
            // return val - (val < 58 ? 48 : 87);
            // Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        byte[] buffer = new byte[hex.Length >> 1];

        for (int i = 0; i < hex.Length >> 1; ++i)
            buffer[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));

        return buffer;
    }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "RaymanRedemption", SearchOption.AllDirectories, "*", "0", 0),
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "RaymanRedemption";

        // Potential other data to show:
        // levelTime{x} = 36 level times, 2 are unused
        // shop_item{x} = 34 shop items, but do minus 3 since they can be bought multiple times
        // joe_item{x} = 2 items, combine with shop items?
        // betillaupgrade - max is 4
        // healthmax - max is 7
        // Achievements (separate from the slots)

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            FileSystemPath filePath = saveDir + $"rayrede{saveIndex + 1}.txt";

            Logger.Info("{0} slot {1} is being loaded...", Game, saveIndex);

            GameMaker_DSMap? saveData = await Task.Run(() =>
            {
                // Get the file
                using Stream? file = fileSystem.ReadFile(filePath);

                if (file == null)
                    return null;

                // Read into a string
                using StreamReader reader = new(file);
                string str = reader.ReadToEnd();

                // Convert the hex string to bytes
                byte[] bytes = StringToByteArray(str);

                // Use a memory stream
                using MemoryStream mem = new(bytes);

                // Deserialize the data
                BinarySerializerSettings settings = new(Endian.Little, Encoding.ASCII);
                return BinarySerializableHelpers.ReadFromStream<GameMaker_DSMap>(mem, settings, Services.App.GetBinarySerializerLogger());
            });

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", Game);
                continue;
            }

            Logger.Info("{0} slot has been deserialized", Game);

            string saveName = saveData.GetValue("savename0").StringValue + 
                              saveData.GetValue("savename1").StringValue + 
                              saveData.GetValue("savename2").StringValue;

            string[] gameModes = { "Casual", "Classic", "Demise" };
            int gameMode = saveData.GetValue("gamemode").NumberValueAsInt;
            string gameModeStr = gameModes[gameMode];

            int getValuesCount(string key, string? invalidKey = null, int value = 1) => saveData.Entries.Count(x => 
                x.Key.StringValue.StartsWith(key) && 
                (invalidKey == null || !x.Key.StringValue.StartsWith(invalidKey)) && 
                x.Value.NumberValueAsInt == value);

            const int maxCages = 168;
            int cages = getValuesCount("cage");

            const int maxTokens = 40;
            int tokens = getValuesCount("token");

            const int maxPresents = 30;
            int presents = getValuesCount("present");

            const int maxRaymanSkins = 24;
            int raymanSkins = getValuesCount("unlock_skin");

            const int maxBzzitSkins = 5;
            int bzzitSkins = getValuesCount("unlock_moskito");

            const int maxCheckpointSkins = 13;
            int checkpointSkins = getValuesCount("unlock_checkpoint");

            const int maxLevelsCompleted = 37;
            int levelsCompleted = getValuesCount("levelcompleted");

            //const int maxMagicianLevelsCompleted = 24;
            int magicianLevelsCompleted = getValuesCount("magician", invalidKey: "magicianTime", value: 2);

            string lives = gameMode == 0 ? "∞" : saveData.GetValue("lives").NumberValueAsInt.ToString();

            int tings = saveData.GetValue("tings").NumberValueAsInt;

            // Get the magician level times
            string[] magicianLevelNames = GetMagicianLevelNames;
            IEnumerable<ProgressionDataViewModel> magicianBonusDataItems = Enumerable.Range(0, 24).Select(i => new
            {
                Index = i,
                TimeValue = saveData.GetValue($"magicianTime{i}").NumberValue,
            }).Where(x => x.TimeValue > 0).Select(x =>
            {
                var icon = (GameProgression_Icon)Enum.Parse(typeof(GameProgression_Icon), $"Redemption_Magician{x.Index}");
                var time = TimeSpan.FromSeconds(x.TimeValue / 60);
                return new ProgressionDataViewModel(false, icon, new ConstLocString($"{time:mm\\:ss\\.fff}"), new ConstLocString(magicianLevelNames[x.Index]));
            });

            int totalProgress = levelsCompleted +
                                tokens +
                                presents +
                                cages +
                                magicianLevelsCompleted +
                                getValuesCount("pink") +
                                saveData.GetValue("gamecompleted").NumberValueAsInt +
                                saveData.GetValue("betillaupgrade").NumberValueAsInt +
                                raymanSkins +
                                bzzitSkins +
                                checkpointSkins +
                                (saveData.GetValue("shop_item3").NumberValueAsInt == 2 ? 1 : 0) +
                                (saveData.GetValue("shop_item4").NumberValueAsInt == 2 ? 1 : 0) +
                                getValuesCount("joe_item");

            double percentage = Math.Floor(totalProgress / (double)352 * 100);

            yield return new ProgressionSlotViewModel(new ConstLocString($"{saveName} ({gameModeStr})"), saveIndex, percentage, new ProgressionDataViewModel[]
            {
                new ProgressionDataViewModel(true, GameProgression_Icon.R1_LevelExit, levelsCompleted, maxLevelsCompleted),
                new ProgressionDataViewModel(true, GameProgression_Icon.R1_Cage, cages, maxCages),
                new ProgressionDataViewModel(true, GameProgression_Icon.Redemption_Token, tokens, maxTokens),
                new ProgressionDataViewModel(true, GameProgression_Icon.Redemption_Present, presents, maxPresents),
                new ProgressionDataViewModel(false, GameProgression_Icon.Redemption_RaymanSkin, raymanSkins, maxRaymanSkins),
                new ProgressionDataViewModel(false, GameProgression_Icon.Redemption_BzzitSkin, bzzitSkins, maxBzzitSkins),
                new ProgressionDataViewModel(false, GameProgression_Icon.Redemption_CheckpointSkin, checkpointSkins, maxCheckpointSkins),
                new ProgressionDataViewModel(false, GameProgression_Icon.R1_Ting, tings),
                new ProgressionDataViewModel(false, GameProgression_Icon.R1_Life, new ConstLocString(lives)),
            }.Concat(magicianBonusDataItems))
            {
                FilePath = filePath
            };

            Logger.Info("{0} slot has been loaded", Game);
        }
    }
}