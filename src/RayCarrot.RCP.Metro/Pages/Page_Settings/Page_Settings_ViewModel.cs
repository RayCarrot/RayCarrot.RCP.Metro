using Microsoft.Win32;
using Nito.AsyncEx;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

// TODO-14: Perhaps remove the links page? We could move game-specific links to game hub (like registry key links). The RCP
//          specific ones could be moved to the page popup for either the Settings or About page.

/// <summary>
/// View model for the settings page
/// </summary>
public class Page_Settings_ViewModel : BasePageViewModel
{
    #region Constructor

    public Page_Settings_ViewModel(
        AppViewModel app, 
        AppUserData data, 
        IAppInstanceData instanceData, 
        IMessageUIManager messageUi, 
        AppUIManager ui, 
        GamesManager gamesManager, 
        JumpListManager jumpListManager) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        InstanceData = instanceData ?? throw new ArgumentNullException(nameof(instanceData));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        JumpListManager = jumpListManager ?? throw new ArgumentNullException(nameof(jumpListManager));

        // Create commands
        ContributeLocalizationCommand = new RelayCommand(ContributeLocalization);
        EditJumpListCommand = new AsyncRelayCommand(EditJumpListAsync);
        RefreshCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await RefreshAsync(true, true, true)));
        ResetCommand = new AsyncRelayCommand(ResetAsync);

        UpdatePatchFileTypeAssociationCommand = new AsyncRelayCommand(UpdatePatchFileTypeAssociationAsync);
        UpdatePatchURIProtocolAssociationCommand = new AsyncRelayCommand(UpdatePatchURIProtocolAssociationAsync);

        // Create properties
        AsyncLock = new AsyncLock();
        LocalLinkItems = new LinkItemCollection(Data);
        AssociatedPrograms = new ObservableCollection<AssociatedProgramEntryViewModel>();

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(LocalLinkItems, this);

        // Refresh when needed
        InstanceData.CultureChanged += async (_, _) => await Task.Run(async () => await RefreshAsync(false, false, false));
        InstanceData.UserLevelChanged += async (_, _) => await Task.Run(async () => await RefreshAsync(false, false, false));
        Data.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Data.Archive_AssociatedPrograms))
                RefreshAssociatedPrograms();
        };
        AssociatedPrograms.CollectionChanged += (_, e) =>
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

    public ICommand UpdatePatchFileTypeAssociationCommand { get; }
    public ICommand UpdatePatchURIProtocolAssociationCommand { get; }

    #endregion

    #region Private Properties

    /// <summary>
    /// The async lock for refreshing the links
    /// </summary>
    private AsyncLock AsyncLock { get; }

    #endregion

    #region Services

    public AppUserData Data { get; }
    public IAppInstanceData InstanceData { get; }
    public IMessageUIManager MessageUI { get; }
    public AppUIManager UI { get; }
    public GamesManager GamesManager { get; }
    private JumpListManager JumpListManager { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Settings;

    /// <summary>
    /// The current culture info
    /// </summary>
    public CultureInfo? CurrentCultureInfo
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

    public bool CanAssociatePatchFileType { get; set; }
    public bool CanAssociatePatchURIProtocol { get; set; }
    public bool AssociatePatchFileType { get; set; }
    public bool AssociatePatchURIProtocol { get; set; }

    #endregion

    #region Public Methods

    protected override Task InitializeAsync()
    {
        return RefreshAsync(true, true, true);
    }

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
        JumpListEditResult result = await UI.EditJumpListAsync(new JumpListEditViewModel());

        if (result.CanceledByUser)
            return;

        // Update the jump list items collection
        Data.App_AutoSortJumpList = result.AutoSort;
        JumpListManager.SetItems(result.IncludedItems.Select(x => new JumpListItem(x.GameInstallation.InstallationId, x.Id)));
    }

    /// <summary>
    /// Refreshes the page
    /// </summary>
    /// <param name="refreshLocalization">Indicates if the localization should be refreshed</param>
    /// <param name="refreshAssociatedPrograms">Indicates if the associated programs should be refreshed</param>
    /// <param name="refreshPatchAssociations">Indicates if the patch file and URL associations should be refreshed</param>
    /// <returns>The task</returns>
    public async Task RefreshAsync(bool refreshLocalization, bool refreshAssociatedPrograms, bool refreshPatchAssociations)
    {
        using (await AsyncLock.LockAsync())
        {
            // Refresh the languages
            if (refreshLocalization)
                Application.Current.Dispatcher.Invoke(RefreshLanguages);

            // Refresh associated programs
            if (refreshAssociatedPrograms)
                RefreshAssociatedPrograms();

            if (refreshPatchAssociations)
            {
                bool? isAssociatedWithFileType = PatchFile.IsAssociatedWithFileType();
                bool? isAssociatedWithURIProtocol = PatchFile.IsAssociatedWithURIProtocol();

                CanAssociatePatchFileType = isAssociatedWithFileType != null;
                CanAssociatePatchURIProtocol = isAssociatedWithURIProtocol != null;

                AssociatePatchFileType = isAssociatedWithFileType ?? false;
                AssociatePatchURIProtocol = isAssociatedWithURIProtocol ?? false;
            }

            try
            {
                Stopwatch time = new();

                time.Start();

                Logger.Info("The links are refreshing...");

                LocalLinkItems.Clear();

                GameInstallation? r2GameInstallation = GamesManager.FindInstalledGame(GameSearch.Create(Game.Rayman2, GamePlatform.Win32));

                // Config files
                LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                {
                    new(AppFilePaths.UbiIniPath1, Resources.Links_Local_PrimaryUbiIni),
                    new(AppFilePaths.UbiIniPath2, Resources.Links_Local_SecondaryUbiIni, UserLevel.Advanced),
                    new(r2GameInstallation != null
                        ? r2GameInstallation.InstallLocation.Directory + "ubi.ini"
                        : FileSystemPath.EmptyPath, Resources.Links_Local_R2UbiIni, UserLevel.Advanced),
                    new(Environment.SpecialFolder.CommonApplicationData.GetFolderPath() + @"Ubisoft\RGH Launcher\1.0.0.0\Launcher_5.exe.config", Resources.Links_Local_RGHConfig, UserLevel.Advanced)
                });

                // Steam paths
                try
                {
                    using RegistryKey? key = RegistryHelpers.GetKeyFromFullPath(@"HKEY_CURRENT_USER\Software\Valve\Steam", RegistryView.Default);

                    if (key != null)
                    {
                        FileSystemPath steamDir = key.GetValue("SteamPath", null)?.ToString();
                        string? steamExe = key.GetValue("SteamExe", null)?.ToString();

                        if (steamDir.DirectoryExists)
                        {
                            if (steamExe != null)
                                LocalLinkItems.Add(new Page_Settings_LinkItemViewModel(
                                    steamDir + steamExe, Resources.Links_Local_Steam).YieldToArray());

                            LocalLinkItems.Add(new Page_Settings_LinkItemViewModel(
                                steamDir + @"steamapps\common", Resources.Links_Local_SteamGames, UserLevel.Advanced).YieldToArray());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Getting Steam Registry key");
                }

                // GOG paths
                try
                {
                    using RegistryKey? key = RegistryHelpers.GetKeyFromFullPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths", RegistryView.Default);
                    
                    if (key != null)
                    {
                        FileSystemPath gogDir = key.GetValue("client", null)?.ToString();

                        if (gogDir.DirectoryExists)
                            LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                            {
                                new(gogDir + "GalaxyClient.exe", Resources.Links_Local_GOGClient),
                                new(gogDir + @"Games", Resources.Links_Local_GOGGames, UserLevel.Advanced)
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
                    new(AppFilePaths.RaymanRavingRabbidsRegistryKey, Resources.Links_Local_RRRRegSettings, UserLevel.Technical),
                    new(AppFilePaths.RaymanRavingRabbids2RegistryKey, Resources.Links_Local_RRR2RegSettings, UserLevel.Technical),
                    new(AppFilePaths.RaymanOriginsRegistryKey, Resources.Links_Local_RORegSettings, UserLevel.Technical),
                    new(AppFilePaths.RaymanLegendsRegistryKey, Resources.Links_Local_RLRegSettings, UserLevel.Technical),
                    new(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide", Resources.Links_Local_nGlideRegSettings, UserLevel.Technical),
                    new(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide2", Resources.Links_Local_nGlide2RegSettings, UserLevel.Technical)
                });

                // Debug paths
                LocalLinkItems.Add(new Page_Settings_LinkItemViewModel[]
                {
                    new(AppFilePaths.UserDataBaseDir, Resources.Links_Local_AppData, UserLevel.Technical),
                    new(AppFilePaths.LogFile, Resources.Links_Local_LogFile, UserLevel.Debug),
                    new(AppFilePaths.UtilitiesBaseDir, Resources.Links_Local_Utilities, UserLevel.Debug),
                    new(AppFilePaths.TempPath, Resources.Links_Local_Temp, UserLevel.Debug),
                    new(AppFilePaths.RegistryBaseKey, Resources.Links_Local_RegAppData, UserLevel.Technical)
                });

                // Load icons
                await Task.Run(() =>
                {
                    foreach (Page_Settings_LinkItemViewModel link in LocalLinkItems.SelectMany(x => x))
                        link.LoadIcon();
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
        if (!await MessageUI.DisplayMessageAsync(Resources.ConfirmResetData, Resources.ConfirmResetDataHeader, MessageType.Question, true))
        {
            return;
        }

        // The app data can't be reset while the app is running as it could cause multiple issues, so better to restart
        await App.RestartAsync("-reset");
    }

    public async Task UpdatePatchFileTypeAssociationAsync()
    {
        try
        {
            PatchFile.AssociateWithFileType(Data.App_ApplicationPath, AssociatePatchFileType);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting patch file type association");            

            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_AssociateFileTypeError);
        }
    }

    public async Task UpdatePatchURIProtocolAssociationAsync()
    {
        try
        {
            PatchFile.AssociateWithURIProtocol(Data.App_ApplicationPath, AssociatePatchURIProtocol);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting patch uri protocol association");

            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_AssociateURIProtocolError);
        }
    }

    #endregion

    #region Classes

    /// <summary>
    /// A collection of link item groups
    /// </summary>
    public class LinkItemCollection : ObservableCollection<Page_Settings_LinkItemViewModel[]>
    {
        public LinkItemCollection(AppUserData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public AppUserData Data { get; }

        /// <summary>
        /// Adds a new group to the collection
        /// </summary>
        /// <param name="group">The group to add</param>
        public new void Add(Page_Settings_LinkItemViewModel[] group)
        {
            // Get the valid items
            var validItems = group.Where(x => x.IsValid && x.MinUserLevel <= Data.App_UserLevel).ToArray();

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