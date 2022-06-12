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

    #region Public Properties

    public OpenSpaceSettings Settings { get; }
    public override string[] ProcessNameKeywords => new[] { "Rayman2" };

    #endregion

    #region Public Methods
    
    public override void InitializeContext(Context context)
    {
        context.AddSettings(Settings);
    }

    public override IEnumerable<EditorFieldGroupViewModel> CreateEditorFieldGroups()
    {
        GroupedEditorDropDownFieldViewModel.DropDownItem<string>[] mapDropDowns =
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
            new("The Woods of Light", "learn_10"),

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