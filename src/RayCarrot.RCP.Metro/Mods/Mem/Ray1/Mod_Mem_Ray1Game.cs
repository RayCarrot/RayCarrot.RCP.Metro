using BinarySerializer;
using BinarySerializer.Ray1;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

public class Mod_Mem_Ray1Game : Mod_Mem_Game<Mod_Mem_Ray1MemoryData>
{
    #region Constructor

    public Mod_Mem_Ray1Game(Ray1EngineVersion version)
    {
        Version = version;
    }

    #endregion

    #region Public Properties

    public Ray1EngineVersion Version { get; }

    #endregion

    #region Private Methods

    private IEnumerable<EditorFieldViewModel> CreateEditorFields_General()
    {
        yield return new EditorIntFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Progression_Lives)),
            info: null,
            getValueAction: () => AccessMemory(m => m.StatusBar?.LivesCount ?? 0),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.StatusBar == null)
                    return;

                m.StatusBar.LivesCount = (byte)x;
                m.ModifiedValue(nameof(m.StatusBar));
            }),
            max: 99);

        yield return new EditorIntFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Progression_Tings)),
            info: null,
            getValueAction: () => AccessMemory(m => m.StatusBar?.TingsCount ?? 0),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.StatusBar == null)
                    return;

                m.StatusBar.TingsCount = (byte)x;
                m.ModifiedValue(nameof(m.StatusBar));
            }),
            max: 99);

        yield return new EditorIntFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_HP)),
            info: null,
            getValueAction: () => AccessMemory(m => Version == Ray1EngineVersion.R2_PS1
                ? m.R2_Ray?.HitPoints ?? 0
                : m.Ray?.HitPoints ?? 0),
            setValueAction: x => AccessMemory(m =>
            {
                if (Version == Ray1EngineVersion.R2_PS1)
                {
                    if (m.R2_Ray == null)
                        return;

                    m.R2_Ray.HitPoints = (byte)x;
                    m.ModifiedValue(nameof(m.R2_Ray));
                }
                else
                {
                    if (m.Ray == null)
                        return;

                    m.Ray.HitPoints = (byte)Math.Min(x, m.StatusBar?.MaxHealth ?? 0);
                    m.ModifiedValue(nameof(m.Ray));
                }
            }));

        if (Version == Ray1EngineVersion.R2_PS1)
        {
            yield return new EditorIntFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_MaxHP)),
                info: null,
                getValueAction: () => AccessMemory(m => m.StatusBar?.MaxHealth ?? 0),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.StatusBar == null)
                        return;

                    m.StatusBar.MaxHealth = (byte)x;
                    m.ModifiedValue(nameof(m.StatusBar));
                }),
                max: Byte.MaxValue);
        }
        else
        {
            byte min = Version switch
            {
                Ray1EngineVersion.GBA => 3,
                _ => 2
            };
            byte max = Version switch
            {
                Ray1EngineVersion.GBA => 5,
                _ => 4
            };

            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_MaxHP)),
                info: null,
                getValueAction: () => AccessMemory(m => m.StatusBar?.MaxHealth == max),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.StatusBar == null)
                        return;

                    m.StatusBar.MaxHealth = x ? max : min;
                    m.ModifiedValue(nameof(m.StatusBar));
                }));
        }

        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_PlaceRay)),
            info: new ResourceLocString(nameof(Resources.Mod_Mem_R1_PlaceRayInfo)),
            getValueAction: () => AccessMemory(m => (short)m.RayMode < 0),
            setValueAction: x => AccessMemory(m =>
            {
                bool isEnabled = (short)m.RayMode < 0;

                if (isEnabled != x)
                    m.RayMode = (RayMode)((short)m.RayMode * -1);

                m.ModifiedValue(nameof(m.RayMode));
            }));

        if (Version != Ray1EngineVersion.R2_PS1)
        {
            int[] lvls = Version switch
            {
                Ray1EngineVersion.PC => new[] { 0, 22, 18, 13, 13, 12, 4 }, // Breakout is map 22 in Jungle
                Ray1EngineVersion.GBA => new[] { 0, 21, 18, 13, 13, 12, 4, 0, 6 }, // Multiplayer is world 8
                _ => new[] { 0, 21, 18, 13, 13, 12, 4 },
            };

            EditorDropDownFieldViewModel.DropDownItem[][] levelDropDowns =
                lvls.Select(x => Enumerable.Range(0, x).
                        Select(l => new EditorDropDownFieldViewModel.DropDownItem($"Map {l + 1}", null)).
                        ToArray())
                    .ToArray();

            yield return new EditorDropDownFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_CurrentMap)),
                info: null,
                getValueAction: () => AccessMemory(m => m.NumLevel - 1),
                setValueAction: x => AccessMemory(m =>
                {
                    m.NumLevelChoice = (short)(x + 1);
                    m.NewLevel = 1;
                    m.ModifiedValue(nameof(m.NumLevelChoice), nameof(m.NewLevel));
                }),
                getItemsAction: () => levelDropDowns[AccessMemory(m => m.NumWorld)]);
        }

        if (AccessMemory(m => m.SupportsProperty(nameof(m.AllWorld))))
            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_MapSelect)),
                info: new ResourceLocString(nameof(Resources.Mod_Mem_R1_MapSelectInfo)),
                getValueAction: () => AccessMemory(m => m.AllWorld),
                setValueAction: x => AccessMemory(m =>
                {
                    m.AllWorld = x;
                    m.ModifiedValue(nameof(m.AllWorld));
                }));

        if (Version == Ray1EngineVersion.R2_PS1)
        {
            string[] langs = { "French", "English", "German", "Italian", "Spanish" };
            var langItems = langs.Select(x => new EditorDropDownFieldViewModel.DropDownItem(x, null)).ToArray();

            yield return new EditorDropDownFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_Lang)),
                info: null,
                getValueAction: () => AccessMemory(m => m.R2_Language),
                setValueAction: x => AccessMemory(m =>
                {
                    m.R2_Language = x;
                    m.ModifiedValue(nameof(m.R2_Language));
                }),
                getItemsAction: () => langItems);
        }
    }

    private IEnumerable<EditorFieldViewModel> CreateEditorFields_Powers()
    {
        if (Version == Ray1EngineVersion.R2_PS1)
        {
            return new (R2_RayEvts Evts, LocalizedString Header)[]
            {
                (R2_RayEvts.Fist, new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistPower))),
                (R2_RayEvts.Flag_1, new ResourceLocString(nameof(Resources.Mod_Mem_Unk), 1)),
                (R2_RayEvts.Flag_2, new ResourceLocString(nameof(Resources.Mod_Mem_Unk), 2)),
                (R2_RayEvts.FistPlatform, new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistPlatform))),
                (R2_RayEvts.Hang, new ResourceLocString(nameof(Resources.Mod_Mem_R1_HangPower))),
                (R2_RayEvts.Helico, new ResourceLocString(nameof(Resources.Mod_Mem_R1_HelicoPower))),
                (R2_RayEvts.SuperHelico, new ResourceLocString(nameof(Resources.Mod_Mem_R1_SuperHelicoPower))),
                (R2_RayEvts.Seed, new ResourceLocString(nameof(Resources.Mod_Mem_R1_SeedPower))),
                (R2_RayEvts.Flag_8, new ResourceLocString(nameof(Resources.Mod_Mem_Unk), 8)),
                (R2_RayEvts.Grab, new ResourceLocString(nameof(Resources.Mod_Mem_R1_GrabPower))),
                (R2_RayEvts.Run, new ResourceLocString(nameof(Resources.Mod_Mem_R1_RunPower))),
            }.Select(ev => new EditorBoolFieldViewModel(
                header: ev.Header,
                info: null,
                getValueAction: () => AccessMemory(m => (m.R2_RayEvts & ev.Evts) != 0),
                setValueAction: x => AccessMemory(m =>
                {
                    if (x)
                        m.R2_RayEvts |= ev.Evts;
                    else
                        m.R2_RayEvts &= ~ev.Evts;

                    m.ModifiedValue(nameof(m.R2_RayEvts));
                })));
        }
        else
        {
            return new (RayEvts Evts, LocalizedString Header)[]
            {
                (RayEvts.Fist, new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistPower))),
                (RayEvts.Hang, new ResourceLocString(nameof(Resources.Mod_Mem_R1_HangPower))),
                (RayEvts.Helico, new ResourceLocString(nameof(Resources.Mod_Mem_R1_HelicoPower))),
                (RayEvts.Grab, new ResourceLocString(nameof(Resources.Mod_Mem_R1_GrabPower))),
                (RayEvts.Run, new ResourceLocString(nameof(Resources.Mod_Mem_R1_RunPower))),
                (RayEvts.Seed, new ResourceLocString(nameof(Resources.Mod_Mem_R1_SeedPower))),
                (RayEvts.SuperHelico, new ResourceLocString(nameof(Resources.Mod_Mem_R1_SuperHelicoPower))),
                (RayEvts.SquishedRayman, new ResourceLocString(nameof(Resources.Mod_Mem_R1_SquishedPower))),
                (RayEvts.Firefly, new ResourceLocString(nameof(Resources.Mod_Mem_R1_FireflyPower))),
                (RayEvts.ForceRun, new ResourceLocString(nameof(Resources.Mod_Mem_R1_ForcedRunPower))),
                (RayEvts.ReverseControls, new ResourceLocString(nameof(Resources.Mod_Mem_R1_ReversePower))),
            }.Select(ev => new EditorBoolFieldViewModel(
                header: ev.Header,
                info: null,
                getValueAction: () => AccessMemory(m => (m.RayEvts & ev.Evts) != 0),
                setValueAction: x => AccessMemory(m =>
                {
                    if (x)
                        m.RayEvts |= ev.Evts;
                    else
                        m.RayEvts &= ~ev.Evts;

                    m.ModifiedValue(nameof(m.RayEvts));
                })));
        }
    }

    private IEnumerable<EditorFieldViewModel> CreateEditorFields_R2_Debug()
    {
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_EngineText)),
            info: null,
            getValueAction: () => AccessMemory(m => m.R2_ShowEngineInfo),
            setValueAction: x => AccessMemory(m =>
            {
                m.R2_ShowEngineInfo = x;
                m.ModifiedValue(nameof(m.R2_ShowEngineInfo));
            }));

        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_ObjText)),
            info: null,
            getValueAction: () => AccessMemory(m => m.R2_UnusedMapLoopFunctionCall == 0x0c0337c4),
            setValueAction: x => AccessMemory(m =>
            {
                // Replace unused function call in map loop
                m.R2_UnusedMapLoopFunctionCall = x ? 0x0c0337c4U : 0x0c045763U;
                m.ModifiedValue(nameof(m.R2_UnusedMapLoopFunctionCall));
            }));

        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_DebugMode)),
            info: null,
            getValueAction: () => AccessMemory(m => m.R2_DebugMode),
            setValueAction: x => AccessMemory(m =>
            {
                m.R2_DebugMode = x;
                m.ModifiedValue(nameof(m.R2_DebugMode));
            }));

        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_DemoMode)),
            info: new ResourceLocString(nameof(Resources.Mod_Mem_R1_DemoModeInfo)),
            getValueAction: () => AccessMemory(m => m.R2_MapInitFunctionCall == 0x0c03fc4b),
            setValueAction: x => AccessMemory(m =>
            {
                // Replace function call in map init
                m.R2_MapInitFunctionCall = x ? 0x0c03fc4bU : 0x0c03db0dU;
                m.ModifiedValue(nameof(m.R2_MapInitFunctionCall));
            }));
    }

    private IEnumerable<EditorFieldViewModel> CreateEditorFields_GBA_Multiplayer()
    {
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_MultiplayerMenus)),
            info: new ResourceLocString(nameof(Resources.Mod_Mem_R1_MultiplayerMenusInfo)),
            getValueAction: () => AccessMemory(m => m.GBA_EnableMultiplayerMenus),
            setValueAction: x => AccessMemory(m =>
            {
                m.GBA_EnableMultiplayerMenus = x;
                m.GBA_MultiplayerLevelLoad = (ushort)(x ? 0 : 0x7004);
                m.ModifiedValue(nameof(m.GBA_EnableMultiplayerMenus), nameof(m.GBA_MultiplayerLevelLoad));
            }));

        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_MultiplayerTimeout)),
            info: null,
            getValueAction: () => AccessMemory(m => m.GBA_MultiplayerTimeout == 0xE6DA),
            setValueAction: x => AccessMemory(m =>
            {
                m.GBA_MultiplayerTimeout = (ushort)(x ? 0xE6DA : 0);
                m.ModifiedValue(nameof(m.GBA_MultiplayerTimeout));
            }));

        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_MultiplayerP2)),
            info: null,
            getValueAction: () => AccessMemory(m => m.GBA_MultiplayerPlayerSelection),
            setValueAction: x => AccessMemory(m =>
            {
                m.GBA_MultiplayerPlayerSelection = x;
                m.ModifiedValue(nameof(m.GBA_MultiplayerPlayerSelection));
            }));

        EditorDropDownFieldViewModel.DropDownItem[] maps = Enumerable.Range(0, 6).
            Select(x => new EditorDropDownFieldViewModel.DropDownItem($"{x + 1}", null)).
            ToArray();

        yield return new EditorDropDownFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_MultiplayerMap)),
            info: null,
            getValueAction: () => AccessMemory(m => m.GBA_MultiplayerLevelSelection - 1),
            setValueAction: x => AccessMemory(m =>
            {
                m.GBA_MultiplayerLevelSelection = (byte)(x + 1);
                m.ModifiedValue(nameof(m.GBA_MultiplayerLevelSelection));
            }),
            getItemsAction: () => maps);
    }

    #endregion

    #region Public Methods
    
    public override void InitializeContext(Context context)
    {
        context.AddSettings(new Ray1Settings(Version));
    }

    public override IEnumerable<EditorFieldGroupViewModel> CreateEditorFieldGroups()
    {
        yield return new EditorFieldGroupViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_GeneralCategory)),
            editorFields: CreateEditorFields_General());

        if (Version == Ray1EngineVersion.R2_PS1)
            yield return new EditorFieldGroupViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_DebugCategory)),
                editorFields: CreateEditorFields_R2_Debug());

        if (Version == Ray1EngineVersion.GBA)
            yield return new EditorFieldGroupViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_MultiplayerCategory)),
                editorFields: CreateEditorFields_GBA_Multiplayer());

        yield return new EditorFieldGroupViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_PowersCategory)),
            editorFields: CreateEditorFields_Powers());
    }

    public override IEnumerable<DuoGridItemViewModel> CreateInfoItems()
    {
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_CamX)), m => m.XMap);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_CamY)), m => m.YMap);

        if (Version == Ray1EngineVersion.R2_PS1)
        {
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_XPos)), m => m.R2_Ray?.XPosition);
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_YPos)), m => m.R2_Ray?.YPosition);
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_RayState)), m => $"{m.R2_Ray?.Etat}-{m.R2_Ray?.SubEtat}");
        }
        else
        {
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_XPos)), m => m.Ray?.XPosition);
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_YPos)), m => m.Ray?.YPosition);
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_RayState)), m => $"{m.Ray?.Etat}-{m.Ray?.SubEtat}");
        }

        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_HelicoTime)), m => m.HelicoTime);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistCharge)), m => m.Poing?.FistChargedLevel);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_ActiveObjs)), m => m.ActiveObjects?.ActiveObjectsCount);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_MapTime)), m => m.MapTime);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_RandIndex)), m => m.RandomIndex);

        if (AccessMemory(m => m.SupportsProperty(nameof(m.MenuEtape))))
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_Menu)), m => m.MenuEtape);
    }

    public override IEnumerable<Mod_Mem_ActionViewModel> CreateActions()
    {
        if (AccessMemory(m => m.SupportsProperty(nameof(m.FinBoss))))
            yield return new Mod_Mem_ActionViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_FinishLvlAction)),
                iconKind: PackIconMaterialKind.FlagOutline,
                command: new RelayCommand(() => AccessMemory(m =>
                {
                    m.FinBoss = true;
                    m.ModifiedValue(nameof(m.FinBoss));
                })),
                isEnabledFunc: () => AccessMemory(m => m.Ray is { Etat: 0, SpeedX: 0, SpeedY: 0, Short_4A: -1 }));

        if (AccessMemory(m => m.SupportsProperty(nameof(m.WorldInfo))))
        {
            yield return new Mod_Mem_ActionViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_UnlockAllLvlsAction)),
                iconKind: PackIconMaterialKind.CircleMultipleOutline,
                command: new RelayCommand(() => AccessMemory(m =>
                {
                    if (m.WorldInfo == null)
                        return;

                    foreach (WorldInfo w in m.WorldInfo)
                    {
                        if ((w.Runtime_State & 1) == 0)
                            w.Runtime_State |= 4;
                    }

                    m.ModifiedValue(nameof(m.WorldInfo));
                })),
                isEnabledFunc: () => true);

            yield return new Mod_Mem_ActionViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_AllCagesAction)),
                iconKind: PackIconMaterialKind.CircleMultiple,
                command: new RelayCommand(() => AccessMemory(m =>
                {
                    if (m.WorldInfo == null)
                        return;

                    foreach (WorldInfo w in m.WorldInfo)
                    {
                        w.Runtime_Cages = 6;

                        if ((w.Runtime_State & 1) == 0)
                            w.Runtime_State |= 4;
                    }

                    m.ModifiedValue(nameof(m.WorldInfo));
                })),
                isEnabledFunc: () => true);
        }

        if (AccessMemory(m => m.SupportsProperty(nameof(m.NewWorld))))
            yield return new Mod_Mem_ActionViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_ExitLvlAction)),
                iconKind: PackIconMaterialKind.MapOutline,
                command: new RelayCommand(() => AccessMemory(m =>
                {
                    m.NewWorld = 1;
                    m.ModifiedValue(nameof(m.NewWorld));
                })),
                isEnabledFunc: () => true);
    }

    #endregion
}