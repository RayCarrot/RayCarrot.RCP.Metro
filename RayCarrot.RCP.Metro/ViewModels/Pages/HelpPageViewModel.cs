using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;
// ReSharper disable StringLiteralTypo

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the help page
    /// </summary>
    public class HelpPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public HelpPageViewModel()
        {
            Refresh();

            RCF.Data.CultureChanged += (s, e) => Refresh();
            
            OpenDiscordCommand = new RelayCommand(OpenDiscord);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the help items
        /// </summary>
        //public void Refresh()
        //{
        //    HelpItems = new ObservableCollection<HelpItemViewModel>()
        //    {
        //        // Rayman Control Panel
        //        new HelpItemViewModel()
        //        {
        //            DisplayHeader = Resources.Help_RCP,
        //            SubItems = new ObservableCollection<HelpItemViewModel>()
        //            {
        //                // Updates
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_RCP_UpdatesHeader,
        //                    HelpText = String.Format(Resources.Help_RCP_Updates, "http://raycarrot.ylemnova.com/")
        //                },

        //                // Compatibility
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_RCP_CompatibilityHeader,
        //                    HelpText = Resources.Help_RCP_Compatibility
        //                },

        //                // Game Installer
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_RCP_GameInstallerHeader,
        //                    HelpText = Resources.Help_RCP_GameInstaller
        //                },

        //                // Backup Games
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_RCP_BackupRestoreHeader,
        //                    HelpText = String.Format(Resources.Help_RCP_BackupRestore, AppViewModel.BackupFamily)
        //                },

        //                // App Data Location
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_RCP_AppDataHeader,
        //                    HelpText = String.Format(Resources.Help_RCP_AppData, CommonPaths.UserDataBaseDir, CommonPaths.RegistryBaseKey, RCFRegistryPaths.RCFBasePath, CommonPaths.TempPath),
        //                    RequiredUserLevel = UserLevel.Advanced
        //                },

        //                // Launch arguments
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_RCP_LaunchArgsHeader,
        //                    HelpText = Resources.Help_RCP_LaunchArgs,
        //                    RequiredUserLevel = UserLevel.Technical
        //                },

        //                // Launch arguments
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_RCP_DebugHeader,
        //                    HelpText = String.Format(Resources.Help_RCP_Debug, CommonPaths.LogFile),
        //                    RequiredUserLevel = UserLevel.Debug
        //                },
        //            }
        //        },

        //        // Games
        //        new HelpItemViewModel()
        //        {
        //            DisplayHeader = Resources.Help_Games,
        //            SubItems = new ObservableCollection<HelpItemViewModel>()
        //            {
        //                // General
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Games_General,
        //                    SubItems = new ObservableCollection<HelpItemViewModel>()
        //                    {
        //                        // Game not Launching
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_General_GameNotLaunchingHeader,
        //                            HelpText = Resources.Help_Games_General_GameNotLaunching
        //                        },
        //                    }
        //                },

        //                // Rayman 1
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Games_R1,
        //                    SubItems = new ObservableCollection<HelpItemViewModel>()
        //                    {
        //                        // Dos Emulator
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_R1_EmulatorHeader,
        //                            HelpText = Resources.Help_Games_R1_Emulator
        //                        },

        //                        // Installation failed
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_R1_InstallationFailedHeader,
        //                            HelpText = Resources.Help_Games_R1_InstallationFailed
        //                        },

        //                        // Rayman Designer Editor
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_R1_MapperHeader,
        //                            HelpText = Resources.Help_Games_R1_Mapper
        //                        },

        //                        // Importing Maps
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_R1_ImportMapsHeader,
        //                            HelpText = Resources.Help_Games_R1_ImportMaps
        //                        },
        //                    }
        //                },

        //                // Rayman 2
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Games_R2,
        //                    SubItems = new ObservableCollection<HelpItemViewModel>()
        //                    {
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_R2_GameSpeedHeader,
        //                            HelpText = Resources.Help_Games_R2_GameSpeed
        //                        },
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_R2_FpsHeader,
        //                            HelpText = Resources.Help_Games_R2_Fps
        //                        },
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_R2_NoDiscHeader,
        //                            HelpText = Resources.Help_Games_R2_NoDisc
        //                        },
        //                    }
        //                },

        //                // Rayman M/Arena
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Games_RMA,
        //                    SubItems = new ObservableCollection<HelpItemViewModel>()
        //                    {
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_RMA_MissingTexturesHeader,
        //                            HelpText = Resources.Help_Games_RMA_MissingTextures
        //                        },
        //                    }
        //                },

        //                // Rayman 3
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Games_R3,
        //                    SubItems = new ObservableCollection<HelpItemViewModel>()
        //                    {
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_RMA_MissingTexturesHeader,
        //                            HelpText = Resources.Help_Games_RMA_MissingTextures
        //                        },
        //                    }
        //                },

        //                // Rayman Raving Rabbids
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Games_RRR,
        //                    SubItems = new ObservableCollection<HelpItemViewModel>()
        //                    {
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_RRR_EngineErrorHeader,
        //                            HelpText = Resources.Help_Games_RRR_EngineError
        //                        },
        //                    }
        //                },

        //                // Rayman Legends
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Games_RL,
        //                    SubItems = new ObservableCollection<HelpItemViewModel>()
        //                    {
        //                        new HelpItemViewModel()
        //                        {
        //                            DisplayHeader = Resources.Help_Games_RL_LoadErrorHeader,
        //                            HelpText = Resources.Help_Games_RL_LoadError
        //                        },
        //                    }
        //                },
        //            }
        //        },

        //        // Cheat Codes
        //        new HelpItemViewModel()
        //        {
        //            DisplayHeader = Resources.Help_Cheats,
        //            SubItems = new ObservableCollection<HelpItemViewModel>()
        //            {
        //                // TODO: Localize + add tooltip to column headers

        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Cheats_R1Header,
        //                    HelpText = "Different cheat codes will work in different versions. Some versions support multiple cheat code types.",
        //                    CheatCodeItems = new BaseCheatCodeItemViewModel[]
        //                    {
        //                        new Rayman1CheatCodeItemViewModel("99 lives", "Level", "trj8p", "raylives", "[TAB]bertrand"),
        //                        new Rayman1CheatCodeItemViewModel("All fist power-ups", "Level", "2x2rmfmf", "goldfist", "[TAB]antoine"),
        //                        new Rayman1CheatCodeItemViewModel("All normal power-ups", "Level", "en5gol2g", "power", "[TAB]benoit"),
        //                        new Rayman1CheatCodeItemViewModel("Full health", "Level", "kom0ogdk", "raypoint", "[TAB]christ"),
        //                        new Rayman1CheatCodeItemViewModel("10 added Tings", "Level", "86e40g91", "raywiz", "[TAB]jojo"),
        //                        new Rayman1CheatCodeItemViewModel("Skip level section", "Level", "o8feh", "winmap", "[TAB]cyril"),
        //                        new Rayman1CheatCodeItemViewModel("Display hidden message", "Level", String.Empty, String.Empty, "[TAB]program"),
        //                        new Rayman1CheatCodeItemViewModel("Free movement", "Level", String.Empty, String.Empty, "[TAB];overay[BACKSPACE]"),
        //                        new Rayman1CheatCodeItemViewModel("100MHz Refresh rate", "Map", String.Empty, "freq10", String.Empty),
        //                        new Rayman1CheatCodeItemViewModel("80MHz Refresh rate", "Map", String.Empty, "freq80", String.Empty),
        //                        new Rayman1CheatCodeItemViewModel("All normal powers", "Map", String.Empty, "power", "[TAB]benoit"),
        //                        new Rayman1CheatCodeItemViewModel("Lens effect", "Map", String.Empty, "lens", String.Empty),
        //                        new Rayman1CheatCodeItemViewModel("Unlock all levels", "Map", "4ctrepfj", "alworld", "[TAB]francois"),
        //                        new Rayman1CheatCodeItemViewModel("Enter Breakout minigame (requires Mr Dark's Dare to have been completed)", "Map", "b76b7081", "cbray", "[TAB]olivier"),
        //                        new Rayman1CheatCodeItemViewModel("Enter random stage from Breakout minigame", "Map", String.Empty, String.Empty, "[TAB]cbrayal[BACKSPACE]"),
        //                        new Rayman1CheatCodeItemViewModel("Stage selection", "Map", String.Empty, String.Empty, "[TAB]alevel[BACKSPACE]"),
        //                    }
        //                },
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Cheats_RDHeader,
        //                    HelpText = String.Empty,
        //                    CheatCodeItems = new BaseCheatCodeItemViewModel[]
        //                    {
        //                        new GenericCheatCodeItemViewModel("5 lives", "Level", "[TAB]lives05[BACKSPACE]"), 
        //                        new GenericCheatCodeItemViewModel("20 lives", "Level", "[TAB]lives20[BACKSPACE]"), 
        //                        new GenericCheatCodeItemViewModel("50 lives", "Level", "[TAB]lives50[BACKSPACE]"), 
        //                        new GenericCheatCodeItemViewModel("All fist power-ups", "Level", "[TAB]goldens"), 
        //                        new GenericCheatCodeItemViewModel("Finish level", "Level", "[TAB]finishing"), 
        //                        new GenericCheatCodeItemViewModel("Full health", "Level", "[TAB]points"), 
        //                        new GenericCheatCodeItemViewModel("Display map index", "Level", "[TAB]map[BACKSPACE]"), 
        //                        new GenericCheatCodeItemViewModel("Free movement", "Level", "[TAB]moveray[BACKSPACE]"), 
        //                        new GenericCheatCodeItemViewModel("Unlock all levels", "Map", "[TAB]openall[BACKSPACE]"), 
        //                    }
        //                },
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Cheats_R2Header,
        //                    HelpText = String.Empty,
        //                    CheatCodeItems = new BaseCheatCodeItemViewModel[]
        //                    {
        //                        new GenericCheatCodeItemViewModel("Upgrade magic fist", "Pause screen", "glowfist"),
        //                        new GenericCheatCodeItemViewModel("Restore health", "Pause screen", "gimmelife"),
        //                        new GenericCheatCodeItemViewModel("Gain 5 Yellow Lums", "Pause screen", "gimmelumz"),
        //                        new GenericCheatCodeItemViewModel("Go to any level", "Pause screen", "gothere"),
        //                        new GenericCheatCodeItemViewModel("Unlock grappling power", "Pause screen", "hangon"),
        //                        new GenericCheatCodeItemViewModel("Gain maximum health", "Pause screen", "press and release the J key to the rhythm of the Rayman 2 theme"),
        //                        new GenericCheatCodeItemViewModel("Enter bonus level without all Lums and Cages", "Access Denied screen", "A[NUMPAD 0]QWQW[ENTER]"),
        //                        new GenericCheatCodeItemViewModel("Skip cutscenes", "Pause screen", "NOMOVIES"),
        //                        new GenericCheatCodeItemViewModel("New loading screens", "Pause screen", "ALLVIGN"),
        //                        new GenericCheatCodeItemViewModel("Disable Murfy", "Pause screen", "NOMOREMURFY"),
        //                        new GenericCheatCodeItemViewModel("Access hidden area in Tomb of the Ancients", "Tomb of the Ancients part 3, while standing on the third panel away from the entrance to the Technical Check-up where Rayman fights Clark", "PLAYJEFF"),
        //                        new GenericCheatCodeItemViewModel("Access Menezis", "Credits", "SHOOTEMUP"),
        //                        new GenericCheatCodeItemViewModel("Unknown", "Pause screen", "GETELIX"),
        //                    }
        //                },
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Cheats_RMAHeader,
        //                    HelpText = $"Music team:{Environment.NewLine}" +
        //                               "To activate it, enter race mode and pause the game. On the pause screen, hold L1 + R1 (or equivalent on the currently used controller), then press the optimize button. Five blue balls will appear at the bottom of the screen. Release everything and press Jump. The player should hear a high pitched \"Ding\" sound, and after 0.5 seconds, the first ball should turn yellow. As soon as the ball turns yellow, hold Jump. The second ball should then turn yellow. As soon as the second ball turns yellow, release Jump. The third ball should then turn yellow. As soon as the third ball turns yellow, hold Jump again. The fourth ball should then turn yellow. As soon as the fourth ball turns yellow, release Jump. The player should be taken to the \"Music Mode\" screen where the player will be able to choose \"Music Team\".",
        //                    CheatCodeItems = new BaseCheatCodeItemViewModel[]
        //                    {
        //                        new GenericCheatCodeItemViewModel("Skip cutscenes", "Cutscene", "esc"),
        //                        new GenericCheatCodeItemViewModel("Unlock all battle levels", "Profile name", "Enter ALLFISH as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Unlock all race levels", "Profile name", "Enter ALLTRIBES as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Unlock all levels", "Profile name", "Enter ALLRAYMANM as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Unlock all characters", "Profile name", "Enter PUPPETS as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Unlock all skins", "Profile name", "Enter CARNIVAL as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Unlock all battle levels in mode 1", "Profile name", "Enter ARENAS as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Unlock all race levels in mode 1", "Profile name", "Enter TRACKS as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Unlock all levels in mode 1", "Profile name", "Enter FIELDS as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Ragtime music in races", "Profile name", "Enter OLDTV as a profile name and press Shift + Ctrl + Alt"),
        //                        new GenericCheatCodeItemViewModel("Reverse map", "Level", "reverse"),
        //                    }
        //                },
        //                new HelpItemViewModel()
        //                {
        //                    DisplayHeader = Resources.Help_Cheats_R3Header,
        //                    HelpText = String.Empty,
        //                    CheatCodeItems = new BaseCheatCodeItemViewModel[]
        //                    {
        //                        new GenericCheatCodeItemViewModel("Skip cutscenes", "Cutscene", "esc"),
        //                        new GenericCheatCodeItemViewModel("Reverse map", "Level", "reverse"),
        //                    }
        //                },
        //            }
        //        }
        //    };
        //}

        /// <summary>
        /// Refreshes the help items
        /// </summary>
        public void Refresh()
        {
            HelpItems = new ObservableCollection<HelpItemViewModel>()
            {
                // Rayman Control Panel
                new HelpItemViewModel()
                {
                    DisplayHeader = Resources.Help_RCP,
                    SubItems = new ObservableCollection<HelpItemViewModel>()
                    {
                        // Updates
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_UpdatesHeader,
                            HelpText = String.Format(Resources.Help_RCP_Updates, "http://raycarrot.ylemnova.com/")
                        },

                        // Compatibility
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_CompatibilityHeader,
                            HelpText = Resources.Help_RCP_Compatibility
                        },

                        // Game Installer
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_GameInstallerHeader,
                            HelpText = Resources.Help_RCP_GameInstaller
                        },

                        // Backup Games
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_BackupRestoreHeader,
                            HelpText = String.Format(Resources.Help_RCP_BackupRestore, AppViewModel.BackupFamily)
                        },

                        // App Data Location
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_AppDataHeader,
                            HelpText = String.Format(Resources.Help_RCP_AppData, CommonPaths.UserDataBaseDir, CommonPaths.RegistryBaseKey, RCFRegistryPaths.RCFBasePath, CommonPaths.TempPath),
                            RequiredUserLevel = UserLevel.Advanced
                        },

                        // Launch arguments
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_LaunchArgsHeader,
                            HelpText = Resources.Help_RCP_LaunchArgs,
                            RequiredUserLevel = UserLevel.Technical
                        },

                        // Launch arguments
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_DebugHeader,
                            HelpText = string.Format(Resources.Help_RCP_Debug, CommonPaths.LogFile),
                            RequiredUserLevel = UserLevel.Debug
                        },
                    }
                },

                // Games
                new HelpItemViewModel()
                {
                    DisplayHeader = Resources.Help_Games,
                    SubItems = new ObservableCollection<HelpItemViewModel>()
                    {
                        // General
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_General,
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                // Game not Launching
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_General_GameNotLaunchingHeader,
                                    HelpText = Resources.Help_Games_General_GameNotLaunching
                                },
                            }
                        },

                        // Rayman 1
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_R1,
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                // Dos Emulator
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R1_EmulatorHeader,
                                    HelpText = Resources.Help_Games_R1_Emulator
                                },

                                // Installation failed
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R1_InstallationFailedHeader,
                                    HelpText = Resources.Help_Games_R1_InstallationFailed
                                },

                                // Rayman Designer Editor
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R1_MapperHeader,
                                    HelpText = Resources.Help_Games_R1_Mapper
                                },

                                // Importing Maps
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R1_ImportMapsHeader,
                                    HelpText = Resources.Help_Games_R1_ImportMaps
                                },
                            }
                        },

                        // Rayman 2
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_R2,
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R2_GameSpeedHeader,
                                    HelpText = Resources.Help_Games_R2_GameSpeed
                                },
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R2_FpsHeader,
                                    HelpText = Resources.Help_Games_R2_Fps
                                },
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R2_NoDiscHeader,
                                    HelpText = Resources.Help_Games_R2_NoDisc
                                },
                            }
                        },

                        // Rayman M/Arena
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_RMA,
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_RMA_MissingTexturesHeader,
                                    HelpText = Resources.Help_Games_RMA_MissingTextures
                                },
                            }
                        },

                        // Rayman 3
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_R3,
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_RMA_MissingTexturesHeader,
                                    HelpText = Resources.Help_Games_RMA_MissingTextures
                                },
                            }
                        },

                        // Rayman Raving Rabbids
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_RRR,
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_RRR_EngineErrorHeader,
                                    HelpText = Resources.Help_Games_RRR_EngineError
                                },
                            }
                        },

                        // Rayman Legends
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_RL,
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_RL_LoadErrorHeader,
                                    HelpText = Resources.Help_Games_RL_LoadError
                                },
                            }
                        },
                    }
                },

                // Cheat Codes
                new HelpItemViewModel()
                {
                    DisplayHeader = Resources.Help_Cheats,
                    SubItems = new ObservableCollection<HelpItemViewModel>()
                    {
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R1Header,
                            HelpText = Resources.Help_Cheats_R1
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_RDHeader,
                            HelpText = Resources.Help_Cheats_RD
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R2Header,
                            HelpText = Resources.Help_Cheats_R2
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_RMAHeader,
                            HelpText = Resources.Help_Cheats_RMA
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R3Header,
                            HelpText = Resources.Help_Cheats_R3
                        },
                    }
                }
            };
        }
        /// <summary>
        /// Opens the Discord URL
        /// </summary>
        public void OpenDiscord()
        {
            try
            {
                Process.Start(CommonUrls.DiscordUrl)?.Dispose();
            }
            catch (Exception ex)
            {
                ex.HandleError($"Opening Discord URL");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The help items
        /// </summary>
        public ObservableCollection<HelpItemViewModel> HelpItems { get; set; }

        #endregion

        #region Commands

        public ICommand OpenDiscordCommand { get; }

        #endregion
    }
}