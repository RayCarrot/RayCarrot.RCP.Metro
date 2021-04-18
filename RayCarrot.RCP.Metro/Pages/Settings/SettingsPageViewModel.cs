using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using RayCarrot.UI;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Nito.AsyncEx;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the settings page
    /// </summary>
    public class SettingsPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsPageViewModel()
        {
            // Create commands
            ContributeLocalizationCommand = new RelayCommand(ContributeLocalization);
            EditJumpListCommand = new AsyncRelayCommand(EditJumpListAsync);
            RefreshCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await RefreshAsync(true)));

            // Create properties
            AsyncLock = new AsyncLock();
            LocalLinkItems = new LinkItemCollection();

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(LocalLinkItems, this);

            // Refresh on startup
            Metro.App.Current.StartupComplete += async (s, e) => await RefreshAsync(true);

            Services.Data.CultureChanged += async (s, e) => await Task.Run(async () => await RefreshAsync(false));
            Services.Data.UserLevelChanged += async (s, e) => await Task.Run(async () => await RefreshAsync(false));
        }

        #endregion

        #region Commands

        public ICommand ContributeLocalizationCommand { get; }

        public ICommand EditJumpListCommand { get; }

        public ICommand RefreshCommand { get; }

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
            get => new CultureInfo(Data.CurrentCulture);
            set
            {
                if (value == null)
                    return;

                Data.CurrentCulture = value.Name;
            }
        }

        public bool ShowIncompleteTranslations
        {
            get => Data.ShowIncompleteTranslations;
            set
            {
                Data.ShowIncompleteTranslations = value;
                RefreshLanguages();
            }
        }

        /// <summary>
        /// The local link items
        /// </summary>
        public LinkItemCollection LocalLinkItems { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the URL for contributing to localizing the program
        /// </summary>
        public void ContributeLocalization()
        {
            App.OpenUrl(CommonUrls.TranslationUrl);
        }

        /// <summary>
        /// Edits the jump list items
        /// </summary>
        /// <returns>The task</returns>
        public async Task EditJumpListAsync()
        {
            // Get the result
            var result = await RCPServices.UI.EditJumpListAsync(new JumpListEditViewModel());

            if (result.CanceledByUser)
                return;

            // Update the jump list items collection
            Data.JumpListItemIDCollection = result.IncludedItems.Select(x => x.ID).ToList();

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, false, false, true));
        }

        /// <summary>
        /// Refreshes the page
        /// </summary>
        /// <param name="refreshLocalization">Indicates if the localization should be refreshed</param>
        /// <returns>The task</returns>
        public async Task RefreshAsync(bool refreshLocalization)
        {
            using (await AsyncLock.LockAsync())
            {
                // Refresh the languages
                if (refreshLocalization)
                    Application.Current.Dispatcher.Invoke(RefreshLanguages);

                try
                {
                    Stopwatch time = new Stopwatch();

                    time.Start();

                    RL.Logger?.LogInformationSource("The links are refreshing...");

                    LocalLinkItems.Clear();

                    // Config files
                    LocalLinkItems.Add(new LinkItemViewModel[]
                    {
                        new LinkItemViewModel(CommonPaths.UbiIniPath1, Resources.Links_Local_PrimaryUbiIni),
                        new LinkItemViewModel(CommonPaths.UbiIniPath2, Resources.Links_Local_SecondaryUbiIni,
                            UserLevel.Advanced),
                        new LinkItemViewModel(Games.Rayman2.IsAdded()
                            ? Games.Rayman2.GetInstallDir() + "ubi.ini"
                            : FileSystemPath.EmptyPath, Resources.Links_Local_R2UbiIni, UserLevel.Advanced),
                        new LinkItemViewModel(Environment.SpecialFolder.CommonApplicationData.GetFolderPath() + @"Ubisoft\RGH Launcher\1.0.0.0\Launcher_5.exe.config", Resources.Links_Local_RGHConfig, UserLevel.Advanced)
                    });

                    // DOSBox files
                    LocalLinkItems.Add(new LinkItemViewModel[]
                    {
                        new LinkItemViewModel(new FileSystemPath(Data.DosBoxPath),
                            Resources.Links_Local_DOSBox),
                        new LinkItemViewModel(new FileSystemPath(Data.DosBoxConfig),
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
                                LocalLinkItems.Add(new LinkItemViewModel[]
                                {
                                    new LinkItemViewModel(steamDir + steamExe, Resources.Links_Local_Steam),
                                    new LinkItemViewModel(steamDir + @"steamapps\common",
                                        Resources.Links_Local_SteamGames, UserLevel.Advanced)
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleUnexpected("Getting Steam Registry key");
                    }

                    // GOG paths
                    try
                    {
                        using RegistryKey key = RegistryHelpers.GetKeyFromFullPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths", RegistryView.Default);
                        if (key != null)
                        {
                            FileSystemPath gogDir = key.GetValue("client", null)?.ToString();

                            if (gogDir.DirectoryExists)
                                LocalLinkItems.Add(new LinkItemViewModel[]
                                {
                                    new LinkItemViewModel(gogDir + "GalaxyClient.exe",
                                        Resources.Links_Local_GOGClient),
                                    new LinkItemViewModel(gogDir + @"Games", Resources.Links_Local_GOGGames,
                                        UserLevel.Advanced)
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleUnexpected("Getting GOG Galaxy Registry key");
                    }

                    // Registry paths
                    LocalLinkItems.Add(new LinkItemViewModel[]
                    {
                        new LinkItemViewModel(CommonPaths.RaymanRavingRabbidsRegistryKey,
                            Resources.Links_Local_RRRRegSettings, UserLevel.Technical),
                        new LinkItemViewModel(CommonPaths.RaymanRavingRabbids2RegistryKey,
                            Resources.Links_Local_RRR2RegSettings, UserLevel.Technical),
                        new LinkItemViewModel(CommonPaths.RaymanOriginsRegistryKey,
                            Resources.Links_Local_RORegSettings, UserLevel.Technical),
                        new LinkItemViewModel(CommonPaths.RaymanLegendsRegistryKey,
                            Resources.Links_Local_RLRegSettings, UserLevel.Technical),
                        new LinkItemViewModel(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide",
                            Resources.Links_Local_nGlideRegSettings, UserLevel.Technical),
                        new LinkItemViewModel(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide2",
                            Resources.Links_Local_nGlide2RegSettings, UserLevel.Technical)
                    });

                    // Debug paths
                    LocalLinkItems.Add(new LinkItemViewModel[]
                    {
                        new LinkItemViewModel(CommonPaths.UserDataBaseDir, Resources.Links_Local_AppData,
                            UserLevel.Technical),
                        new LinkItemViewModel(CommonPaths.LogFile, Resources.Links_Local_LogFile,
                            UserLevel.Debug),
                        new LinkItemViewModel(CommonPaths.UtilitiesBaseDir, Resources.Links_Local_Utilities,
                            UserLevel.Debug),
                        new LinkItemViewModel(CommonPaths.RegistryBaseKey, Resources.Links_Local_RegAppData,
                            UserLevel.Technical)
                    });

                    time.Stop();

                    RL.Logger?.LogInformationSource("The links have refreshed");
                    RL.Logger?.LogDebugSource($"The link refresh time was {time.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Refreshing links");
                }
            }
        }

        public void RefreshLanguages()
        {
            LocalizationManager.RefreshLanguages(Data.ShowIncompleteTranslations);
            OnPropertyChanged(nameof(CurrentCultureInfo));
        }

        #endregion

        #region Classes

        /// <summary>
        /// A collection of link item groups
        /// </summary>
        public class LinkItemCollection : ObservableCollection<LinkItemViewModel[]>
        {
            /// <summary>
            /// Adds a new group to the collection
            /// </summary>
            /// <param name="group">The group to add</param>
            public new void Add(LinkItemViewModel[] group)
            {
                // Get the valid items
                var validItems = group.Where(x => x.IsValid && x.MinUserLevel <= RCPServices.Data.UserLevel).ToArray();

                // If there are valid items, add them
                if (validItems.Any())
                    base.Add(validItems);
            }
        }

        #endregion
    }
}