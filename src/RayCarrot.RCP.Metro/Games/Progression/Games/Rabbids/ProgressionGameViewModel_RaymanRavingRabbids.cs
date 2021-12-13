using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanRavingRabbids : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanRavingRabbids() : base(Games.RaymanRavingRabbids) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // TODO-UPDATE: Localize and fix formatting
    private static string[] MinigameNames => new[]
    {
        "Bunnies don't like being disturbed on holiday",
        "Bunnies don't like being disturbed at night",
        "Bunnies helped tame the Wild West",
        "Bunnies have a soft spot for plungers",
        "Bunnies never struck gold",
        "Bunnies think they're in a movie",
        "Bunnies love digging tunnels",
        "Bunnies aren't scared of the dark",
        "Bunnies don't rest in peace",
        "Bunnies sometimes recycle the scenery from other games",
        "Bunnies don't like being disturbed on holiday",
        "Bunnies don't like being disturbed at night",
        "Bunnies helped tame the Wild West",
        "Bunnies have a soft spot for plungers",
        "Bunnies never struck gold",
        "Bunnies think they're in a movie",
        "Bunnies love digging tunnels",
        "Bunnies aren't scared of the dark",
        "Bunnies don't rest in peace",
        "Bunnies sometimes recycle the scenery from other games",
        "Bunnies can't jump",
        "Bunnies don't give gifts",
        "Bunnies don't know what to do with cows",
        "Bunnies never close doors",
        "Bunnies don't milk cows",
        "Bunnies are addicted to carrot juice",
        "Bunnies can't slide Part 2",
        "Bunnies are heartless with pigs",
        "Bunnies don't like bats",
        "Bunnies don't use toothpaste",
        "Bunnies can't fly",
        "Bunnies can only fly downwards",
        "Bunnies can't play soccer",
        "Bunnies don't understand bowling",
        "Animal Farm",
        "Bunnies have a great ear for music",
        "Bunnies are slow to react",
        "Bunnies are fantastic dancers Part 3",
        "Bunnies like to stuff themselves",
        "Bunnies are raving mad Part 2",
        "Bunnies like a good race",
        "Race good a like Bunnies",
        "Bunnies like a good race on the beach",
        "Bunnies only fly downwards",
        "Bunnies are fantastic dancers",
        "Bunnies don't sleep well",
        "Bunnies have no memory",
        "Bunnies rarely leave their burrows",
        "Bunnies can't jump Part 2",
        "Bunnies never close doors Part 2",
        "Bunnies don't milk cows Part 2",
        "Bunnies like a good cowboy race",
        "Bunnies are heartless with pigs Part 2",
        "Bunnies Psychology. Volume 1",
        "Bunnies don't like bats Part 2",
        "Bunnies don't use toothpaste Part 2",
        "Bunnies can't fly Part 2",
        "Bunnies Psychology. Volume 2",
        "Bunnies can't shear sheep",
        "Bunnies are slow to react Part 2",
        "Bunnies are slow to react Part 3",
        "Bunnies like to stuff themselves Part 2",
        "Bunnies have no memory Part 2",
        "Bunnies rarely leave their burrows Part 2",
        "Bunnies like surprises",
        "Bunnies don't like being shot at",
        "Bunnies have natural rhythm",
        "Bunnies are a-mazing",
        "The Dark Side",
        "Bunnies have natural rhythm Part 2",
        "Bunnies are ticklish",
        "Bunnies Psychology. Volume 3",
        "Bunnies are bad at peek-a-boo",
        "Bunnies are oversensitive",
        "Bunnies have a poor grasp of anatomy",
        "Bunnies can't slide",
        "Bunnies can't herd",
        "Bunnies are not ostriches",
        "Bunnies just wanna have fun",
        "Bunnies love disco dancing",
        "Bunnies get a kick out of Hip-Hop",
        "Bunninos dansa la Bamba",
        "Deep down, Bunnies are rockers",
        "Bunnies are raving mad",
        "Bunnies are fantastic dancers Part 2",
        "Bunnies love disco dancing Part 2",
        "Bunnies just wanna have fun Part 2",
        "Bunnies get a kick out of Hip-Hop Part 2",
        "Bunninos dansa la Bamba Part 2",
        "Deep down, Bunnies are rockers Part 2",
        "Bunnies don't like being disturbed on holiday",
        "Bunnies don't like being disturbed at night",
        "Bunnies helped tame the Wild West",
        "Bunnies have a soft spot for plungers",
        "Bunnies never struck gold",
        "Bunnies think they're in a movie",
        "Bunnies love digging tunnels",
        "Bunnies aren't scared of the dark",
        "Bunnies don't rest in peace",
        "Bunnies sometimes recycle the scenery from other games",
        "Bunnies don't like being disturbed on holiday",
        "Bunnies don't like being disturbed at night",
        "Bunnies helped tame the Wild West",
        "Bunnies have a soft spot for plungers",
        "Bunnies never struck gold",
        "Bunnies think they're in a movie",
        "Bunnies love digging tunnels",
        "Bunnies aren't scared of the dark",
        "Bunnies don't rest in peace",
        "Bunnies sometimes recycle the scenery from other games",
        "Bunnies don't like being disturbed on holiday",
        "Bunnies don't like being disturbed at night",
        "Bunnies helped tame the Wild West",
        "Bunnies have a soft spot for plungers",
        "Bunnies never struck gold",
        "Bunnies think they're in a movie",
        "Bunnies love digging tunnels",
        "Bunnies aren't scared of the dark",
        "Bunnies don't rest in peace",
        "Bunnies sometimes recycle the scenery from other games",
        "Extreme experiences",
        "The jogging Pants-tathlon",
        "The Pants-tathlon in socks",
        "The Pants-tathlon tracksuit",
        "Pain-tathlon",
        "Bunny Fair",
        "Rayman",
        "World Eclecticism Championship",
        "",
        "",
        "Bunnies don't like being disturbed on holiday",
        "Bunnies don't like being disturbed at night",
        "Bunnies helped tame the Wild West",
        "Bunnies have a soft spot for plungers",
        "Bunnies never struck gold",
        "Bunnies think they're in a movie",
        "Bunnies love digging tunnels",
        "Bunnies aren't scared of the dark",
        "Bunnies don't rest in peace",
        "Bunnies sometimes recycle the scenery from other games",
        "Bunnies are shooting all over the place",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
    };

    private static double[] GetMinigameMaxScores
    {
        get
        {
            double[] MG_score_max = new double[150];

            MG_score_max[0] = 28000.0;
            MG_score_max[1] = 36000.0;
            MG_score_max[2] = 34000.0;
            MG_score_max[3] = 30000.0;
            MG_score_max[4] = 28000.0;
            MG_score_max[5] = 28000.0;
            MG_score_max[6] = 27000.0;
            MG_score_max[7] = 308.0; // Note: This is actually the max score for MG 17. The actual max score for MG 7 is supposed to be 28000.0
            MG_score_max[8] = 33000.0;
            MG_score_max[9] = 38000.0;
            MG_score_max[10] = 210.0;
            MG_score_max[11] = 395.0;
            MG_score_max[12] = 255.0;
            MG_score_max[13] = 150.0;
            MG_score_max[14] = 235.0;
            MG_score_max[15] = 155.0;
            MG_score_max[16] = 285.0;
            MG_score_max[17] = 312.0;  // - (equal to record 0 for MG 17, so: MG_score_max[17] = 312.0)
            MG_score_max[18] = 295.0;
            MG_score_max[19] = 340.0;
            MG_score_max[20] = 20000.0;
            MG_score_max[21] = 22.0;
            MG_score_max[22] = 145.0;
            MG_score_max[23] = 80.0;
            MG_score_max[24] = 7.0;
            MG_score_max[25] = 130.0;
            MG_score_max[26] = 0.1;
            MG_score_max[27] = 10.0;
            MG_score_max[28] = 55.0;
            MG_score_max[29] = 85.0;
            MG_score_max[30] = 170.0;
            MG_score_max[31] = 32.0;
            MG_score_max[32] = 10200.0;
            MG_score_max[33] = 470.0;
            MG_score_max[34] = 3000.0;
            MG_score_max[35] = 3100.0;
            MG_score_max[36] = 20.5;
            MG_score_max[37] = 3178.0;
            MG_score_max[38] = 800.0;
            MG_score_max[39] = 4088.0;
            MG_score_max[40] = 88.0;
            MG_score_max[41] = 90.0;
            MG_score_max[42] = 55.0;
            MG_score_max[43] = 30.0;
            MG_score_max[44] = 1868.0;
            MG_score_max[45] = 40.0;
            MG_score_max[46] = 400.0;
            MG_score_max[47] = 1000.0;
            MG_score_max[48] = 4000.0;
            MG_score_max[49] = 60.0;
            MG_score_max[50] = 10.0;
            MG_score_max[51] = 43.0;
            MG_score_max[52] = 11.0;
            MG_score_max[53] = 3000.0;
            MG_score_max[54] = 30.0;
            MG_score_max[55] = 85.0;
            MG_score_max[56] = 275.0;
            MG_score_max[57] = 3000.0;
            MG_score_max[58] = 1300.0;
            MG_score_max[59] = 21.0;
            MG_score_max[60] = 23.0;
            MG_score_max[61] = 600.0;
            MG_score_max[62] = 400.0;
            MG_score_max[63] = 1000.0;
            MG_score_max[64] = 75000.0;
            MG_score_max[65] = 25000.0;
            MG_score_max[66] = 20.0;
            MG_score_max[67] = 14.0;
            MG_score_max[68] = 3000.0;
            MG_score_max[69] = 21.0;
            MG_score_max[70] = 5600.0;
            MG_score_max[71] = 3000.0;
            MG_score_max[72] = 40.0;
            MG_score_max[73] = 170.0;
            MG_score_max[74] = 800.0;
            MG_score_max[75] = 0.15000001;
            MG_score_max[76] = 7600.0;
            MG_score_max[77] = 30.0;
            MG_score_max[78] = 2088.0;
            MG_score_max[79] = 2668.0;
            MG_score_max[80] = 3198.0;
            MG_score_max[81] = 2358.0;
            MG_score_max[82] = 2478.0;
            MG_score_max[83] = 2658.0;
            MG_score_max[84] = 2528.0;
            MG_score_max[85] = 2908.0;
            MG_score_max[86] = 2288.0;
            MG_score_max[87] = 3538.0;
            MG_score_max[88] = 2568.0;
            MG_score_max[89] = 2928.0;
            MG_score_max[90] = 155.0;
            MG_score_max[91] = 395.0;
            MG_score_max[92] = 257.0;
            MG_score_max[93] = 155.0;
            MG_score_max[94] = 207.0;
            MG_score_max[95] = 157.0;
            MG_score_max[96] = 287.0;
            MG_score_max[97] = 313.0;
            MG_score_max[98] = 297.0;
            MG_score_max[99] = 313.0;
            MG_score_max[100] = 28000.0;
            MG_score_max[101] = 36000.0;
            MG_score_max[102] = 34000.0;
            MG_score_max[103] = 30000.0;
            MG_score_max[104] = 28000.0;
            MG_score_max[105] = 28000.0;
            MG_score_max[106] = 27000.0;
            MG_score_max[107] = 28000.0;
            MG_score_max[108] = 33000.0;
            MG_score_max[109] = 38000.0;
            MG_score_max[110] = 22000.0;
            MG_score_max[111] = 30000.0;
            MG_score_max[112] = 28000.0;
            MG_score_max[113] = 24000.0;
            MG_score_max[114] = 22000.0;
            MG_score_max[115] = 22000.0;
            MG_score_max[116] = 12000.0;
            MG_score_max[117] = 22000.0;
            MG_score_max[118] = 24000.0;
            MG_score_max[119] = 32000.0;
            MG_score_max[120] = 3000.0;
            MG_score_max[121] = 5000.0;
            MG_score_max[122] = 5000.0;
            MG_score_max[123] = 5000.0;
            MG_score_max[124] = 5000.0;
            MG_score_max[125] = 10000.0;
            MG_score_max[126] = 10000.0;
            MG_score_max[127] = 10000.0;

            MG_score_max[140] = 103.0;

            // If PC
            MG_score_max[21] = 24.0;
            MG_score_max[23] = 80.0;
            MG_score_max[24] = 10.0;
            MG_score_max[29] = 85.0;
            MG_score_max[30] = 170.0;
            MG_score_max[47] = 1000.0;
            MG_score_max[49] = 60.0;
            MG_score_max[56] = 275.0;
            MG_score_max[63] = 1000.0;
            MG_score_max[72] = 39.5;
            MG_score_max[73] = 170.0;
            MG_score_max[76] = 7600.0;

            return MG_score_max;
        }
    }

    private static int[] GetMinigameTypes
    {
        get
        {
            int[] MG_objectif_type = new int[150];

            MG_objectif_type[0] = 32786;
            MG_objectif_type[1] = 32786;
            MG_objectif_type[2] = 32786;
            MG_objectif_type[3] = 32786;
            MG_objectif_type[4] = 32786;
            MG_objectif_type[5] = 32786;
            MG_objectif_type[6] = 32786;
            MG_objectif_type[7] = 32786;
            MG_objectif_type[8] = 32786;
            MG_objectif_type[9] = 32786;
            MG_objectif_type[10] = 17;
            MG_objectif_type[11] = 17;
            MG_objectif_type[12] = 17;
            MG_objectif_type[13] = 17;
            MG_objectif_type[14] = 17;
            MG_objectif_type[15] = 17;
            MG_objectif_type[16] = 17;
            MG_objectif_type[17] = 17;
            MG_objectif_type[18] = 17;
            MG_objectif_type[19] = 17;
            MG_objectif_type[20] = 32770;
            MG_objectif_type[21] = 32769;
            MG_objectif_type[22] = 32772;
            MG_objectif_type[23] = 32769;
            MG_objectif_type[24] = 9;
            MG_objectif_type[25] = 32769;
            MG_objectif_type[26] = 12;
            MG_objectif_type[27] = 9;
            MG_objectif_type[28] = 32769;
            MG_objectif_type[29] = 32769;
            MG_objectif_type[30] = 32772;
            MG_objectif_type[31] = 1;
            MG_objectif_type[32] = 32770;
            MG_objectif_type[33] = 32770;
            MG_objectif_type[34] = 33026;
            MG_objectif_type[35] = 32770;
            MG_objectif_type[36] = 1;
            MG_objectif_type[37] = 32906;
            MG_objectif_type[38] = 32770;
            MG_objectif_type[39] = 32906;
            MG_objectif_type[40] = 65;
            MG_objectif_type[41] = 65;
            MG_objectif_type[42] = 65;
            MG_objectif_type[43] = 65;
            MG_objectif_type[44] = 32906;
            MG_objectif_type[45] = 32777;
            MG_objectif_type[46] = 32778;
            MG_objectif_type[47] = 32770;
            MG_objectif_type[48] = 32770;
            MG_objectif_type[49] = 32769;
            MG_objectif_type[50] = 9;
            MG_objectif_type[51] = 65;
            MG_objectif_type[52] = 9;
            MG_objectif_type[53] = 33026;
            MG_objectif_type[54] = 32769;
            MG_objectif_type[55] = 32769;
            MG_objectif_type[56] = 32772;
            MG_objectif_type[57] = 33026;
            MG_objectif_type[58] = 32778;
            MG_objectif_type[59] = 1;
            MG_objectif_type[60] = 1;
            MG_objectif_type[61] = 32770;
            MG_objectif_type[62] = 32778;
            MG_objectif_type[63] = 32770;
            MG_objectif_type[64] = 32770;
            MG_objectif_type[65] = 32778;
            MG_objectif_type[66] = 1;
            MG_objectif_type[67] = 9;
            MG_objectif_type[68] = 33026;
            MG_objectif_type[69] = 1;
            MG_objectif_type[70] = 32770;
            MG_objectif_type[71] = 33026;
            MG_objectif_type[72] = 9;
            MG_objectif_type[73] = 32780;
            MG_objectif_type[74] = 32778;
            MG_objectif_type[75] = 12;
            MG_objectif_type[76] = 32778;
            MG_objectif_type[77] = 1;
            MG_objectif_type[78] = 32906;
            MG_objectif_type[79] = 32906;
            MG_objectif_type[80] = 32906;
            MG_objectif_type[81] = 32906;
            MG_objectif_type[82] = 32906;
            MG_objectif_type[83] = 32906;
            MG_objectif_type[84] = 32906;
            MG_objectif_type[85] = 32906;
            MG_objectif_type[86] = 32906;
            MG_objectif_type[87] = 32906;
            MG_objectif_type[88] = 32906;
            MG_objectif_type[89] = 32906;
            MG_objectif_type[90] = 25;
            MG_objectif_type[91] = 25;
            MG_objectif_type[92] = 25;
            MG_objectif_type[93] = 25;
            MG_objectif_type[94] = 25;
            MG_objectif_type[95] = 25;
            MG_objectif_type[96] = 25;
            MG_objectif_type[97] = 25;
            MG_objectif_type[98] = 25;
            MG_objectif_type[99] = 25;
            MG_objectif_type[100] = 32794;
            MG_objectif_type[101] = 32794;
            MG_objectif_type[102] = 32794;
            MG_objectif_type[103] = 32794;
            MG_objectif_type[104] = 32794;
            MG_objectif_type[105] = 32794;
            MG_objectif_type[106] = 32794;
            MG_objectif_type[107] = 32794;
            MG_objectif_type[108] = 32794;
            MG_objectif_type[109] = 32794;
            MG_objectif_type[110] = 32786;
            MG_objectif_type[111] = 32786;
            MG_objectif_type[112] = 32786;
            MG_objectif_type[113] = 32786;
            MG_objectif_type[114] = 32786;
            MG_objectif_type[115] = 32786;
            MG_objectif_type[116] = 32786;
            MG_objectif_type[117] = 32786;
            MG_objectif_type[118] = 32786;
            MG_objectif_type[119] = 32786;
            MG_objectif_type[120] = 33026;
            MG_objectif_type[121] = 33026;
            MG_objectif_type[122] = 33026;
            MG_objectif_type[123] = 33026;
            MG_objectif_type[124] = 33026;
            MG_objectif_type[125] = 33026;
            MG_objectif_type[126] = 33026;
            MG_objectif_type[127] = 33026;

            MG_objectif_type[130] = 32794;
            MG_objectif_type[131] = 32794;
            MG_objectif_type[132] = 32794;
            MG_objectif_type[133] = 32794;
            MG_objectif_type[134] = 32794;
            MG_objectif_type[135] = 32794;
            MG_objectif_type[136] = 32794;
            MG_objectif_type[137] = 32794;
            MG_objectif_type[138] = 32794;
            MG_objectif_type[139] = 32794;
            MG_objectif_type[140] = 32778;

            return MG_objectif_type;
        }
    }

    private static int ConvertScore(double max, int type, int mgID, float score)
    {
        // Check if the score should be used as is
        if ((type & 0x100) != 0)
            return (int)score;

        double scaledScore;

        // Bunnies like surprises
        if (mgID == 64)
        {
            double v3 = score - 50000.0;

            if (v3 <= 0)
                v3 = 0;

            scaledScore = v3 / (max - 50000.0);

            if (scaledScore > 1)
                return 1 * 1000;

            if (scaledScore < 0)
                return 0 * 1000;
        }
        else
        {
            if (score < 0.001)
                return 0 * 1000;

            double v5;

            if ((BitHelpers.ExtractBits(type, 8, 8) & 0x80u) == 0)
                v5 = max / score;
            else
                v5 = score / max;

            scaledScore = v5 switch
            {
                > 1 => 1,
                < 0 => 0,
                _ => v5
            };
        }
        return (int)(scaledScore * 1000);
    }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
        new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "VirtualStore" + Game.GetInstallDir().RemoveRoot(), SearchOption.TopDirectoryOnly, "*.sav", "0", 0)
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        // TODO-UPDATE: Check both dir and virtual store - find better solution for dealing with VirtualStore redirection
        FileSystemPath saveFile = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "VirtualStore" + Game.GetInstallDir().RemoveRoot() + "Rayman4.sav";

        Logger.Info("{0} save is being loaded...", Game);

        BinarySerializerSettings settings = new(Endian.Little, Encoding.UTF8);
        RRR_SaveFile? saveData = await SerializeFileDataAsync<RRR_SaveFile>(fileSystem, saveFile, settings, new RRR_SaveEncoder());

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", Game);
            yield break;
        }

        Logger.Info("Save has been deserialized");

        // Add save slots
        for (int saveIndex = 0; saveIndex < saveData.StorySlots.Length; saveIndex++)
        {
            RRR_SaveSlot slot = saveData.StorySlots[saveIndex];

            if (slot.SlotDesc.Time == 0)
                continue;

            yield return new ProgressionSlotViewModel(new ConstLocString(slot.SlotDesc.Name), saveIndex, slot.SlotDesc.Progress_Percentage, new ProgressionDataViewModel[]
            {
                new ProgressionDataViewModel(true, GameProgression_Icon.RRR_Plunger, slot.SlotDesc.Progress_Days, 15),
            });
        }

        string[] names = MinigameNames;
        double[] maxScores = GetMinigameMaxScores;
        int[] types = GetMinigameTypes;

        List<ProgressionDataViewModel> scoreDataItems = new();

        int totalScore = 0;
        const int maxScore = 183000;

        // Enumerate every minigame
        for (int mgID = 0; mgID < 128; mgID++)
        {
            // Find player top score
            int[] playerScores = Enumerable.Range(0, 3).Where(i => saveData.ScoreSlot.Univers.MG_record_pn[mgID * 3 * 3 + i * 3] >= 0).ToArray();

            bool isAscending = (types[mgID] & 0x8000) != 0;
            float defaultScore = isAscending ? 0 : 100000000000000;

            // Skip if there are no high scores in this minigame
            if (playerScores.Length == 0)
            {
                totalScore += ConvertScore(maxScores[mgID], types[mgID], mgID, defaultScore);
                continue;
            }

            int highScoreIndex = isAscending
                ? playerScores.OrderBy(i => saveData.ScoreSlot.Univers.MG_record[mgID * 3 + i]).Last()
                : playerScores.OrderByDescending(i => saveData.ScoreSlot.Univers.MG_record[mgID * 3 + i]).Last();

            float playerHighScore = saveData.ScoreSlot.Univers.MG_record[mgID * 3 + highScoreIndex];

            int score = ConvertScore(maxScores[mgID], types[mgID], mgID, isAscending
                ? Math.Max(defaultScore, playerHighScore)
                : Math.Min(defaultScore, playerHighScore));

            totalScore += score;

            // Maybe correct this?
            static string getChar(int value)
            {
                value = Math.Abs(value);

                value = BitHelpers.ExtractBits(value, 12, 0);

                return ((char)value).ToString();
            }

            string char0 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[mgID * 3 * 3 + highScoreIndex * 3 + 0]);
            string char1 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[mgID * 3 * 3 + highScoreIndex * 3 + 1]);
            string char2 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[mgID * 3 * 3 + highScoreIndex * 3 + 2]);

            string name = char0 + char1 + char2;

            if (score != 0 && (types[mgID] & 0x100) == 0)
                scoreDataItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RRR_Star, score, 1000, new ConstLocString(names[mgID])));

            scoreDataItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RRR_Trophy, new ConstLocString($"{name}: {playerHighScore}"), new ConstLocString(names[mgID])));
        }

        // Add total score
        scoreDataItems.Insert(0, new ProgressionDataViewModel(true, GameProgression_Icon.RRR_Star, totalScore, maxScore));

        // Add score slot
        // TODO-UPDATE: Localize
        yield return new ProgressionSlotViewModel(new ConstLocString("Score"), 3, totalScore, maxScore, scoreDataItems);

        Logger.Info("{0} save has been loaded", Game);
    }
}