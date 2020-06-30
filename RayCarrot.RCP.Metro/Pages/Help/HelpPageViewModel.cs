using RayCarrot.Logging;
using RayCarrot.WPF;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

            Services.Data.CultureChanged += (s, e) => Refresh();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the help items
        /// </summary>
        public void Refresh()
        {
            var time = new Stopwatch();

            time.Start();

            RL.Logger?.LogInformationSource("The help items are refreshing...");

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
                            HelpText = String.Format(Resources.Help_RCP_AppData, CommonPaths.UserDataBaseDir, CommonPaths.RegistryBaseKey),
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
                            HelpText = Resources.Cheats_R1,
                            CheatCodeItems = new BaseCheatCodeItemViewModel[]
                            {
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_99LivesTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_99LivesInput1, Resources.Cheats_R1_99LivesInput2, Resources.Cheats_R1_99LivesInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_FistPowerTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_FistPowerInput1, Resources.Cheats_R1_FistPowerInput2, Resources.Cheats_R1_FistPowerInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_AllPowerUpsTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_AllPowerUpsInput1, Resources.Cheats_R1_AllPowerUpsInput2, Resources.Cheats_R1_AllPowerUpsInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_FullHealthTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_FullHealthInput1, Resources.Cheats_R1_FullHealthInput2, Resources.Cheats_R1_FullHealthInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_10TingsTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_10TingsInput1, Resources.Cheats_R1_10TingsInput2, Resources.Cheats_R1_10TingsInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_SkipLevelTitle, Resources.Cheats_Location_Level, Resources.Cheats_R1_SkipLevelInput1, Resources.Cheats_R1_SkipLevelInput2, Resources.Cheats_R1_SkipLevelInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_HiddenMessageTitle, Resources.Cheats_Location_Level, String.Empty, String.Empty, Resources.Cheats_R1_HiddenMessageInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_FreeMovementTitle, Resources.Cheats_Location_Level, String.Empty, String.Empty, Resources.Cheats_R1_FreeMovementInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_100RefreshRateTitle, Resources.Cheats_Location_Map, String.Empty, Resources.Cheats_R1_100RefreshRateInput2, String.Empty),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_80RefreshRateTitle, Resources.Cheats_Location_Map, String.Empty, Resources.Cheats_R1_80RefreshRateInput2, String.Empty),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_LensEffectTitle, Resources.Cheats_Location_Map, String.Empty, Resources.Cheats_R1_LensEffectInput2, String.Empty),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_AllLevelsTitle, Resources.Cheats_Location_Map, Resources.Cheats_R1_AllLevelsInput1, Resources.Cheats_R1_AllLevelsInput2, Resources.Cheats_R1_AllLevelsInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_BreakoutTitle, Resources.Cheats_Location_Map, Resources.Cheats_R1_BreakoutInput1, Resources.Cheats_R1_BreakoutInput2, Resources.Cheats_R1_BreakoutInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_BreakoutStageTitle, Resources.Cheats_Location_Map, String.Empty, String.Empty, Resources.Cheats_R1_BreakoutStageInput3),
                                new Rayman1CheatCodeItemViewModel(Resources.Cheats_R1_SelectStageTitle, Resources.Cheats_Location_Map, String.Empty, String.Empty, Resources.Cheats_R1_SelectStageInput3),
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_RDHeader,
                            HelpText = String.Empty,
                            CheatCodeItems = new BaseCheatCodeItemViewModel[]
                            {
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_5LivesTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_5LivesInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_20LivesTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_20LivesInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_50LivesTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_50LivesInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_FistPowerTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_FistPowerInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_FinishLevelTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_FinishLevelInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_FullHealthTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_FullHealthInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_MapIndexTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_MapIndexInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_FreeMovementTitle, Resources.Cheats_Location_Level, Resources.Cheats_RD_FreeMovementInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RD_AllLevelsTitle, Resources.Cheats_Location_Map, Resources.Cheats_RD_AllLevelsInput),
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R2Header,
                            HelpText = String.Empty,
                            CheatCodeItems = new BaseCheatCodeItemViewModel[]
                            {
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_FistUpgradeTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_FistUpgradeInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_RestoreHealthTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_RestoreHealthInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_5LumsTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_5LumsInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_LevelSelectionTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_LevelSelectionInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_GrappleTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_GrappleInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_MaxHealthTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_MaxHealthInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_BonusLevelTitle, Resources.Cheats_Location_AccessDenied, Resources.Cheats_R2_BonusLevelInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_SkipMoviesTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_SkipMoviesInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_LoadingScreensTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_LoadingScreensInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_NoMurfyTitle, Resources.Cheats_Location_Pause, Resources.Cheats_R2_NoMurfyInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_TotASecretTitle, Resources.Cheats_Location_TotA3, Resources.Cheats_R2_TotASecretInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_R2_MenezisTitle, Resources.Cheats_Location_Credits, Resources.Cheats_R2_MenezisInput),
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_RMAHeader,
                            HelpText = Resources.Cheats_RM,
                            CheatCodeItems = new BaseCheatCodeItemViewModel[]
                            {
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM3_SkipMovieTitle, Resources.Cheats_Location_Cutscene, Resources.Cheats_RM3_SkipMovieInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM3_ReverseMapTitle, Resources.Cheats_Location_Level, Resources.Cheats_RM3_ReverseMapInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_UnlockBattlesTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_UnlockBattlesInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_UnlockRacesTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_UnlockRacesInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_UnlockAllTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_UnlockAllInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_AllCharactersTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_AllCharactersInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_AllSkinsTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_AllSkinsInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_Battle1Title, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_Battle1Input),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_Race1Title, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_Race1Input),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_Levels1Title, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_Levels1Input),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM_RagtimeMusicTitle, Resources.Cheats_Location_ProfileName, Resources.Cheats_RM_RagtimeMusicInput),
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = Resources.Help_Cheats_R3Header,
                            HelpText = String.Empty,
                            CheatCodeItems = new BaseCheatCodeItemViewModel[]
                            {
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM3_SkipMovieTitle, Resources.Cheats_Location_Cutscene, Resources.Cheats_RM3_SkipMovieInput),
                                new GenericCheatCodeItemViewModel(Resources.Cheats_RM3_ReverseMapTitle, Resources.Cheats_Location_Level, Resources.Cheats_RM3_ReverseMapInput),
                            }
                        },
                    }
                }
            };

            time.Stop();

            RL.Logger?.LogInformationSource("The help items have refreshed");
            RL.Logger?.LogDebugSource($"The help items refresh time was {time.ElapsedMilliseconds} ms");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The help items
        /// </summary>
        public ObservableCollection<HelpItemViewModel> HelpItems { get; set; }

        #endregion
    }
}