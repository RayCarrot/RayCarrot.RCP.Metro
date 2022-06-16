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

    private GroupedEditorDropDownFieldViewModel.DropDownItem<string>[] Maps_R2_PC => new GroupedEditorDropDownFieldViewModel.DropDownItem<string>[] 
    {
        new(new ResourceLocString(nameof(Resources.Mod_Mem_MenuLevelsCategory)), null, true),
        new(new ResourceLocString(nameof(Resources.Mod_Mem_MenuLevel)), "menu"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_mapmonde)), "mapmonde"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_raycap)), "raycap"),

        new(new ResourceLocString(nameof(Resources.Mod_Mem_BonusLevelsCategory)), null, true),
        new(new ResourceLocString(nameof(Resources.R2_BonusLevelName_1)), "ly_10"),
        new(new ResourceLocString(nameof(Resources.R2_BonusLevelName_2)), "ly_20"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_bonux)), "bonux"),

        new(new ResourceLocString(nameof(Resources.Mod_Mem_CutsceneLevelsCategory)), null, true),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_jail_10)), "jail_10"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_jail_20)), "jail_20"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_nego_10)), "nego_10"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_batam_10)), "batam_10"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_batam_20)), "batam_20"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_bast_09)), "bast_09"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_ball)), "ball"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_end_10)), "end_10"),
        new(new ResourceLocString(nameof(Resources.Mod_Mem_CreditsLevel)), "staff_10"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_poloc_10)), "poloc_10"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_poloc_20)), "poloc_20"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_poloc_30)), "poloc_30"),
        new(new ResourceLocString(nameof(Resources.R2_LevelName_poloc_40)), "poloc_40"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_learn_10)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "learn_10"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_bast)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "learn_30"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "learn_31"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "bast_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "bast_22"),
        new(new ResourceLocString(nameof(Resources.PartX), 5), "learn_60"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_ski)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "ski_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "ski_60"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_chase)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "chase_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "chase_22"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_water)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "water_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "water_20"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_rodeo)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "rodeo_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "rodeo_40"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "rodeo_60"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_vulca)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "vulca_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "vulca_20"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_glob)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "glob_30"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "glob_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "glob_20"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_whale)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "whale_00"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "whale_05"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "whale_10"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_plum)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "plum_00"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "plum_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "plum_10"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_cask)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "bast_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "cask_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "cask_30"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_nave)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "nave_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "nave_15"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "nave_20"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_seat)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "seat_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "seat_11"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_earth)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "earth_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "earth_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "earth_30"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_helic)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "helic_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "helic_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "helic_30"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_morb)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "morb_00"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "morb_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "morb_20"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_ile)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "learn_40"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "ile_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "mine_10"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_astro)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "boat01"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "boat02"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "astro_00"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "astro_10"),

        new(new ResourceLocString(nameof(Resources.R2_LevelName_rhop)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "rhop_10"),
    };

    private GroupedEditorDropDownFieldViewModel.DropDownItem<string>[] Maps_R3_PC => new GroupedEditorDropDownFieldViewModel.DropDownItem<string>[]
    {
        new(new ResourceLocString(nameof(Resources.Mod_Mem_MenuLevelsCategory)), null, true),
        new(new ResourceLocString(nameof(Resources.Mod_Mem_MenuLevel)), "menumap"),
        new(new ResourceLocString(nameof(Resources.Mod_Mem_CreditsLevel)), "staff"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_BonusTXT)), "BonusTXT"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_endgame)), "endgame"),

        new(new ResourceLocString(nameof(Resources.Mod_Mem_BonusLevelsCategory)), null, true),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_toudi_00)), "toudi_00"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_ten_map)), "ten_map"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_crush)), "crush"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_raz_map)), "raz_map"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_sentinel)), "sentinel"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_snipe_00)), "snipe_00"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_ballmap)), "ballmap"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_ship_map)), "ship_map"),
        new(new ResourceLocString(nameof(Resources.R3_LevelName_commando)), "commando"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level1Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "intro_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "intro_15"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "intro_17"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "intro_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 5), "menu_00"),
        new(new ResourceLocString(nameof(Resources.PartX), 6), "sk8_00"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level2Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "wood_11"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "wood_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "wood_19"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "wood_50"),
        new(new ResourceLocString(nameof(Resources.PartX), 5), "menu_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 6), "sk8_10"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level3Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "swamp_60"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "swamp_82"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "swamp_81"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "swamp_83"),
        new(new ResourceLocString(nameof(Resources.PartX), 5), "swamp_50"),
        new(new ResourceLocString(nameof(Resources.PartX), 6), "swamp_51"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level4Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "moor_00"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "moor_30"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "moor_60"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "moor_19"),
        new(new ResourceLocString(nameof(Resources.PartX), 5), "menu_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 6), "sk8_20"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level5Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "knaar_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "knaar_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "knaar_30"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "knaar_45"),
        new(new ResourceLocString(nameof(Resources.PartX), 5), "knaar_60"),
        new(new ResourceLocString(nameof(Resources.PartX), 6), "knaar_69"),
        new(new ResourceLocString(nameof(Resources.PartX), 7), "knaar_70"),
        new(new ResourceLocString(nameof(Resources.PartX), 8), "menu_30"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level6Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "flash_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "flash_30"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "flash_10"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level7Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "sea_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "mount_50"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "mount_4x"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level8Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "fact_40"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "fact_50"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "fact_55"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "fact_34"),
        new(new ResourceLocString(nameof(Resources.PartX), 5), "fact_22"),

        new(new ResourceLocString(nameof(Resources.Progression_R3_Level9Header)), null, true),
        new(new ResourceLocString(nameof(Resources.PartX), 1), "tower_10"),
        new(new ResourceLocString(nameof(Resources.PartX), 2), "tower_20"),
        new(new ResourceLocString(nameof(Resources.PartX), 3), "tower_30"),
        new(new ResourceLocString(nameof(Resources.PartX), 4), "tower_40"),
        new(new ResourceLocString(nameof(Resources.PartX), 5), "lept_15"),
    };

    #endregion

    #region Public Properties

    public OpenSpaceSettings Settings { get; }

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

        yield return new EditorFieldGroupViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_GeneralCategory)),
            editorFields: new EditorFieldViewModel[]
            {
                new GroupedEditorDropDownFieldViewModel(
                    header: new ResourceLocString(nameof(Resources.Mod_Mem_Map)),
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
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_Map)), m => m.CurrentMap);
    }

    public override IEnumerable<Mod_Mem_ActionViewModel> CreateActions()
    {
        yield break;
    }

    #endregion
}