using BinarySerializer;
using BinarySerializer.Ray1;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class Ray1GameManager : GameManager<Ray1MemoryData>
{
    #region Constructor

    public Ray1GameManager(
        LocalizedString displayName, 
        Func<Dictionary<string, long>> getOffsetsFunc) 
        : base(displayName, getOffsetsFunc)
    { }

    #endregion

    #region Private Methods

    private IEnumerable<EditorFieldViewModel> CreateEditorFields_General(Ray1Settings settings)
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
            getMaxAction: () => 99);

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
            getMaxAction: () => 99);

        yield return new EditorIntSliderFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_HP)),
            info: null,
            getValueAction: () => AccessMemory(m => settings.EngineVersion == Ray1EngineVersion.R2_PS1
                ? m.R2_Ray?.HitPoints ?? 0
                : m.Ray?.HitPoints ?? 0) + 1,
            setValueAction: x => AccessMemory(m =>
            {
                x--; 
                
                if (settings.EngineVersion == Ray1EngineVersion.R2_PS1)
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

                    m.Ray.HitPoints = (byte)x;
                    m.ModifiedValue(nameof(m.Ray));
                }
            }),
            getMinAction: () => 1,
            getMaxAction: () => AccessMemory(m => m.StatusBar?.MaxHealth ?? 0) + 1);

        if (settings.EngineVersion == Ray1EngineVersion.R2_PS1)
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
                getMaxAction: () => Byte.MaxValue);
        }
        else
        {
            byte min = settings.EngineVersion switch
            {
                Ray1EngineVersion.GBA => 3,
                _ => 2
            };
            byte max = settings.EngineVersion switch
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

                    if (settings.EngineVersion == Ray1EngineVersion.R2_PS1)
                    {
                        if (m.R2_Ray == null)
                            return;

                        if (m.R2_Ray.HitPoints > m.StatusBar.MaxHealth)
                        {
                            m.R2_Ray.HitPoints = m.StatusBar.MaxHealth;
                            m.ModifiedValue(nameof(m.R2_Ray));
                        }
                    }
                    else
                    {
                        if (m.Ray == null)
                            return;

                        if (m.Ray.HitPoints > m.StatusBar.MaxHealth)
                        {
                            m.Ray.HitPoints = m.StatusBar.MaxHealth;
                            m.ModifiedValue(nameof(m.Ray));
                        }
                    }
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

        if (settings.EngineVersion != Ray1EngineVersion.R2_PS1)
        {
            int[] lvls = settings.EngineVersion switch
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

        if (settings.EngineVersion == Ray1EngineVersion.R2_PS1)
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

    private IEnumerable<EditorFieldViewModel> CreateEditorFields_Powers(Ray1Settings settings)
    {
        // TODO-UPDATE: Potentially remove some of these since they break the game

        // Fist
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistPower)),
            info: null,
            getValueAction: () => AccessMemory(m => m.RayEvts?.Fist ?? false),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.RayEvts == null)
                    return;

                m.RayEvts.Fist = x;
                m.ModifiedValue(nameof(m.RayEvts));
            }));

        if (settings.EngineVersion == Ray1EngineVersion.R2_PS1)
        {
            // Fist down-slam
            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistDownSlam)),
                info: null,
                getValueAction: () => AccessMemory(m => m.RayEvts?.FistDownSlam ?? false),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.RayEvts == null)
                        return;

                    m.RayEvts.FistDownSlam = x;
                    m.ModifiedValue(nameof(m.RayEvts));
                }));

            // Fist control
            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistControl)),
                info: null,
                getValueAction: () => AccessMemory(m => m.RayEvts?.FistControl ?? false),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.RayEvts == null)
                        return;

                    m.RayEvts.FistControl = x;
                    m.ModifiedValue(nameof(m.RayEvts));
                }));

            // Fist platform
            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistPlatform)),
                info: null,
                getValueAction: () => AccessMemory(m => m.RayEvts?.FistPlatform ?? false),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.RayEvts == null)
                        return;

                    m.RayEvts.FistPlatform = x;
                    m.ModifiedValue(nameof(m.RayEvts));
                }));
        }

        // Hang
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_HangPower)),
            info: null,
            getValueAction: () => AccessMemory(m => m.RayEvts?.Hang ?? false),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.RayEvts == null)
                    return;

                m.RayEvts.Hang = x;
                m.ModifiedValue(nameof(m.RayEvts));
            }));

        // Helico
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_HelicoPower)),
            info: null,
            getValueAction: () => AccessMemory(m => m.RayEvts?.Helico ?? false),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.RayEvts == null)
                    return;

                m.RayEvts.Helico = x;
                m.ModifiedValue(nameof(m.RayEvts));
            }));

        // Super-helico
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_SuperHelicoPower)),
            info: null,
            getValueAction: () => AccessMemory(m => m.RayEvts?.SuperHelico ?? false),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.RayEvts == null)
                    return;

                m.RayEvts.SuperHelico = x;
                m.ModifiedValue(nameof(m.RayEvts));
            }));

        // Grab
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_GrabPower)),
            info: null,
            getValueAction: () => AccessMemory(m => m.RayEvts?.Grab ?? false),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.RayEvts == null)
                    return;

                m.RayEvts.Grab = x;
                m.ModifiedValue(nameof(m.RayEvts));
            }));

        // Run
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_RunPower)),
            info: null,
            getValueAction: () => AccessMemory(m => m.RayEvts?.Run ?? false),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.RayEvts == null)
                    return;

                m.RayEvts.Run = x;
                m.ModifiedValue(nameof(m.RayEvts));
            }));

        // Seed
        yield return new EditorBoolFieldViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_SeedPower)),
            info: null,
            getValueAction: () => AccessMemory(m => m.RayEvts?.Seed ?? false),
            setValueAction: x => AccessMemory(m =>
            {
                if (m.RayEvts == null)
                    return;

                m.RayEvts.Seed = x;
                m.ModifiedValue(nameof(m.RayEvts));
            }));

        if (settings.EngineVersion != Ray1EngineVersion.R2_PS1)
        {
            // Small Rayman
            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_SquishedPower)),
                info: null,
                getValueAction: () => AccessMemory(m => m.RayEvts?.SmallRayman ?? false),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.RayEvts == null)
                        return;

                    m.RayEvts.SmallRayman = x;
                    m.ModifiedValue(nameof(m.RayEvts));
                }));

            // Firefly
            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_FireflyPower)),
                info: null,
                getValueAction: () => AccessMemory(m => m.RayEvts?.Firefly ?? false),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.RayEvts == null)
                        return;

                    m.RayEvts.Firefly = x;
                    m.ModifiedValue(nameof(m.RayEvts));
                }));

            // Force run
            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_ForcedRunPower)),
                info: null,
                getValueAction: () => AccessMemory(m => m.RayEvts?.ForceRun ?? false),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.RayEvts == null)
                        return;

                    m.RayEvts.ForceRun = x;
                    m.ModifiedValue(nameof(m.RayEvts));
                }));

            // Reverse controls
            yield return new EditorBoolFieldViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_R1_ReversePower)),
                info: null,
                getValueAction: () => AccessMemory(m => m.RayEvts?.ReverseControls ?? false),
                setValueAction: x => AccessMemory(m =>
                {
                    if (m.RayEvts == null)
                        return;

                    m.RayEvts.ReverseControls = x;
                    m.ModifiedValue(nameof(m.RayEvts));
                }));
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
        // We're loading from memory, so the data is never packed
        Ray1Settings settings = context.GetRequiredSettings<Ray1Settings>();
        settings.IsLoadingPackedPCData = false;
    }

    public override IEnumerable<EditorFieldGroupViewModel> CreateEditorFieldGroups(Context context)
    {
        Ray1Settings settings = context.GetRequiredSettings<Ray1Settings>();

        yield return new EditorFieldGroupViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_GeneralCategory)),
            editorFields: CreateEditorFields_General(settings));

        if (settings.EngineVersion == Ray1EngineVersion.R2_PS1)
            yield return new EditorFieldGroupViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_DebugCategory)),
                editorFields: CreateEditorFields_R2_Debug());

        if (settings.EngineVersion == Ray1EngineVersion.GBA)
            yield return new EditorFieldGroupViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_MultiplayerCategory)),
                editorFields: CreateEditorFields_GBA_Multiplayer());

        yield return new EditorFieldGroupViewModel(
            header: new ResourceLocString(nameof(Resources.Mod_Mem_PowersCategory)),
            editorFields: CreateEditorFields_Powers(settings));
    }

    public override IEnumerable<DuoGridItemViewModel> CreateInfoItems(Context context)
    {
        Ray1Settings settings = context.GetRequiredSettings<Ray1Settings>();

        // TODO-LOC
        yield return DuoGridItem("Camera position", m => $"{m.XMap} x {m.YMap}");

        if (settings.EngineVersion == Ray1EngineVersion.R2_PS1)
        {
            // TODO-LOC
            yield return DuoGridItem("Rayman position", m => $"{m.R2_Ray?.XPosition} x {m.R2_Ray?.YPosition}");
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_RayState)), m => $"{m.R2_Ray?.MainEtat}-{m.R2_Ray?.SubEtat}");
        }
        else
        {
            // TODO-LOC
            yield return DuoGridItem("Rayman position", m => $"{m.Ray?.XPosition} x {m.Ray?.YPosition}");
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_RayState)), m => $"{m.Ray?.MainEtat}-{m.Ray?.SubEtat}");

            // Don't show speed storage on GBA since we're not serializing states there (see Ray1MemoryData)
            if (settings.EngineBranch != Ray1EngineBranch.GBA)
            {
                // TODO-LOC
                yield return DuoGridItem("Left speed storage", m => $"{m.Ray?.ETA.States[2][0x11].LeftSpeed}");
                yield return DuoGridItem("Right speed storage", m => $"{m.Ray?.ETA.States[2][0x11].RightSpeed}");
            }
        }

        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_HelicoTime)), m => m.HelicoTime);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_FistCharge)), m => m.Poing?.Charge);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_ActiveObjs)), m => m.ActiveObjects?.ActiveObjectsCount);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_MapTime)), m => m.MapTime);
        yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_RandIndex)), m => m.RandomIndex);

        if (AccessMemory(m => m.SupportsProperty(nameof(m.MenuEtape))))
            yield return DuoGridItem(new ResourceLocString(nameof(Resources.Mod_Mem_R1_Menu)), m => m.MenuEtape);
    }

    public override IEnumerable<ActionViewModel> CreateActions(Context context)
    {
        if (AccessMemory(m => m.SupportsProperty(nameof(m.FinBoss))))
            yield return new ActionViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_FinishLvlAction)),
                iconKind: PackIconMaterialKind.FlagOutline,
                command: new RelayCommand(() => AccessMemory(m =>
                {
                    m.FinBoss = true;
                    m.ModifiedValue(nameof(m.FinBoss));
                })),
                isEnabledFunc: () => AccessMemory(m => m.Ray is { MainEtat: 0, SpeedX: 0, SpeedY: 0, Short_4A: -1 }));

        if (AccessMemory(m => m.SupportsProperty(nameof(m.WorldInfo))))
        {
            yield return new ActionViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_UnlockAllLvlsAction)),
                iconKind: PackIconMaterialKind.CircleMultipleOutline,
                command: new RelayCommand(() => AccessMemory(m =>
                {
                    if (m.WorldInfo == null)
                        return;

                    foreach (WorldInfo w in m.WorldInfo)
                    {
                        if (!w.IsUnlocked)
                            w.IsUnlocking = true;
                    }

                    m.ModifiedValue(nameof(m.WorldInfo));
                })),
                isEnabledFunc: () => true);

            yield return new ActionViewModel(
                header: new ResourceLocString(nameof(Resources.Mod_Mem_AllCagesAction)),
                iconKind: PackIconMaterialKind.CircleMultiple,
                command: new RelayCommand(() => AccessMemory(m =>
                {
                    if (m.WorldInfo == null)
                        return;

                    foreach (WorldInfo w in m.WorldInfo)
                    {
                        w.CollectedCages = 6;

                        if (!w.IsUnlocked)
                            w.IsUnlocking = true;
                    }

                    m.ModifiedValue(nameof(m.WorldInfo));
                })),
                isEnabledFunc: () => true);
        }

        if (AccessMemory(m => m.SupportsProperty(nameof(m.NewWorld))))
            yield return new ActionViewModel(
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