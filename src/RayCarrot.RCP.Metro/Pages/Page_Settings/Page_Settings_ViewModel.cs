using Microsoft.Win32;
using Nito.AsyncEx;
using RayCarrot.IO;
using NLog;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the settings page
    /// </summary>
    public class Page_Settings_ViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Page_Settings_ViewModel()
        {
            // Create commands
            ContributeLocalizationCommand = new RelayCommand(ContributeLocalization);
            EditJumpListCommand = new AsyncRelayCommand(EditJumpListAsync);
            RefreshCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await RefreshAsync(true, true)));
            ResetCommand = new AsyncRelayCommand(ResetAsync);

            // Create properties
            AsyncLock = new AsyncLock();
            LocalLinkItems = new LinkItemCollection();
            AssociatedPrograms = new ObservableCollection<AssociatedProgramEntryViewModel>();

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(LocalLinkItems, this);

            // Refresh on startup
            Metro.App.Current.StartupComplete += async (s, e) => await RefreshAsync(true, true);

            Services.InstanceData.CultureChanged += async (s, e) => await Task.Run(async () => await RefreshAsync(false, false));
            Services.InstanceData.UserLevelChanged += async (s, e) => await Task.Run(async () => await RefreshAsync(false, false));
            Data.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Data.Archive_AssociatedPrograms))
                    RefreshAssociatedPrograms();
            };
            AssociatedPrograms.CollectionChanged += (s, e) =>
            {
                // For now you can only remove items from the UI
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (AssociatedProgramEntryViewModel item in e.OldItems)
                        Data.Archive_AssociatedPrograms.Remove(item.FileExtension);
                
                    // Make sure the check for if the collection is empty or not updates
                    OnPropertyChanged(nameof(AssociatedPrograms));
                }
            };
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Commands

        public ICommand ContributeLocalizationCommand { get; }

        public ICommand EditJumpListCommand { get; }

        public ICommand RefreshCommand { get; }

        public ICommand ResetCommand { get; }

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock for refreshing the links
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current culture info
        /// </summary>
        public CultureInfo CurrentCultureInfo
        {
            get => new CultureInfo(Data.App_CurrentCulture);
            set
            {
                if (value == null)
                    return;

                Data.App_CurrentCulture = value.Name;
            }
        }

        public bool ShowIncompleteTranslations
        {
            get => Data.App_ShowIncompleteTranslations;
            set
            {
                Data.App_ShowIncompleteTranslations = value;
                RefreshLanguages();
            }
        }

        /// <summary>
        /// The local link items
        /// </summary>
        public LinkItemCollection LocalLinkItems { get; }

        public ObservableCollection<AssociatedProgramEntryViewModel> AssociatedPrograms { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the URL for contributing to localizing the program
        /// </summary>
        public void ContributeLocalization()
        {
            App.OpenUrl(AppURLs.TranslationUrl);
        }

        /// <summary>
        /// Edits the jump list items
        /// </summary>
        /// <returns>The task</returns>
        public async Task EditJumpListAsync()
        {
            // Get the result
            var result = await Services.UI.EditJumpListAsync(new JumpListEditViewModel());

            if (result.CanceledByUser)
                return;

            // Update the jump list items collection
            Data.App_JumpListItemIDCollection = result.IncludedItems.Select(x => x.ID).ToList();

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.JumpList));
        }

        /// <summary>
        /// Refreshes the page
        /// </summary>
        /// <param name="refreshLocalization">Indicates if the localization should be refreshed</param>
        /// <param name="refreshAssociatedPrograms">Indicates if the associated programs should be refreshed</param>
        /// <returns>The task</returns>
        public async Task RefreshAsync(bool refreshLocalization, bool refreshAssociatedPrograms)
        {
            using (await AsyncLock.LockAsync())
            {
                // Refresh the languages
                if (refreshLocalization)
                    Application.Current.Dispatcher.Invoke(RefreshLanguages);

                // Refresh associated programs
                if (refreshAssociatedPrograms)
                    RefreshAssociatedPrograms();

                try
                {
                    Stopwatch time = new Stopwatch();

                    time.Start();

                    Logger.Info("The links are refreshing...");

                    LocalLinkItems.Clear();

                    // Config files
                    LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                    {
                        new Page_Settings_LinkItemViewModel(AppFilePaths.UbiIniPath1, Resources.Links_Local_PrimaryUbiIni),
                        new Page_Settings_LinkItemViewModel(AppFilePaths.UbiIniPath2, Resources.Links_Local_SecondaryUbiIni,
                            UserLevel.Advanced),
                        new Page_Settings_LinkItemViewModel(Games.Rayman2.IsAdded()
                            ? Games.Rayman2.GetInstallDir() + "ubi.ini"
                            : FileSystemPath.EmptyPath, Resources.Links_Local_R2UbiIni, UserLevel.Advanced),
                        new Page_Settings_LinkItemViewModel(Environment.SpecialFolder.CommonApplicationData.GetFolderPath() + @"Ubisoft\RGH Launcher\1.0.0.0\Launcher_5.exe.config", Resources.Links_Local_RGHConfig, UserLevel.Advanced)
                    });

                    // DOSBox files
                    LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                    {
                        new Page_Settings_LinkItemViewModel(new FileSystemPath(Data.Emu_DOSBox_Path),
                            Resources.Links_Local_DOSBox),
                        new Page_Settings_LinkItemViewModel(new FileSystemPath(Data.Emu_DOSBox_ConfigPath),
                            Resources.Links_Local_DOSBoxConfig, UserLevel.Technical)
                    });

                    // Steam paths
                    try
                    {
                        using RegistryKey key = RegistryHelpers.GetKeyFromFullPath(@"HKEY_CURRENT_USER\Software\Valve\Steam", RegistryView.Default);
                        if (key != null)
                        {
                            FileSystemPath steamDir = key.GetValue("SteamPath", null)?.ToString();
                            var steamExe = key.GetValue("SteamExe", null)?.ToString();

                            if (steamDir.DirectoryExists)
                                LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                                {
                                    new Page_Settings_LinkItemViewModel(steamDir + steamExe, Resources.Links_Local_Steam),
                                    new Page_Settings_LinkItemViewModel(steamDir + @"steamapps\common",
                                        Resources.Links_Local_SteamGames, UserLevel.Advanced)
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Getting Steam Registry key");
                    }

                    // GOG paths
                    try
                    {
                        using RegistryKey key = RegistryHelpers.GetKeyFromFullPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths", RegistryView.Default);
                        if (key != null)
                        {
                            FileSystemPath gogDir = key.GetValue("client", null)?.ToString();

                            if (gogDir.DirectoryExists)
                                LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                                {
                                    new Page_Settings_LinkItemViewModel(gogDir + "GalaxyClient.exe",
                                        Resources.Links_Local_GOGClient),
                                    new Page_Settings_LinkItemViewModel(gogDir + @"Games", Resources.Links_Local_GOGGames,
                                        UserLevel.Advanced)
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Getting GOG Galaxy Registry key");
                    }

                    // Registry paths
                    LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                    {
                        new Page_Settings_LinkItemViewModel(AppFilePaths.RaymanRavingRabbidsRegistryKey,
                            Resources.Links_Local_RRRRegSettings, UserLevel.Technical),
                        new Page_Settings_LinkItemViewModel(AppFilePaths.RaymanRavingRabbids2RegistryKey,
                            Resources.Links_Local_RRR2RegSettings, UserLevel.Technical),
                        new Page_Settings_LinkItemViewModel(AppFilePaths.RaymanOriginsRegistryKey,
                            Resources.Links_Local_RORegSettings, UserLevel.Technical),
                        new Page_Settings_LinkItemViewModel(AppFilePaths.RaymanLegendsRegistryKey,
                            Resources.Links_Local_RLRegSettings, UserLevel.Technical),
                        new Page_Settings_LinkItemViewModel(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide",
                            Resources.Links_Local_nGlideRegSettings, UserLevel.Technical),
                        new Page_Settings_LinkItemViewModel(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide2",
                            Resources.Links_Local_nGlide2RegSettings, UserLevel.Technical)
                    });

                    // Debug paths
                    LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                    {
                        new Page_Settings_LinkItemViewModel(AppFilePaths.UserDataBaseDir, Resources.Links_Local_AppData,
                            UserLevel.Technical),
                        new Page_Settings_LinkItemViewModel(AppFilePaths.LogFile, Resources.Links_Local_LogFile,
                            UserLevel.Debug),
                        new Page_Settings_LinkItemViewModel(AppFilePaths.UtilitiesBaseDir, Resources.Links_Local_Utilities,
                            UserLevel.Debug),
                        new Page_Settings_LinkItemViewModel(AppFilePaths.RegistryBaseKey, Resources.Links_Local_RegAppData,
                            UserLevel.Technical)
                    });

                    time.Stop();

                    Logger.Info("The links have refreshed");
                    Logger.Debug("The link refresh time was {0} ms", time.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Refreshing links");
                }
            }
        }

        public void RefreshLanguages()
        {
            LocalizationManager.RefreshLanguages(Data.App_ShowIncompleteTranslations);
            OnPropertyChanged(nameof(CurrentCultureInfo));
        }

        public void RefreshAssociatedPrograms()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AssociatedPrograms.Clear();

                AssociatedPrograms.AddRange(Data.Archive_AssociatedPrograms.Select(x => new AssociatedProgramEntryViewModel(x.Key)));

                OnPropertyChanged(nameof(AssociatedPrograms));
            });
        }

        public async Task ResetAsync()
        {
            if (!await Services.MessageUI.DisplayMessageAsync(Resources.ConfirmResetData, Resources.ConfirmResetDataHeader, MessageType.Question, true))
            {
                return;
            }

            // The app data can't be reset while the app is running as it could cause multiple issues, so better to restart
            await App.RestartAsync("-reset");
        }

        #endregion

        #region Classes

        /// <summary>
        /// A collection of link item groups
        /// </summary>
        public class LinkItemCollection : ObservableCollection<Page_Settings_LinkItemViewModel[]>
        {
            /// <summary>
            /// Adds a new group to the collection
            /// </summary>
            /// <param name="group">The group to add</param>
            public new void Add(Page_Settings_LinkItemViewModel[] group)
            {
                // Get the valid items
                var validItems = group.Where(x => x.IsValid && x.MinUserLevel <= Services.Data.App_UserLevel).ToArray();

                // If there are valid items, add them
                if (validItems.Any())
                    base.Add(validItems);
            }
        }

        public class AssociatedProgramEntryViewModel : BaseRCPViewModel
        {
            public AssociatedProgramEntryViewModel(string fileExtension)
            {
                FileExtension = fileExtension;
            }

            public string FileExtension { get; }
            public string ExeFilePath
            {
                get => Data.Archive_AssociatedPrograms[FileExtension];
                set => Data.Archive_AssociatedPrograms[FileExtension] = value;
            }
        }

        #endregion
    }
}