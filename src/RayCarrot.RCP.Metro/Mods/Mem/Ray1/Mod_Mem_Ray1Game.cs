using System;
using System.Collections.Generic;
using System.Linq;
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
        // TODO-UPDATE: Localize
        yield return new EditorIntFieldViewModel(
            header: "Lives",
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
            header: "Tings",
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
            header: "Hit-points",
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
                header: "Max hit points",
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
                header: "Max hit points",
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
            header: "Place Ray",
            info: "Allows Rayman to be placed freely in the level",
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
                header: "Current level",
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
                header: "Map selection",
                info: "Toggles the in-game map selection on the world map",
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
                header: "Language",
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
        // TODO-UPDATE: Localize
        if (Version == Ray1EngineVersion.R2_PS1)
        {
            return new (R2_RayEvts Evts, LocalizedString Header)[]
            {
                (R2_RayEvts.Fist, "Fist"),
                (R2_RayEvts.Flag_1, "Unknown 1"),
                (R2_RayEvts.Flag_2, "Unknown 2"),
                (R2_RayEvts.FistPlatform, "Fist platform"),
                (R2_RayEvts.Hang, "Hang"),
                (R2_RayEvts.Helico, "Helico"),
                (R2_RayEvts.SuperHelico, "Super-helico"),
                (R2_RayEvts.Seed, "Seed"),
                (R2_RayEvts.Flag_8, "Unknown 8"),
                (R2_RayEvts.Grab, "Grab"),
                (R2_RayEvts.Run, "Run"),
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
                (RayEvts.Fist, "Fist"),
                (RayEvts.Hang, "Hang"),
                (RayEvts.Helico, "Helico"),
                (RayEvts.Grab, "Grab"),
                (RayEvts.Run, "Run"),
                (RayEvts.Seed, "Seed"),
                (RayEvts.SuperHelico, "Super-helico"),
                (RayEvts.SquishedRayman, "Squished"),
                (RayEvts.Firefly, "Firefly"),
                (RayEvts.ForceRun, "Force run"),
                (RayEvts.ReverseControls, "Reverse controls"),
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
        // TODO-UPDATE: Localize
        yield return new EditorBoolFieldViewModel(
            header: "Show engine info",
            info: null,
            getValueAction: () => AccessMemory(m => m.R2_ShowEngineInfo),
            setValueAction: x => AccessMemory(m =>
            {
                m.R2_ShowEngineInfo = x;
                m.ModifiedValue(nameof(m.R2_ShowEngineInfo));
            }));

        yield return new EditorBoolFieldViewModel(
            header: "Show objects info",
            info: null,
            getValueAction: () => AccessMemory(m => m.R2_UnusedMapLoopFunctionCall == 0x0c0337c4),
            setValueAction: x => AccessMemory(m =>
            {
                // Replace unused function call in map loop
                m.R2_UnusedMapLoopFunctionCall = x ? 0x0c0337c4U : 0x0c045763U;
                m.ModifiedValue(nameof(m.R2_UnusedMapLoopFunctionCall));
            }));

        yield return new EditorBoolFieldViewModel(
            header: "Debug mode",
            info: null,
            getValueAction: () => AccessMemory(m => m.R2_DebugMode),
            setValueAction: x => AccessMemory(m =>
            {
                m.R2_DebugMode = x;
                m.ModifiedValue(nameof(m.R2_DebugMode));
            }));

        yield return new EditorBoolFieldViewModel(
            header: "Start in demo mode",
            info: "If enabled then the game will start playing the two recorded demos when the level loads. This can be triggered by loosing all lives and choosing to try again.",
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
        // TODO-UPDATE: Localize
        yield return new EditorBoolFieldViewModel(
            header: "Load multiplayer menus",
            info: "When this is enabled the game will load the multiplayer menus on startup. If the game has already been loaded then this can be accessed by choosing to quit the game from the pause menu.\nThe multiplayer menus themselves are not complete due to this feature never being fully implemented. The first menu is the player selection and the second one the level selection. Due to these not being functional the only way to change these are by using the below fields.",
            getValueAction: () => AccessMemory(m => m.GBA_EnableMultiplayerMenus),
            setValueAction: x => AccessMemory(m =>
            {
                m.GBA_EnableMultiplayerMenus = x;
                m.GBA_MultiplayerLevelLoad = (ushort)(x ? 0 : 0x7004);
                m.ModifiedValue(nameof(m.GBA_EnableMultiplayerMenus), nameof(m.GBA_MultiplayerLevelLoad));
            }));

        yield return new EditorBoolFieldViewModel(
            header: "Level timeout",
            info: null,
            getValueAction: () => AccessMemory(m => m.GBA_MultiplayerTimeout == 0xE6DA),
            setValueAction: x => AccessMemory(m =>
            {
                m.GBA_MultiplayerTimeout = (ushort)(x ? 0xE6DA : 0);
                m.ModifiedValue(nameof(m.GBA_MultiplayerTimeout));
            }));

        yield return new EditorBoolFieldViewModel(
            header: "Is player 2",
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
            header: "Selected map",
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
        // TODO-UPDATE: Localize
        yield return new EditorFieldGroupViewModel(
            header: "General",
            editorFields: CreateEditorFields_General());

        if (Version == Ray1EngineVersion.R2_PS1)
            yield return new EditorFieldGroupViewModel(
                header: "Debug",
                editorFields: CreateEditorFields_R2_Debug());

        if (Version == Ray1EngineVersion.GBA)
            yield return new EditorFieldGroupViewModel(
                header: "Multiplayer",
                editorFields: CreateEditorFields_GBA_Multiplayer());

        yield return new EditorFieldGroupViewModel(
            header: "Powers",
            editorFields: CreateEditorFields_Powers());
    }

    public override IEnumerable<DuoGridItemViewModel> CreateInfoItems()
    {
        // TODO-UPDATE: Localize
        yield return DuoGridItem("Camera X", m => m.XMap);
        yield return DuoGridItem("Camera Y", m => $"{m.YMap}");

        if (Version == Ray1EngineVersion.R2_PS1)
        {
            yield return DuoGridItem("X position", m => $"{m.R2_Ray?.XPosition}");
            yield return DuoGridItem("Y position", m => $"{m.R2_Ray?.YPosition}");
            yield return DuoGridItem("Rayman state", m => $"{m.R2_Ray?.Etat}-{m.R2_Ray?.SubEtat}");
        }
        else
        {
            yield return DuoGridItem("X position", m => $"{m.Ray?.XPosition}");
            yield return DuoGridItem("Y position", m => $"{m.Ray?.YPosition}");
            yield return DuoGridItem("Rayman state", m => $"{m.Ray?.Etat}-{m.Ray?.SubEtat}");
        }

        yield return DuoGridItem("Helico time", m => $"{m.HelicoTime}");
        yield return DuoGridItem("Fist charge", m => $"{m.Poing?.FistChargedLevel}");
        yield return DuoGridItem("Active objects", m => $"{m.ActiveObjects?.ActiveObjectsCount}");
        yield return DuoGridItem("Map time", m => $"{m.MapTime}");
        yield return DuoGridItem("Random index", m => $"{m.RandomIndex}");

        if (AccessMemory(m => m.SupportsProperty(nameof(m.MenuEtape))))
            yield return DuoGridItem("Menu", m => $"{m.MenuEtape}");
    }

    public override IEnumerable<Mod_Mem_ActionViewModel> CreateActions()
    {
        if (AccessMemory(m => m.SupportsProperty(nameof(m.FinBoss))))
            yield return new Mod_Mem_ActionViewModel(
                header: "Finish level",
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
                header: "Unlock all levels",
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
                header: "All cages",
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
                header: "Return to world map",
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