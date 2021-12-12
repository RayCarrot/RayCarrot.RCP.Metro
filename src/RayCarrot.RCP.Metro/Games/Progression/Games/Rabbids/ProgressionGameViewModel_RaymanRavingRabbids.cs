using System;
using System.Collections.Generic;
using System.IO;
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

        List<ProgressionDataViewModel> scoreDataItems = new();

        // Enumerate every minigame
        for (int minigameIndex = 0; minigameIndex < 150; minigameIndex++)
        {
            // Enumerate every score
            for (int scoreIndex = 0; scoreIndex < 3; scoreIndex++)
            {
                // Check if it's a non-default score
                int firstCharValue = saveData.ScoreSlot.Univers.MG_record_pn[minigameIndex * 3 * 3 + scoreIndex * 3 + 0];
                bool isNonDefault = firstCharValue > 0 && BitHelpers.ExtractBits(firstCharValue, 1, 12) == 1;

                if (!isNonDefault)
                    continue;

                static string getChar(int value)
                {
                    value = Math.Abs(value);

                    value = BitHelpers.ExtractBits(value, 12, 0);

                    return ((char)value).ToString();
                }

                string char0 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[minigameIndex * 3 * 3 + scoreIndex * 3 + 0]);
                string char1 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[minigameIndex * 3 * 3 + scoreIndex * 3 + 1]);
                string char2 = getChar(saveData.ScoreSlot.Univers.MG_record_pn[minigameIndex * 3 * 3 + scoreIndex * 3 + 2]);

                string name = char0 + char1 + char2;

                float score = saveData.ScoreSlot.Univers.MG_record[minigameIndex * 3 + scoreIndex];

                scoreDataItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RRR_Trophy, new ConstLocString($"{name}: {score}"), new ConstLocString(names[minigameIndex])));

                break;
            }
        }

        // Add score slot
        // TODO-UPDATE: Localize
        // TODO-UPDATE: Show minigame scores (x/1000)
        // TODO-UPDATE: Calculate percentage
        yield return new ProgressionSlotViewModel(new ConstLocString("Score"), 3, 0, 0, scoreDataItems);

        Logger.Info("{0} save has been loaded", Game);
    }
}