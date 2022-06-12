using System;
using System.Collections.Generic;
using BinarySerializer;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class Mod_Mem_CPAGame : Mod_Mem_Game<Mod_Mem_CPAMemoryData>
{
    #region Constructor

    public Mod_Mem_CPAGame(OpenSpaceSettings settings)
    {
        Settings = settings;
    }

    #endregion

    #region Private Properties

    // TODO-UPDATE: Localize
    private GroupedEditorDropDownFieldViewModel.DropDownItem<string>[] Maps_R2_PC => new GroupedEditorDropDownFieldViewModel.DropDownItem<string>[] 
    {
        new("Menu", null, true),
        new("Main Menu", "menu"),
        new("The Hall of Doors", "mapmonde"),
        new("Score Screen", "raycap"),

        new("Bonus", null, true),
        new("Walk of Life", "ly_10"),
        new("Walk of Power", "ly_20"),
        new("Bonus Stage", "bonux"),

        new("Cutscenes", null, true),
        new("Council Chamber of the Teensies", "nego_10"),
        new("Meanwhile in the Prison Ship 1", "batam_10"),
        new("Meanwhile in the Prison Ship 2", "batam_20"),
        new("Echoing Caves Intro", "bast_09"),
        new("Iron Mountains Balloon Cutscene", "ball"),
        new("Ending", "end_10"),
        new("Credits", "staff_10"),
        new("Polokus Mask 1", "poloc_10"),
        new("Polokus Mask 2", "poloc_20"),
        new("Polokus Mask 3", "poloc_30"),
        new("Polokus Mask 4", "poloc_40"),

        new("The Woods of Light", null, true),
        new("Intro", "jail_10"),
        new("Jail", "jail_20"),
        new("Part 1", "learn_10"),

        new("The Fairy Glade", null, true),
        new("Part 1", "learn_30"),
        new("Part 2", "learn_31"),
        new("Part 3", "bast_20"),
        new("Part 4", "bast_22"),
        new("Part 5", "learn_60"),

        new("The Marshes of Awakening", null, true),
        new("Part 1", "ski_10"),
        new("Part 2", "ski_60"),

        new("The Bayou", null, true),
        new("Part 1", "chase_10"),
        new("Part 2", "chase_22"),

        new("The Sanctuary of Water and Ice", null, true),
        new("Part 1", "water_10"),
        new("Part 2", "water_20"),

        new("The Menhir Hills", null, true),
        new("Part 1", "rodeo_10"),
        new("Part 2", "rodeo_40"),
        new("Part 3", "rodeo_60"),

        new("The Cave of Bad Dreams", null, true),
        new("Part 1", "vulca_10"),
        new("Part 2", "vulca_20"),

        new("The Canopy", null, true),
        new("Part 1", "glob_30"),
        new("Part 2", "glob_10"),
        new("Part 3", "glob_20"),

        new("Whale Bay", null, true),
        new("Part 1", "whale_00"),
        new("Part 2", "whale_05"),
        new("Part 3", "whale_10"),

        new("The Sanctuary of Stone and Fire", null, true),
        new("Part 1", "plum_00"),
        new("Part 2", "plum_20"),
        new("Part 3", "plum_10"),

        new("The Echoing Caves", null, true),
        new("Part 1", "bast_10"),
        new("Part 2", "cask_10"),
        new("Part 3", "cask_30"),

        new("The Precipice", null, true),
        new("Part 1", "nave_10"),
        new("Part 2", "nave_15"),
        new("Part 3", "nave_20"),

        new("The Top of the World", null, true),
        new("Part 1", "seat_10"),
        new("Part 2", "seat_11"),

        new("The Sanctuary of Rock and Lava", null, true),
        new("Part 1", "earth_10"),
        new("Part 2", "earth_20"),
        new("Part 3", "earth_30"),

        new("Beneath the Sanctuary of Rock and Lava", null, true),
        new("Part 1", "helic_10"),
        new("Part 2", "helic_20"),
        new("Part 3", "helic_30"),

        new("Tomb of the Ancients", null, true),
        new("Part 1", "morb_00"),
        new("Part 2", "morb_10"),
        new("Part 3", "morb_20"),

        new("The Iron Mountains", null, true),
        new("Part 1", "learn_40"),
        new("Part 2", "ile_10"),
        new("Part 3", "mine_10"),

        new("The Prison Ship", null, true),
        new("Part 1", "boat01"),
        new("Part 2", "boat02"),
        new("Part 3", "astro_00"),
        new("Part 4", "astro_10"),

        new("The Crow's Nest", null, true),
        new("Part 1", "rhop_10"),
    };

    private GroupedEditorDropDownFieldViewModel.DropDownItem<string>[] Maps_R3_PC => new GroupedEditorDropDownFieldViewModel.DropDownItem<string>[]
    {
        new("Menu", null, true),
        new("Main Menu", "menumap"),
        new("Staff Roll", "staff"),
        new("Bonus", "BonusTXT"),
        new("Endgame", "endgame"),

        new("Arcade", null, true),
        new("2D Madness", "toudi_00"),
        new("Racket Jump", "ten_map"),
        new("Crush", "crush"),
        new("Razoff Circus", "raz_map"),
        new("Sentinel", "sentinel"),
        new("Missile Command", "snipe_00"),
        new("Balloons", "ballmap"),
        new("Special Invaders", "ship_map"),
        new("Commando", "commando"),

        new("The Fairy Council", null, true),
        new("Part 1", "intro_10"),
        new("Part 2", "intro_15"),
        new("Part 3", "intro_17"),
        new("Part 4", "intro_20"),
        new("Part 5", "menu_00"),
        new("Part 6", "sk8_00"),

        new("Clearleaf Forest", null, true),
        new("Part 1", "wood_11"),
        new("Part 2", "wood_10"),
        new("Part 3", "wood_19"),
        new("Part 4", "wood_50"),
        new("Part 5", "menu_10"),
        new("Part 6", "sk8_10"),

        new("The Bog of Murk", null, true),
        new("Part 1", "swamp_60"),
        new("Part 2", "swamp_82"),
        new("Part 3", "swamp_81"),
        new("Part 4", "swamp_83"),
        new("Part 5", "swamp_50"),
        new("Part 6", "swamp_51"),

        new("The Land of the Livid Dead", null, true),
        new("Part 1", "moor_00"),
        new("Part 2", "moor_30"),
        new("Part 3", "moor_60"),
        new("Part 4", "moor_19"),
        new("Part 5", "menu_20"),
        new("Part 6", "sk8_20"),

        new("The Desert of the Knaaren", null, true),
        new("Part 1", "knaar_10"),
        new("Part 2", "knaar_20"),
        new("Part 3", "knaar_30"),
        new("Part 4", "knaar_45"),
        new("Part 5", "knaar_60"),
        new("Part 6", "knaar_69"),
        new("Part 7", "knaar_70"),
        new("Part 8", "menu_30"),

        new("The Longest Shortcut", null, true),
        new("Part 1", "flash_20"),
        new("Part 2", "flash_30"),
        new("Part 3", "flash_10"),

        new("The Summit Beyond the Clouds", null, true),
        new("Part 1", "sea_10"),
        new("Part 2", "mount_50"),
        new("Part 3 (Snowboard)", "mount_4x"),

        new("Hoodlum Headquarters", null, true),
        new("Part 1", "fact_40"),
        new("Part 2", "fact_50"),
        new("Part 3", "fact_55"),
        new("Part 4", "fact_34"),
        new("Part 5", "fact_22"),

        new("The Tower of the Leptys", null, true),
        new("Part 1", "tower_10"),
        new("Part 2", "tower_20"),
        new("Part 3", "tower_30"),
        new("Part 4", "tower_40"),
        new("Part 5", "lept_15"),
    };

    #endregion

    #region Public Properties

    public OpenSpaceSettings Settings { get; }
    public override string[] ProcessNameKeywords => new[] { "Rayman2", "Rayman3" }; // TODO-UPDATE: Move to game VM

    #endregion

    #region Public Methods
    
    public override void InitializeContext(Context context)
    {
        context.AddSettings(Settings);
    }

    public override IEnumerable<EditorFieldGroupViewModel> CreateEditorFieldGroups()
    {
        GroupedEditorDropDownFieldViewModel.DropDownItem<string>[] mapDropDowns = Settings.EngineVersion switch
        {
            EngineVersion.Rayman2 when Settings.Platform == Platform.PC => Maps_R2_PC,
            EngineVersion.Rayman3 when Settings.Platform == Platform.PC => Maps_R3_PC,
            _ => throw new IndexOutOfRangeException()
        };

        // TODO-UPDATE: Localize
        yield return new EditorFieldGroupViewModel(
            header: "General",
            editorFields: new EditorFieldViewModel[]
            {
                new GroupedEditorDropDownFieldViewModel(
                    header: "Current map", 
                    info: null,
                    getValueAction: () => AccessMemory(m => mapDropDowns.FindItemIndex(x => x.Data == m.CurrentMap)),
                    setValueAction: x => AccessMemory(m =>
                    {
                        m.CurrentMap = mapDropDowns[x].Data;
                        m.EngineMode = 6;

                        m.ModifiedValue(nameof(m.EngineMode), nameof(m.CurrentMap));
                    }),
                    getItemsAction: () => mapDropDowns)
            });
    }

    public override IEnumerable<DuoGridItemViewModel> CreateInfoItems()
    {
        yield return DuoGridItem("Map", m => m.CurrentMap);
    }

    public override IEnumerable<Mod_Mem_ActionViewModel> CreateActions()
    {
        yield break;
    }

    #endregion
}