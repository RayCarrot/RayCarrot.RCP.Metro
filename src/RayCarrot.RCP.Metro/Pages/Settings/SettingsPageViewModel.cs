using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Metro.Pages.Settings;

/// <summary>
/// View model for the settings page
/// </summary>
public class SettingsPageViewModel : BasePageViewModel
{
    #region Constructor

    public SettingsPageViewModel(
        AppViewModel app, 
        AppUserData data, 
        IAppInstanceData instanceData, 
        IMessageUIManager messageUi, 
        AppUIManager ui, 
        GamesManager gamesManager, 
        JumpListManager jumpListManager, 
        FileManager fileManager) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        InstanceData = instanceData ?? throw new ArgumentNullException(nameof(instanceData));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        JumpListManager = jumpListManager ?? throw new ArgumentNullException(nameof(jumpListManager));
        FileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));

        // Create commands
        ContributeLocalizationCommand = new RelayCommand(ContributeLocalization);
        EditJumpListCommand = new AsyncRelayCommand(EditJumpListAsync);
        RefreshCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await RefreshAsync(true, true, true)));
        OpenFileCommand = new AsyncRelayCommand(x => OpenFileAsync((FileSystemPath)x!));
        OpenDirectoryCommand = new AsyncRelayCommand(x => OpenDirectoryAsync((FileSystemPath)x!));
        OpenRegistryKeyCommand = new AsyncRelayCommand(x => OpenRegistryKeyAsync((string)x!));
        ResetCommand = new AsyncRelayCommand(ResetAsync);

        UpdatePatchFileTypeAssociationCommand = new AsyncRelayCommand(UpdatePatchFileTypeAssociationAsync);
        UpdatePatchURIProtocolAssociationCommand = new AsyncRelayCommand(UpdatePatchURIProtocolAssociationAsync);

        // Create properties
        AsyncLock = new AsyncLock();
        AssociatedPrograms = new ObservableCollection<AssociatedProgramEntryViewModel>();

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
    public ICommand OpenFileCommand { get; }
    public ICommand OpenDirectoryCommand { get; }
    public ICommand OpenRegistryKeyCommand { get; }
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
    private FileManager FileManager { get; }

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
                bool? isAssociatedWithFileType = new ModFileLaunchHandler().IsAssociatedWithFileType();
                bool? isAssociatedWithURIProtocol = new ModFileUriLaunchHandler().IsAssociatedWithUriProtocol();

                CanAssociatePatchFileType = isAssociatedWithFileType != null;
                CanAssociatePatchURIProtocol = isAssociatedWithURIProtocol != null;

                AssociatePatchFileType = isAssociatedWithFileType ?? false;
                AssociatePatchURIProtocol = isAssociatedWithURIProtocol ?? false;
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

    public Task OpenFileAsync(FileSystemPath filePath) => FileManager.LaunchFileAsync(filePath);
    public Task OpenDirectoryAsync(FileSystemPath dirPath) => FileManager.OpenExplorerLocationAsync(dirPath);
    public Task OpenRegistryKeyAsync(string registryKey) => FileManager.OpenRegistryKeyAsync(registryKey);

    public async Task ResetAsync()
    {
        if (!await MessageUI.DisplayMessageAsync(Resources.ConfirmResetData, Resources.ConfirmResetDataHeader, MessageType.Question, true))
        {
            return;
        }

        // The app data can't be reset while the app is running as it could cause multiple issues, so better to restart
        await App.RestartAsync("-reset");
    }

    // TODO-UPDATE: Rework UI to have these things be dynamic based on the available handlers
    public async Task UpdatePatchFileTypeAssociationAsync()
    {
        try
        {
            new ModFileLaunchHandler().AssociateWithFileType(Data.App_ApplicationPath, AssociatePatchFileType);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting mod file type association");            

            // TODO-UPDATE: Update localization
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_AssociateFileTypeError);
        }
    }

    public async Task UpdatePatchURIProtocolAssociationAsync()
    {
        try
        {
            new ModFileUriLaunchHandler().AssociateWithUriProtocol(Data.App_ApplicationPath, AssociatePatchURIProtocol);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting mod uri protocol association");

            // TODO-UPDATE: Update localization
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_AssociateURIProtocolError);
        }
    }

    #endregion

    #region Classes

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