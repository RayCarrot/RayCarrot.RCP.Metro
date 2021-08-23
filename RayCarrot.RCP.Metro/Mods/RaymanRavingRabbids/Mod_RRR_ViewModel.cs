using MahApps.Metro.IconPacks;
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
    public class Mod_RRR_ViewModel : BaseModViewModel, IDisposable
    {
        #region Constructor

        public Mod_RRR_ViewModel()
        {
            MemoryPatcher = new Mod_RRR_MemoryPatcher();

            // TODO-UPDATE: Localize
            MemoryModSections = new ObservableCollection<Mod_RRR_MemoryModsSectonViewModel>()
            {
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => "Main"), null, 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Enable prototype features"), 
                            description: new LocalizedString(() => "Rayman's prototype behavior is enabled in all levels"),
                            toggleAction: x =>
                            {
                                MemoryPatcher.enableProtoRaymanEverywhere = x;
                                MemoryModSections?.Skip(1).ForEach(section => section.IsEnabled = x);
                            },
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Add camera controls"), 
                            description: new LocalizedString(() => "Camera controls are mapped to the right stick"),
                            toggleAction: x => MemoryPatcher.addCameraControls = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Add Duel, the lost minigame"), 
                            description: new LocalizedString(() => "\"Bunnies are shooting all over the place\" replaces \"Bunnies don't milk cows part 2\". Play it in multiplayer!"),
                            toggleAction: x => MemoryPatcher.addDuel = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Enable playtest menu"), 
                            description: new LocalizedString(() => "Not recommended. The playtest menu replaces the game type selection, but you cannot pause or exit loaded levels."),
                            toggleAction: x => MemoryPatcher.addPlaytestMenu = x,
                            isToggled: false),
                    }),
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => "Rayman"), new LocalizedString(() => "A gamepad is required to control Rayman. For an Xbox 360 controller, the controls are mapped as follows:\r\n    A: Jump / Hold in the air to use helicopter\r\n    X: Attack\r\n    Y: Use grappling hook\r\n    LB: Toggle light / Hold to control dance tempo if enabled\r\n    LT: Look mode if enabled\r\n    RB: Attack / Press or hold to use finishers if enabled\r\n    RT: Dodge roll / Ground pound / Roll and jump multiple times to boost if enabled\r\n    D-Pad Up: Toggle dance mode. In dance mode, press ABXY or use the left analog stick to perform dance moves\r\n    Left analog stick: Move Rayman\r\n    Right analog stick: Camera controls if enabled\r\n    Back: Noclip mode\r\n    Start: Pause"), 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Add look mode"),
                            description: new LocalizedString(() => "A first-person look mode is added and mapped to LT"),
                            toggleAction: x => MemoryPatcher.addLookMode = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Add boost button"),
                            description: new LocalizedString(() => "Normally reserved to an item not present in the game, pressing RT will make Rayman run really fast and enable speed attacks"),
                            toggleAction: x => MemoryPatcher.addBoostButton = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Add finishers"),
                            description: new LocalizedString(() => "Hold or press RB to add finishers into your combos"),
                            toggleAction: x => MemoryPatcher.addFinishers = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Climb walls"),
                            description: new LocalizedString(() => "Allows Rayman to grab hold of any wall by using his hook"),
                            toggleAction: x => MemoryPatcher.climbWalls = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Hang from grab spots"),
                            description: new LocalizedString(() => "Breaks hook behavior but enables a broken system for using the hook as a rope to hang and jump from"),
                            toggleAction: x => MemoryPatcher.hangFromHotspots = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Change hook graphics"),
                            description: new LocalizedString(() => "Enables even earlier looks for Rayman's hook"),
                            toggleAction: x => MemoryPatcher.setGrappinGFX = x,
                            isToggled: false,
                            selectionOptions: new ObservableCollection<LocalizedString>()
                            {
                                new LocalizedString(() => "Line"),
                                new LocalizedString(() => "Lightning"),
                                new LocalizedString(() => "Sparks"),
                                new LocalizedString(() => "Rope"),
                                new LocalizedString(() => "None"),
                            },
                            selectionAction: x => MemoryPatcher.GrappinGFX = (byte)x),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Upgraded powers"),
                            description: new LocalizedString(() => "Allows Rayman to use the helicopter for a longer time and, if Climb walls is enabled, hold onto walls infinitely"),
                            toggleAction: x => MemoryPatcher.allpowers = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Immortal"),
                            description: new LocalizedString(() => "A developer cheat - Rayman does not die"),
                            toggleAction: x => MemoryPatcher.immortal = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Dance: control tempo"),
                            description: new LocalizedString(() => "In dance mode, holding LB will start a beat for Rayman to dance to"),
                            toggleAction: x => MemoryPatcher.controlTempo = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Dance: groovy moveset"),
                            description: new LocalizedString(() => "Rayman uses different dance moves"),
                            toggleAction: x => MemoryPatcher.groovyRaymanDanceMoveset = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Use selected disguise"),
                            description: new LocalizedString(() => "Rayman's costume will always be based on the player's costume selection in the menu"),
                            toggleAction: x => MemoryPatcher.setPlayer1Costume = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Disable minigame intros"),
                            description: new LocalizedString(() => "Minigame intro cinematics change the state of the world and sometimes deactivate Rayman (e.g. Bunnies don't give gifts)"),
                            toggleAction: x => MemoryPatcher.disableMinigameIntro = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Disable footstep sound"),
                            description: new LocalizedString(() => "If BF mods aren't activated, Rayman's footstep sound in most areas can be annoying - use this option to turn it off"),
                            toggleAction: x => MemoryPatcher.disableFootstepSound = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Make Rayman less slippery"),
                            description: new LocalizedString(() => "Rayman feels slippery by default, this slightly improves that"),
                            toggleAction: x => MemoryPatcher.lowerSlippery = x,
                            isToggled: false),
                    }),
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => "Rabbids"), null, 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Rabbids drop items"),
                            description: new LocalizedString(() => "Rabbids drop their items for Rayman to use"),
                            toggleAction: x => MemoryPatcher.rabbidsDropItems = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Increased HP"),
                            description: new LocalizedString(() => "Each Rabbid has 100 HP"),
                            toggleAction: x => MemoryPatcher.rabbidsIncreasedHP = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Random prototype powers"),
                            description: new LocalizedString(() => "Some Rabbids are given one of 4 prototype powers. Discover them while playing!"),
                            toggleAction: x => MemoryPatcher.randomProtoRabbidPowers = x,
                            isToggled: false),
                    }),
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => "Mounts"), null, 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Tame mounts"),
                            description: new LocalizedString(() => "Mounts become controllable when you first ride them"),
                            toggleAction: x => MemoryPatcher.tameMounts = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Bats act like eagles"),
                            description: new LocalizedString(() => "Bats become huge and act like the eagle with different animations"),
                            toggleAction: x => MemoryPatcher.makeBatsIntoEagles = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Pigs act like plums"),
                            description: new LocalizedString(() => "Pigs use plum behavior and become very bouncy. Try throwing them onto the rabbids' heads!"),
                            toggleAction: x => MemoryPatcher.makePigsIntoPlums = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Rhinos become aggressive"),
                            description: new LocalizedString(() => "The rhinos from the races will attack you. Be prepared to die (or use the immortal cheat)!"),
                            toggleAction: x => MemoryPatcher.makeRhinosAggressive = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Spider robots can jump"),
                            description: new LocalizedString(() => "When riding the spider-like robots, you can jump like spiders could in the prototype"),
                            toggleAction: x => MemoryPatcher.makeSpidersJump = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Bats can shoot"),
                            description: new LocalizedString(() => "Bats can shoot projectiles with the attack button"),
                            toggleAction: x => MemoryPatcher.makeBatShoot = x,
                            isToggled: true),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Saucers start off flying"),
                            description: new LocalizedString(() => "Saucers can be found flying around, but this makes them harder to mount"),
                            toggleAction: x => MemoryPatcher.saucersStartFlying = x,
                            isToggled: false),
                    }),
                new Mod_RRR_MemoryModsSectonViewModel(new LocalizedString(() => "Bunny hunts"), null, 
                    new ObservableCollection<Mod_RRR_MemoryModToggleViewModel>()
                    {
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Missile launchers target Rayman"),
                            description: new LocalizedString(() => "Missiles will target Rayman, but 1 hit from them will kill Rayman!"),
                            toggleAction: x => MemoryPatcher.missileLaunchersTargetPlayer = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Don't destroy bipods"),
                            description: new LocalizedString(() => "Most bipods collapse by default when not in FPS mode. This prevents them from being destroyed."),
                            toggleAction: x => MemoryPatcher.dontDestroyBipods = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Activate all activator triggers"),
                            description: new LocalizedString(() => "Forces all activator triggers to be true constantly. Causes chaos in many levels and spawns many more enemies (and Globox babies)"),
                            toggleAction: x => MemoryPatcher.activateAllActivatorTriggers = x,
                            isToggled: false),
                        new Mod_RRR_MemoryModToggleViewModel(
                            header: new LocalizedString(() => "Activate all bounding volume triggers"),
                            description: new LocalizedString(() => "Some rabbid spawn triggers are difficult to reach. This triggers them, but beware, it also causes crashes in many levels!"),
                            toggleAction: x => MemoryPatcher.activateAllPivotInBVTriggers = x,
                            isToggled: false),
                    }),
            };

            // TODO-UPDATE: Localize
            BFModToggles = new ObservableCollection<Mod_RRR_BFModToggleViewModel>()
            {
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Fix sound effects"),
                    patches: Mod_RRR_BigFilePatch.FixSoundEffects,
                    isDefaultToggled: true),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Change playable character"),
                    patch: Mod_RRR_BigFilePatch.PlayableCharacters,
                    isDefaultToggled: false,
                    selectionOptions: new ObservableCollection<LocalizedString>()
                    {
                        new LocalizedString(() => "Baby Globox"),
                        new LocalizedString(() => "Serguei"),
                        new LocalizedString(() => "Rabbid"),
                        new LocalizedString(() => "Grey Leader Rabbid"),
                        new LocalizedString(() => "Superman Rabbid"),
                        new LocalizedString(() => "Terminator Rabbid (Pink)"),
                        new LocalizedString(() => "Sam Fisher Rabbid"),
                        new LocalizedString(() => "Nurgle Demon"),
                    }),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Make bat sound like eagle"),
                    patch: Mod_RRR_BigFilePatch.MakeBatSoundLikeEagle,
                    isDefaultToggled: false),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Make spider robot sound like spider"),
                    patches: Mod_RRR_BigFilePatch.MakeSpiderRobotsSoundLikeSpider,
                    isDefaultToggled: false),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Add custom helicopter texture"),
                    patch: Mod_RRR_BigFilePatch.AddCustomHelicopterTexture,
                    isDefaultToggled: true),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Add fur to rabbids"),
                    patches: Mod_RRR_BigFilePatch.AddFurToRabbids,
                    isDefaultToggled: false),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Enable flashlight (toggle with LB)"),
                    patch: Mod_RRR_BigFilePatch.EnableFlashlight,
                    isDefaultToggled: true),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Add flashlight to Bunnies Love Digging Tunnels"),
                    patches: Mod_RRR_BigFilePatch.AddFlashlightToMines,
                    isDefaultToggled: false),
                new Mod_RRR_BFModToggleViewModel(
                    header: new LocalizedString(() => "Add unused rabbid items to Bunnies Aren't Scared Of The Dark"),
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

        public override PackIconMaterialKind Icon => PackIconMaterialKind.GamepadVariantOutline;
        public override LocalizedString Header => new LocalizedString(() => "Rayman Raving Rabbids"); // TODO-UPDATE: Localize
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
            GameDirectoryPath = Games.RaymanRavingRabbids.GetInstallDir(false);
            return Task.CompletedTask;
        }

        public async Task ApplyMemoryPatchAsync()
        {
            RL.Logger?.LogInformationSource("Applying RRR memory patch");

            try
            {
                MemoryPatcher.Patch();

                RL.Logger?.LogInformationSource("RRR memory patch applied");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync("Memory patch applied successfully");
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying RRR memory patch");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "The memory patch could not be applied");
            }
        }

        public async Task<GameVersion> DetermineGameVersionAsync()
        {
            FileSystemPath exe = ExeFilePath;

            if (!exe.FileExists)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync("The game install directory is invalid", MessageType.Error);
                return GameVersion.Unknown;
            }

            long fileSize;

            try
            {
                fileSize = (long)exe.GetSize().Bytes;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting RRR exe file size");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "Unable to determine game version");

                return GameVersion.Unknown;
            }

            GameVersion version = ExePatches.FirstOrDefault(x => x.Value.FileSize == fileSize).Key;

            if (version == GameVersion.Unknown)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync("The game version is not supported", MessageType.Error);
            }

            return version;
        }

        public bool CheckIsExePatched()
        {
            RL.Logger?.LogInformationSource("Checking if RRR exe is patched");

            bool? isOriginal = ExePatcher.GetIsOriginal();

            if (isOriginal == true)
                RL.Logger?.LogInformationSource("RRR exe is patched");
            else if (isOriginal == false)
                RL.Logger?.LogInformationSource("RRR exe is not patched");
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
                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The executable was successfully patched");
                else
                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The executable patch was successfully reverted");
            }
            catch (Exception ex)
            {
                ex.HandleError("Patching RRR exe");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when patching the executable");

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
            // Determine the game version
            GameVersion version = await DetermineGameVersionAsync();

            if (version == GameVersion.Unknown)
                return;

            // Download the game
            bool downloaded = await RCPServices.App.DownloadAsync(new Uri[]
            {
                version switch
                {
                    GameVersion.Steam => new Uri(CommonUrls.RRR_PatchedBF_Steam_URL),
                    GameVersion.GOG => new Uri(CommonUrls.RRR_PatchedBF_GOG_URL),
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
            try
            {
                PatchedBFFilePath.DeleteFile();

                IsPatchedBFDownloaded = false;
                CanUpdatePatchedBF = false;
                RefreshBFPatches();

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The patched BF file has been removed");
            }
            catch (Exception ex)
            {
                ex.HandleError("Deleting patched BF file");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "Unable to remove patched BF file");
            }
        }

        public void RefreshBFPatches()
        {
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

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The patched BF file was successfully updated");
            }
            catch (Exception ex)
            {
                ex.HandleError("Updating patched BF");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when updating the patched BF file");
            }
        }

        public async Task LaunchWithPatchedBFAsync()
        {
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