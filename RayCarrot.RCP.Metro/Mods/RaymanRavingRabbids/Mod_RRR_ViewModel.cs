﻿using MahApps.Metro.IconPacks;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro
{
    public class Mod_RRR_ViewModel : Mod_BaseViewModel, IDisposable
    {
        #region Constructor

        public Mod_RRR_ViewModel()
        {
            MemoryPatcher = new Mod_RRR_MemoryPatcher();

            MemoryModSections = new ObservableCollection<Mod_RRR_MemoryModsSectonViewModel>()
            {
                // Main
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => Resources.Mod_RRR_MemMod_MainHeader), null, 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_EnableProtoFeatures), 
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_EnableProtoFeaturesInfo),
                            toggleAction: x =>
                            {
                                MemoryPatcher.enableProtoRaymanEverywhere = x;
                                MemoryModSections?.Skip(1).ForEach(section => section.IsEnabled = x);
                                IsCustomButtonMappingEnabled = x;
                            },
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddCamControls), 
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddCamControlsInfo),
                            toggleAction: x => MemoryPatcher.addCameraControls = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddDuel), 
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddDuelInfo),
                            toggleAction: x => MemoryPatcher.addDuel = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddPlaytestMenu), 
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddPlaytestMenuInfo),
                            toggleAction: x => MemoryPatcher.addPlaytestMenu = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_UnlockAllMinigames),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_UnlockAllMinigamesInfo),
                            toggleAction: x => MemoryPatcher.unlockAllMinigames = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_CheatPage),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_CheatPageInfo),
                            toggleAction: x => MemoryPatcher.setCheatPage = x,
                            isToggled: false,
                            selectionOptions: new ObservableCollection<LocalizedString>()
                            {
                                new LocalizedString(() => "1"),
                                new LocalizedString(() => "2"),
                                new LocalizedString(() => "3"),
                                new LocalizedString(() => "4"),
                                new LocalizedString(() => "5"),
                            },
                            selectionAction: x => MemoryPatcher.cheatPage = x + 1),
                    }),

                // Rayman
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => Resources.Mod_RRR_MemMod_RaymanHeader), new LocalizedString(() => Resources.Mod_RRR_MemMod_RaymanInfo), 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddLookMode),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddLookModeInfo),
                            toggleAction: x => MemoryPatcher.addLookMode = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddBoostButton),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddBoostButtonInfo),
                            toggleAction: x => MemoryPatcher.addBoostButton = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddFinishers),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_AddFinishersInfo),
                            toggleAction: x => MemoryPatcher.addFinishers = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_ClimbWalls),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_ClimbWallsInfo),
                            toggleAction: x => MemoryPatcher.climbWalls = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_HangFromSpots),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_HangFromSpotsInfo),
                            toggleAction: x => MemoryPatcher.hangFromHotspots = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_GrappinGFX),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_GrappinGFXInfo),
                            toggleAction: x => MemoryPatcher.setGrappinGFX = x,
                            isToggled: false,
                            selectionOptions: new ObservableCollection<LocalizedString>()
                            {
                                new LocalizedString(() => Resources.Mod_RRR_MemMod_GrappinGFX_0),
                                new LocalizedString(() => Resources.Mod_RRR_MemMod_GrappinGFX_1),
                                new LocalizedString(() => Resources.Mod_RRR_MemMod_GrappinGFX_2),
                                new LocalizedString(() => Resources.Mod_RRR_MemMod_GrappinGFX_3),
                                new LocalizedString(() => Resources.Mod_RRR_MemMod_GrappinGFX_4),
                            },
                            selectionAction: x => MemoryPatcher.GrappinGFX = (byte)x),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_UpgradedPowers),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_UpgradedPowersInfo),
                            toggleAction: x => MemoryPatcher.allpowers = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_Immortal),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_ImmortalInfo),
                            toggleAction: x => MemoryPatcher.immortal = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_ControlTempo),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_ControlTempoInfo),
                            toggleAction: x => MemoryPatcher.controlTempo = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_GroovyMoveset),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_GroovyMovesetInfo),
                            toggleAction: x => MemoryPatcher.groovyRaymanDanceMoveset = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_SetCostume),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_SetCostumeInfo),
                            toggleAction: x => MemoryPatcher.setPlayer1Costume = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_DisableIntros),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_DisableIntrosInfo),
                            toggleAction: x => MemoryPatcher.disableMinigameIntro = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_DisableFootstepSound),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_DisableFootstepSoundInfo),
                            toggleAction: x => MemoryPatcher.disableFootstepSound = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_LessSlippery),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_LessSlipperyInfo),
                            toggleAction: x => MemoryPatcher.lowerSlippery = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_DrawHealthMana),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_DrawHealthManaInfo),
                            toggleAction: x => MemoryPatcher.drawHealthMana = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_NoInstaKill),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_NoInstaKillInfo),
                            toggleAction: x => MemoryPatcher.noInstaKill = x,
                            isToggled: true),
                    }),

                // Rabbids
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => Resources.Mod_RRR_MemMod_RabbidsHeader), null, 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_RabbidsDropItems),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_RabbidsDropItemsInfo),
                            toggleAction: x => MemoryPatcher.rabbidsDropItems = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_IncreasedRabbidHP),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_IncreasedRabbidHPInfo),
                            toggleAction: x => MemoryPatcher.rabbidsIncreasedHP = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_RandomProtoRabbidPowers),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_RandomProtoRabbidPowersInfo),
                            toggleAction: x => MemoryPatcher.randomProtoRabbidPowers = x,
                            isToggled: false),
                    }),

                // Mounts
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => Resources.Mod_RRR_MemMod_MountsHeader), null, 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_TameMounts),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_TameMountsInfo),
                            toggleAction: x => MemoryPatcher.tameMounts = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_BatsActLikeEagles),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_BatsActLikeEaglesInfo),
                            toggleAction: x => MemoryPatcher.makeBatsIntoEagles = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_PigsActLikePlums),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_PigsActLikePlumsInfo),
                            toggleAction: x => MemoryPatcher.makePigsIntoPlums = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_AggressiveRhinos),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_AggressiveRhinosInfo),
                            toggleAction: x => MemoryPatcher.makeRhinosAggressive = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_SpiderJump),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_SpiderJumpInfo),
                            toggleAction: x => MemoryPatcher.makeSpidersJump = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_BatShoot),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_BatShootInfo),
                            toggleAction: x => MemoryPatcher.makeBatShoot = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_SaucersStartFlying),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_SaucersStartFlyingInfo),
                            toggleAction: x => MemoryPatcher.saucersStartFlying = x,
                            isToggled: false),
                    }),

                // Bunny hunts
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => Resources.Mod_RRR_MemMod_BunnyHuntsHeader), null, 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_MissileLaunchersTarget),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_MissileLaunchersTargetInfo),
                            toggleAction: x => MemoryPatcher.missileLaunchersTargetPlayer = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_DontDestroyBipods),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_DontDestroyBipodsInfo),
                            toggleAction: x => MemoryPatcher.dontDestroyBipods = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_ActivatorTriggers),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_ActivatorTriggersInfo),
                            toggleAction: x => MemoryPatcher.activateAllActivatorTriggers = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => Resources.Mod_RRR_MemMod_BoundingVolumeTriggers),
                            description: new LocalizedString(() => Resources.Mod_RRR_MemMod_BoundingVolumeTriggersInfo),
                            toggleAction: x => MemoryPatcher.activateAllPivotInBVTriggers = x,
                            isToggled: false),
                    }),
            };

            IsCustomButtonMappingEnabled = true;
            UseCustomButtonMapping = true;

            ButtonMappingItems = new ObservableCollection<ButtonMappingKeyItemViewModel<int>>()
            {
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_0), Key.A, 0), // Jump
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_4), Key.Space, 4), // Attack
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_10), Key.X, 10), // Attack (finisher)
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_6), Key.S, 6), // Grapple hook
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_14), Key.W, 14), // Roll/Ground pound
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_28), Key.LeftShift, 28), // Walk
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_8), Key.LeftCtrl, 8), // Light/Tempo
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_24), Key.D, 24), // Dance toggle
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_2), Key.B, 2), // Dance mode: turn
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_12), Key.E, 12), // Look mode
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_16), Key.P, 16), // No-clip
                new ButtonMappingKeyItemViewModel<int>(new LocalizedString(() => Resources.Mod_RRR_KeyAction_18), Key.Escape, 18), // Pause
            };

            BFModToggles = new ObservableCollection<Mod_RRR_BFModToggleViewModel>()
            {
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_FixSounds),
                    patches: Mod_RRR_BigFilePatch.FixSoundEffects,
                    isDefaultToggled: true),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer),
                    patch: Mod_RRR_BigFilePatch.PlayableCharacters,
                    isDefaultToggled: false,
                    selectionOptions: new ObservableCollection<LocalizedString>()
                    {
                        new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer_BabyGlobox), // Baby Globox
                        new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer_Serguei), // Serguei
                        new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer_Rabbid), // Rabbid
                        new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer_LeaderRabbid), // Grey Leader Rabbid
                        new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer_SupermanRabbid), // Superman Rabbid
                        new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer_TerminatorRabbid), // Terminator Rabbid (Pink)
                        new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer_SamFisherRabbid), // Sam Fisher Rabbid
                        new LocalizedString(() => Resources.Mod_RRR_BFPatch_ChangePlayer_NurgleDemon), // Nurgle Demon
                    }),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_BatSoundLikeEagle),
                    patch: Mod_RRR_BigFilePatch.MakeBatSoundLikeEagle,
                    isDefaultToggled: false),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_SpiderRobotSoundLikeSpider),
                    patches: Mod_RRR_BigFilePatch.MakeSpiderRobotsSoundLikeSpider,
                    isDefaultToggled: false),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_CustomHelicoTexture),
                    patch: Mod_RRR_BigFilePatch.AddCustomHelicopterTexture,
                    isDefaultToggled: true),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_RabbidsFur),
                    patches: Mod_RRR_BigFilePatch.AddFurToRabbids,
                    isDefaultToggled: false),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_Flashlight),
                    patch: Mod_RRR_BigFilePatch.EnableFlashlight,
                    isDefaultToggled: true),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_FlashlightMines),
                    patches: Mod_RRR_BigFilePatch.AddFlashlightToMines,
                    isDefaultToggled: false),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => Resources.Mod_RRR_BFPatch_RabbidItems),
                    patches: Mod_RRR_BigFilePatch.ModdedRabbidItems,
                    isDefaultToggled: false),
            };

            ApplyMemoryPatchCommand = new AsyncRelayCommand(ApplyMemoryPatchAsync);
            ApplyExePatchCommand = new AsyncRelayCommand(() => PatchExeAsync(true));
            RevertExePatchCommand = new AsyncRelayCommand(() => PatchExeAsync(false));
            DownloadPatchedBFCommand = new AsyncRelayCommand(DownloadPatchedBFAsync);
            RemovePatchedBFCommand = new AsyncRelayCommand(RemovePatchedBFAsync);
            UpdatePatchedBFCommand = new AsyncRelayCommand(UpdatePatchedBFAsync);
            LaunchWithPatchedBFCommand = new AsyncRelayCommand(LaunchWithPatchedBFAsync);
        }

        #endregion

        #region Commands

        public ICommand ApplyMemoryPatchCommand { get; }
        public ICommand ApplyExePatchCommand { get; }
        public ICommand RevertExePatchCommand { get; }
        public ICommand DownloadPatchedBFCommand { get; }
        public ICommand RemovePatchedBFCommand { get; }
        public ICommand UpdatePatchedBFCommand { get; }
        public ICommand LaunchWithPatchedBFCommand { get; }

        #endregion

        #region Mods Page

        public override GenericIconKind Icon => GenericIconKind.Games;
        public override LocalizedString Header => new LocalizedString(() => Resources.Mod_RRR_Header);
        public override object UIContent => new Mod_RRR_UI()
        {
            DataContext = this
        };

        #endregion

        #region Private Fields

        private FileSystemPath _gameDirectoryPath;

        #endregion

        #region Protected Properties

        protected Dictionary<GameVersion, FilePatcher_Patch> ExePatches { get; } = new()
        {
            // Steam
            [GameVersion.Steam] = new FilePatcher_Patch(0x45B000, new FilePatcher_Patch.PatchEntry[]
            {
                new FilePatcher_Patch.PatchEntry(
                    PatchOffset: 0x003C748D, 
                    OriginalBytes: new byte[]
                    {
                        0x68, 0x20, 0x16, 0xA3, 0x01, 0xE8, 0xB9, 0xFB,
                        0xFF, 0xFF, 0x83, 0xC4, 0x0C, 0xA3, 0x04, 0x7C,
                        0xC8, 0x00, 0xC3, 0x68, 0x20, 0x16, 0xA3, 0x01,
                        0xE8, 0x76, 0xF1, 0xFF, 0xFF, 0x83, 0xC4, 0x08,
                    }, 
                    PatchedBytes: new byte[]
                    {
                        0xE8, 0x02, 0x8C, 0x00, 0x00, 0x83, 0xC4, 0x04,
                        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                        0x90, 0x90, 0x90, 0xE8, 0xF5, 0x8B, 0x00, 0x00,
                        0x83, 0xC4, 0x04, 0x90, 0x90, 0x90, 0x90, 0x90,
                    }),
                new FilePatcher_Patch.PatchEntry(
                    PatchOffset: 0x003C74C9, 
                    OriginalBytes: new byte[]
                    {
                        0x8B, 0x0D, 0xE4, 0x7B, 0xC8, 0x00, 0x85, 0xC9,
                        0x74, 0x09, 0x50, 0xE8, 0x87, 0xF4, 0xFF, 0xFF,
                        0x83, 0xC4, 0x04,
                    }, 
                    PatchedBytes: new byte[]
                    {
                        0x50, 0xE8, 0xC5, 0x8B, 0x00, 0x00, 0x83, 0xC4,
                        0x04, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                        0x90, 0x90, 0x90,
                    }),
            }),

            // GOG
            [GameVersion.GOG] = new FilePatcher_Patch(0x459000, new FilePatcher_Patch.PatchEntry[]
            {
                new FilePatcher_Patch.PatchEntry(
                    PatchOffset: 0x003C73FD, 
                    OriginalBytes: new byte[]
                    {
                        0x68, 0x80, 0x22, 0xA2, 0x01, 0xE8, 0xB9, 0xFB,
                        0xFF, 0xFF, 0x83, 0xC4, 0x0C, 0xA3, 0x24, 0x7C,
                        0xC8, 0x00, 0xC3, 0x68, 0x80, 0x22, 0xA2, 0x01,
                        0xE8, 0x76, 0xF1, 0xFF, 0xFF, 0x83, 0xC4, 0x08,
                    }, 
                    PatchedBytes: new byte[]
                    {
                        0xE8, 0xB2, 0x8C, 0x00, 0x00, 0x83, 0xC4, 0x04,
                        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                        0x90, 0x90, 0x90, 0xE8, 0xA5, 0x8C, 0x00, 0x00,
                        0x83, 0xC4, 0x04, 0x90, 0x90, 0x90, 0x90, 0x90,
                    }),
                new FilePatcher_Patch.PatchEntry(
                    PatchOffset: 0x003C7439, 
                    OriginalBytes: new byte[]
                    {
                        0x8B, 0x0D, 0x04, 0x7C, 0xC8, 0x00, 0x85, 0xC9,
                        0x74, 0x09, 0x50, 0xE8, 0x87, 0xF4, 0xFF, 0xFF,
                        0x83, 0xC4, 0x04,
                    }, 
                    PatchedBytes: new byte[]
                    {
                        0x50, 0xE8, 0x75, 0x8C, 0x00, 0x00, 0x83, 0xC4,
                        0x04, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                        0x90, 0x90, 0x90,
                    }),
            }),
        };
        public FilePatcher ExePatcher => new FilePatcher(ExeFilePath, ExePatches.Values.ToArray());

        protected FileSystemPath ExeFilePath => GameDirectoryPath + "Jade_enr.exe";
        protected FileSystemPath PatchedBFFilePath => GameDirectoryPath + "Rayman4_Mod.bf";

        #endregion

        #region Public Properties

        public FileSystemPath GameDirectoryPath
        {
            get => _gameDirectoryPath;
            set
            {
                _gameDirectoryPath = value;
                GameDirectoryUpdated();
            }
        }

        // Memory Mods
        public Mod_RRR_MemoryPatcher MemoryPatcher { get; }
        public ObservableCollection<Mod_RRR_MemoryModsSectonViewModel> MemoryModSections { get; }
        public bool IsCustomButtonMappingEnabled { get; set; }
        public bool UseCustomButtonMapping
        {
            get => MemoryPatcher.fixKeyboardControls;
            set => MemoryPatcher.fixKeyboardControls = value;
        }
        public ObservableCollection<ButtonMappingKeyItemViewModel<int>> ButtonMappingItems { get; }

        // BF Mods
        public bool IsExePatched { get; set; }
        public bool IsPatchedBFDownloaded { get; set; }
        public bool CanDownloadPatchedBF { get; set; }
        public bool CanUpdatePatchedBF { get; set; }
        public ObservableCollection<Mod_RRR_BFModToggleViewModel> BFModToggles { get; }

        #endregion

        #region Protected Methods

        protected void GameDirectoryUpdated()
        {
            RL.Logger?.LogTraceSource("Updating RRR mod directory");
            IsExePatched = CheckIsExePatched();
            CanDownloadPatchedBF = IsExePatched;
            IsPatchedBFDownloaded = CheckIsPatchedBFDownloaded();
            CanUpdatePatchedBF = IsPatchedBFDownloaded && IsExePatched;
            RefreshBFPatches();
        }

        #endregion

        #region Public Methods

        public override Task InitializeAsync()
        {
            // Set the default directory to that of the game, if it's been added
            GameDirectoryPath = Games.RaymanRavingRabbids.GetInstallDir(false);

            // Restore saved button mapping
            foreach (var buttonItem in Data.Mod_RRR_KeyboardButtonMapping)
            {
                var matchingItem = ButtonMappingItems.FirstOrDefault(x => x.KeyObj == buttonItem.Key);

                if (matchingItem != null)
                    matchingItem.NewKey = buttonItem.Value;
            }

            return Task.CompletedTask;
        }

        public async Task ApplyMemoryPatchAsync()
        {
            RL.Logger?.LogInformationSource("Applying RRR memory patch");

            try
            {
                // Update keyboard controls
                if (UseCustomButtonMapping)
                {
                    foreach (var buttonMappingItem in ButtonMappingItems)
                        MemoryPatcher.KeyboardKeycodes[buttonMappingItem.KeyObj] = DirectXKeyHelpers.GetKeyCode(buttonMappingItem.NewKey);

                    // Save the button mapping
                    Data.Mod_RRR_KeyboardButtonMapping = ButtonMappingItems.ToDictionary(x => x.KeyObj, x => x.NewKey);
                }

                MemoryPatcher.Patch();

                RL.Logger?.LogInformationSource("RRR memory patch applied");

                await Services.MessageUI.DisplayMessageAsync(Resources.Mod_RRR_MemMod_ApplySuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying RRR memory patch");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Mod_RRR_MemMod_ApplyError);
            }
        }

        public async Task<GameVersion> DetermineGameVersionAsync()
        {
            RL.Logger?.LogInformationSource("Determining RRR game version");

            FileSystemPath exe = ExeFilePath;

            if (!exe.FileExists)
            {
                RL.Logger?.LogInformationSource("RRR exe does not exist");
                await Services.MessageUI.DisplayMessageAsync(Resources.Mod_RRR_BFPatch_InvalidGameDir, MessageType.Error);
                return GameVersion.Unknown;
            }

            long fileSize;

            try
            {
                fileSize = (long)exe.GetSize().Bytes;
                RL.Logger?.LogInformationSource($"RRR exe size is {fileSize} bytes");
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting RRR exe file size");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Mod_RRR_BFPatch_DetermineGameVerError);

                return GameVersion.Unknown;
            }

            GameVersion version = ExePatches.FirstOrDefault(x => x.Value.FileSize == fileSize).Key;

            RL.Logger?.LogInformationSource($"RRR game version detected as {version}");

            if (version == GameVersion.Unknown)
                await Services.MessageUI.DisplayMessageAsync(Resources.Mod_RRR_BFPatch_UnsupportedGameExe, MessageType.Error);

            return version;
        }

        public bool CheckIsExePatched()
        {
            RL.Logger?.LogInformationSource("Checking if RRR exe is patched");

            bool? isOriginal = ExePatcher.GetIsOriginal();

            if (isOriginal == true)
                RL.Logger?.LogInformationSource("RRR exe is not patched");
            else if (isOriginal == false)
                RL.Logger?.LogInformationSource("RRR exe is patched");
            else if (isOriginal == null)
                RL.Logger?.LogInformationSource("Could not determine if RRR exe is patched");
            
            return isOriginal == false;
        }

        public bool CheckIsPatchedBFDownloaded()
        {
            RL.Logger?.LogInformationSource("Checking if RRR patched BF is downloaded");

            var isDownloaded = PatchedBFFilePath.FileExists;

            if (isDownloaded)
                RL.Logger?.LogInformationSource("RRR patched BF is downloaded");
            else
                RL.Logger?.LogInformationSource("RRR patched BF is not downloaded");

            return isDownloaded;
        }

        public async Task PatchExeAsync(bool enablePatch)
        {
            try
            {
                RL.Logger?.LogInformationSource("Patching RRR exe");

                // Verify that the version is supported first
                GameVersion version = await DetermineGameVersionAsync();

                if (version == GameVersion.Unknown)
                {
                    IsExePatched = false;
                    CanUpdatePatchedBF = false;
                    CanDownloadPatchedBF = false;
                    return;
                }

                // Patch the file
                ExePatcher.PatchFile(!enablePatch);

                // Update patched state
                IsExePatched = enablePatch;
                CanUpdatePatchedBF = IsPatchedBFDownloaded && IsExePatched;

                // Display message
                if (enablePatch)
                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Mod_RRR_BFPatch_ApplyExePatchSuccess);
                else
                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Mod_RRR_BFPatch_RevertExePatchSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Patching RRR exe");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Mod_RRR_BFPatch_ApplyExePatchError);

                // Update the patched state
                IsExePatched = CheckIsExePatched();
            }

            CanDownloadPatchedBF = IsExePatched;
            IsPatchedBFDownloaded = CheckIsPatchedBFDownloaded();
            CanUpdatePatchedBF = IsPatchedBFDownloaded && IsExePatched;
            RefreshBFPatches();
        }

        public async Task DownloadPatchedBFAsync()
        {
            RL.Logger?.LogInformationSource("Downloading patched RRR BF");

            // Determine the game version
            GameVersion version = await DetermineGameVersionAsync();

            if (version == GameVersion.Unknown)
                return;

            // Download the game
            bool downloaded = await RCPServices.App.DownloadAsync(new Uri[]
            {
                version switch
                {
                    GameVersion.Steam => new Uri(AppURLs.RRR_PatchedBF_Steam_URL),
                    GameVersion.GOG => new Uri(AppURLs.RRR_PatchedBF_GOG_URL),
                    _ => throw new Exception("Invalid game version")
                }
            }, true, GameDirectoryPath);

            if (!downloaded)
                return;

            IsPatchedBFDownloaded = true;
            CanUpdatePatchedBF = IsExePatched;
            RefreshBFPatches();

            // Apply default patches
            foreach (Mod_RRR_BFModToggleViewModel bfMod in BFModToggles)
                bfMod.IsToggled = bfMod.IsDefaultToggled;

            await UpdatePatchedBFAsync();
        }

        public async Task RemovePatchedBFAsync()
        {
            RL.Logger?.LogInformationSource("Removing patched RRR BF");

            try
            {
                PatchedBFFilePath.DeleteFile();

                IsPatchedBFDownloaded = false;
                CanUpdatePatchedBF = false;
                RefreshBFPatches();

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Mod_RRR_BFPatch_RemovePatchedBFSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Deleting patched BF file");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Mod_RRR_BFPatch_RemovePatchedBFError);
            }
        }

        public void RefreshBFPatches()
        {
            RL.Logger?.LogInformationSource("Refreshing RRR BF patches");

            if (!PatchedBFFilePath.FileExists)
            {
                foreach (Mod_RRR_BFModToggleViewModel bfMod in BFModToggles)
                    bfMod.IsToggled = false;

                return;
            }

            try
            {
                using Stream bfFileStream = File.Open(PatchedBFFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                var serializerSettings = new BinarySerializerSettings(Endian.Little, Encoding.GetEncoding(1252));
                JADE_BIG_BigFile bf = BinarySerializableHelpers.ReadFromStream<JADE_BIG_BigFile>(bfFileStream, serializerSettings, App.GetBinarySerializerLogger(name: PatchedBFFilePath.Name));

                foreach (Mod_RRR_BFModToggleViewModel bfMod in BFModToggles)
                {
                    var loggerName = $"{PatchedBFFilePath.Name} - Patch ({bfMod.Header.Value})";
                    var s = new BinaryDeserializer(serializerSettings, bfFileStream, App.GetBinarySerializerLogger(name: loggerName));

                    var patch = bfMod.Patches[0];

                    bfMod.SelectedPatch = patch.GetAppliedPatch(s, bf);
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Refreshing BF patches");

                foreach (Mod_RRR_BFModToggleViewModel bfMod in BFModToggles)
                    bfMod.IsToggled = false;
            }
        }

        public async Task UpdatePatchedBFAsync()
        {
            RL.Logger?.LogInformationSource("Updating patched RRR BF");

            try
            {
                using Stream bfFileStream = File.Open(PatchedBFFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

                var serializerSettings = new BinarySerializerSettings(Endian.Little, Encoding.GetEncoding(1252));
                JADE_BIG_BigFile bf = BinarySerializableHelpers.ReadFromStream<JADE_BIG_BigFile>(bfFileStream, serializerSettings, App.GetBinarySerializerLogger(name: PatchedBFFilePath.Name));

                foreach (Mod_RRR_BFModToggleViewModel bfMod in BFModToggles)
                {
                    var loggerName = $"{PatchedBFFilePath.Name} - Patch ({bfMod.Header.Value})";
                    var s = new BinarySerializer(serializerSettings, bfFileStream, App.GetBinarySerializerLogger(name: loggerName));

                    foreach (Mod_RRR_BigFilePatch patch in bfMod.Patches)
                    {
                        patch.Apply(
                            s: s,
                            bf: bf,
                            patchToApply: bfMod.SelectedPatch);
                    }
                }

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Mod_RRR_BFPatch_UpdatePatchedBFSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Updating patched BF");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Mod_RRR_BFPatch_UpdatePatchedBFError);
            }
        }

        public async Task LaunchWithPatchedBFAsync()
        {
            RL.Logger?.LogInformationSource("Launching RRR with patched BF");

            // Launch the game exe passing in the path to the patched BF
            (await RCPServices.File.LaunchFileAsync(ExeFilePath, arguments: PatchedBFFilePath.Name))?.Dispose();
        }

        public void Dispose()
        {
            MemoryModSections?.DisposeAll();
            BFModToggles?.DisposeAll();
        }

        #endregion

        #region Data Types

        public enum GameVersion
        {
            Unknown,
            Steam,
            GOG,
        }

        #endregion
    }
}