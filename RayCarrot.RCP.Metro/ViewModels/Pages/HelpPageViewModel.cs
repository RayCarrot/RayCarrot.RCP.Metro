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
                            HelpText = String.Format(Resources.Help_RCP_Debug, CommonPaths.LogFile),
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
                            HelpText = String.Empty,

                            // TODO: Localize
                            CheatCodeItems = new Rayman1CheatCodeItemViewModel[]
                            {
                                new Rayman1CheatCodeItemViewModel("99 lives", "Level", "trj8p", "raylives", "[tab]bertrand"),
                                new Rayman1CheatCodeItemViewModel("All fist power-ups", "Level", "2x2rmfmf", "goldfist", "[tab]antoine"),
                                new Rayman1CheatCodeItemViewModel("All normal power-ups", "Level", "en5gol2g", "power", "[tab]benoit"),
                                new Rayman1CheatCodeItemViewModel("Full health", "Level", "kom0ogdk", "raypoint", "[tab]christ"),
                                new Rayman1CheatCodeItemViewModel("10 added Tings", "Level", "86e40g91", "raywiz", "[tab]jojo"),
                                new Rayman1CheatCodeItemViewModel("Skip level section", "Level", "o8feh", "winmap", "[tab]cyril"),
                                new Rayman1CheatCodeItemViewModel("Display hidden message", "Level", String.Empty, String.Empty, "[tab]program"),
                                new Rayman1CheatCodeItemViewModel("Free movement", "Level", String.Empty, String.Empty, "[tab];overay[BACKSPACE]"),
                                new Rayman1CheatCodeItemViewModel("100MHz Refresh rate", "Map", String.Empty, "freq10", String.Empty),
                                new Rayman1CheatCodeItemViewModel("80MHz Refresh rate", "Map", String.Empty, "freq80", String.Empty),
                                new Rayman1CheatCodeItemViewModel("All normal powers", "Map", String.Empty, "power", "[TAB]benoit"),
                                new Rayman1CheatCodeItemViewModel("Lens effect", "Map", String.Empty, "lens", String.Empty),
                                new Rayman1CheatCodeItemViewModel("Unlock all levels", "Map", "4ctrepfj", "alworld", "[TAB]francois"),
                                new Rayman1CheatCodeItemViewModel("Enter Breakout minigame (requires Mr Dark's Dare to have been completed)", "Map", "b76b7081", "cbray", "[TAB]olivier"),
                                new Rayman1CheatCodeItemViewModel("Enter random stage from Breakout minigame", "Map", String.Empty, String.Empty, "[TAB]cbrayal[BACKSPACE]"),
                                new Rayman1CheatCodeItemViewModel("Stage selection", "Map", String.Empty, String.Empty, "[TAB]alevel[BACKSPACE]"),
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_RDHeader,
                            HelpText = Resources.Help_Cheats_RD

                            // TODO: Add cheat code list
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R2Header,
                            HelpText = Resources.Help_Cheats_R2

                            // TODO: Add cheat code list
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_RMAHeader,
                            HelpText = Resources.Help_Cheats_RMA

                            // TODO: Add cheat code list
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R3Header,
                            HelpText = Resources.Help_Cheats_R3

                            // TODO: Add cheat code list
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