using NLog;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the help page
    /// </summary>
    public class Page_Help_ViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Page_Help_ViewModel()
        {
            Refresh();

            Services.InstanceData.CultureChanged += (s, e) => Refresh();
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the help items
        /// </summary>
        public void Refresh()
        {
            var time = new Stopwatch();

            time.Start();

            Logger.Info("The help items are refreshing...");

            HelpItems = new ObservableCollection<Page_Help_ItemViewModel>()
            {
                // Rayman Control Panel
                new Page_Help_ItemViewModel()
                {
                    DisplayHeader = Resources.Help_RCP,
                    SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                    {
                        // Updates
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_UpdatesHeader,
                            HelpText = String.Format(Resources.Help_RCP_Updates, "https://raym.app/rcp/")
                        },

                        // Compatibility
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_CompatibilityHeader,
                            HelpText = Resources.Help_RCP_Compatibility
                        },

                        // Game Installer
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_GameInstallerHeader,
                            HelpText = Resources.Help_RCP_GameInstaller
                        },

                        // Backup Games
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_BackupRestoreHeader,
                            HelpText = String.Format(Resources.Help_RCP_BackupRestore, AppViewModel.BackupFamily)
                        },

                        // App Data Location
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_AppDataHeader,
                            HelpText = String.Format(Resources.Help_RCP_AppData, AppFilePaths.UserDataBaseDir, AppFilePaths.RegistryBaseKey),
                            RequiredUserLevel = UserLevel.Advanced
                        },

                        // Launch arguments
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_LaunchArgsHeader,
                            HelpText = Resources.Help_RCP_LaunchArgs,
                            RequiredUserLevel = UserLevel.Technical
                        },

                        // Launch arguments
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_RCP_DebugHeader,
                            HelpText = String.Format(Resources.Help_RCP_Debug, AppFilePaths.LogFile),
                            RequiredUserLevel = UserLevel.Debug
                        },
                    }
                },

                // Games
                new Page_Help_ItemViewModel()
                {
                    DisplayHeader = Resources.Help_Games,
                    SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                    {
                        // General
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_General,
                            SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                            {
                                // Game not Launching
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_General_GameNotLaunchingHeader,
                                    HelpText = Resources.Help_Games_General_GameNotLaunching
                                },
                            }
                        },

                        // Rayman 1
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_R1,
                            SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                            {
                                // Dos Emulator
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R1_EmulatorHeader,
                                    HelpText = Resources.Help_Games_R1_Emulator
                                },

                                // Installation failed
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R1_InstallationFailedHeader,
                                    HelpText = Resources.Help_Games_R1_InstallationFailed
                                },

                                // Rayman Designer Editor
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R1_MapperHeader,
                                    HelpText = Resources.Help_Games_R1_Mapper
                                },

                                // Importing Maps
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R1_ImportMapsHeader,
                                    HelpText = Resources.Help_Games_R1_ImportMaps
                                },
                            }
                        },

                        // Rayman 2
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_R2,
                            SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                            {
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R2_GameSpeedHeader,
                                    HelpText = Resources.Help_Games_R2_GameSpeed
                                },
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R2_FpsHeader,
                                    HelpText = Resources.Help_Games_R2_Fps
                                },
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_R2_NoDiscHeader,
                                    HelpText = Resources.Help_Games_R2_NoDisc
                                },
                            }
                        },

                        // Rayman M/Arena
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_RMA,
                            SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                            {
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_RMA_MissingTexturesHeader,
                                    HelpText = Resources.Help_Games_RMA_MissingTextures
                                },
                            }
                        },

                        // Rayman 3
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_R3,
                            SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                            {
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_RMA_MissingTexturesHeader,
                                    HelpText = Resources.Help_Games_RMA_MissingTextures
                                },
                            }
                        },

                        // Rayman Raving Rabbids
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_RRR,
                            SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                            {
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_RRR_EngineErrorHeader,
                                    HelpText = Resources.Help_Games_RRR_EngineError
                                },
                            }
                        },

                        // Rayman Legends
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Games_RL,
                            SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                            {
                                new Page_Help_ItemViewModel()
                                {
                                    DisplayHeader = Resources.Help_Games_RL_LoadErrorHeader,
                                    HelpText = Resources.Help_Games_RL_LoadError
                                },
                            }
                        },
                    }
                },

                // Cheat Codes
                new Page_Help_ItemViewModel()
                {
                    DisplayHeader = Resources.Help_Cheats,
                    SubItems = new ObservableCollection<Page_Help_ItemViewModel>()
                    {
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R1Header,
                            HelpText = Resources.Cheats_R1,
                            CheatCodeItems = new Page_Help_BaseCheatCodeItemViewModel[]
                            {
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_99LivesTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_99LivesInput1, Resources.Cheats_R1_99LivesInput2, Resources.Cheats_R1_99LivesInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_FistPowerTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_FistPowerInput1, Resources.Cheats_R1_FistPowerInput2, Resources.Cheats_R1_FistPowerInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_AllPowerUpsTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_AllPowerUpsInput1, Resources.Cheats_R1_AllPowerUpsInput2, Resources.Cheats_R1_AllPowerUpsInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_FullHealthTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_FullHealthInput1, Resources.Cheats_R1_FullHealthInput2, Resources.Cheats_R1_FullHealthInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_10TingsTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_10TingsInput1, Resources.Cheats_R1_10TingsInput2, Resources.Cheats_R1_10TingsInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_SkipLevelTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_SkipLevelInput1, Resources.Cheats_R1_SkipLevelInput2, Resources.Cheats_R1_SkipLevelInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_HiddenMessageTitle, Resources.Cheats_Location_Level, String.Empty, String.Empty, Resources.Cheats_R1_HiddenMessageInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_FreeMovementTitle, Resources.Cheats_Location_Level, String.Empty, String.Empty, Resources.Cheats_R1_FreeMovementInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_100RefreshRateTitle, Resources.Cheats_Location_Map, String.Empty, Resources.Cheats_R1_100RefreshRateInput2, String.Empty),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_80RefreshRateTitle, Resources.Cheats_Location_Map, String.Empty, Resources.Cheats_R1_80RefreshRateInput2, String.Empty),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_LensEffectTitle, Resources.Cheats_Location_Map, String.Empty, Resources.Cheats_R1_LensEffectInput2, String.Empty),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_AllLevelsTitle, Resources.Cheats_Location_Map, Resources.Cheats_R1_AllLevelsInput1, Resources.Cheats_R1_AllLevelsInput2, Resources.Cheats_R1_AllLevelsInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_BreakoutTitle, Resources.Cheats_Location_Map, Resources.Cheats_R1_BreakoutInput1, Resources.Cheats_R1_BreakoutInput2, Resources.Cheats_R1_BreakoutInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_BreakoutStageTitle, Resources.Cheats_Location_Map, String.Empty, String.Empty, Resources.Cheats_R1_BreakoutStageInput3),
                                new Page_Help_Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_SelectStageTitle, Resources.Cheats_Location_Map, String.Empty, String.Empty, Resources.Cheats_R1_SelectStageInput3),
                            }
                        },
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_RDHeader,
                            HelpText = String.Empty,
                            CheatCodeItems = new Page_Help_BaseCheatCodeItemViewModel[]
                            {
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_5LivesTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_5LivesInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_20LivesTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_20LivesInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_50LivesTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_50LivesInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_FistPowerTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_FistPowerInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_FinishLevelTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_FinishLevelInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_FullHealthTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_FullHealthInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_MapIndexTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_MapIndexInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_FreeMovementTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_FreeMovementInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RD_AllLevelsTitle, Resources.Cheats_Location_Map, Resources.Cheats_RD_AllLevelsInput),
                            }
                        },
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R2Header,
                            HelpText = String.Empty,
                            CheatCodeItems = new Page_Help_BaseCheatCodeItemViewModel[]
                            {
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_FistUpgradeTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_FistUpgradeInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_RestoreHealthTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_RestoreHealthInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_5LumsTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_5LumsInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_LevelSelectionTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_LevelSelectionInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_GrappleTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_GrappleInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_MaxHealthTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_MaxHealthInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_BonusLevelTitle, Resources.Cheats_Location_AccessDenied, Resources.Cheats_R2_BonusLevelInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_SkipMoviesTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_SkipMoviesInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_LoadingScreensTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_LoadingScreensInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_NoMurfyTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_NoMurfyInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_TotASecretTitle, Resources.Cheats_Location_TotA3, Resources.Cheats_R2_TotASecretInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_R2_MenezisTitle, Resources.Cheats_Location_Credits, Resources.Cheats_R2_MenezisInput),
                            }
                        },
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_RMAHeader,
                            HelpText = Resources.Cheats_RM,
                            CheatCodeItems = new Page_Help_BaseCheatCodeItemViewModel[]
                            {
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM3_SkipMovieTitle, Resources.Cheats_Location_Cutscene, Resources.Cheats_RM3_SkipMovieInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM3_ReverseMapTitle, Resources.Cheats_Location_Level, Resources.Cheats_RM3_ReverseMapInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_UnlockBattlesTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_UnlockBattlesInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_UnlockRacesTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_UnlockRacesInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_UnlockAllTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_UnlockAllInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_AllCharactersTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_AllCharactersInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_AllSkinsTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_AllSkinsInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_Battle1Title, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_Battle1Input),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_Race1Title, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_Race1Input),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_Levels1Title, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_Levels1Input),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM_RagtimeMusicTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_RagtimeMusicInput),
                            }
                        },
                        new Page_Help_ItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R3Header,
                            HelpText = String.Empty,
                            CheatCodeItems = new Page_Help_BaseCheatCodeItemViewModel[]
                            {
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM3_SkipMovieTitle, Resources.Cheats_Location_Cutscene, Resources.Cheats_RM3_SkipMovieInput),
                                new Page_Help_GenericCheatCodeItemViewModel(Resources.Cheats_RM3_ReverseMapTitle, Resources.Cheats_Location_Level, Resources.Cheats_RM3_ReverseMapInput),
                            }
                        },
                    }
                }
            };

            time.Stop();

            Logger.Info("The help items have refreshed");
            Logger.Debug("The help items refresh time was {0} ms", time.ElapsedMilliseconds);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The help items
        /// </summary>
        public ObservableCollection<Page_Help_ItemViewModel> HelpItems { get; set; }

        #endregion
    }
}