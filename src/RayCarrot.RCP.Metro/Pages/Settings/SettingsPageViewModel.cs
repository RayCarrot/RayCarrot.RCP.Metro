using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Pages.Settings.Sections;

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
        EditJumpListCommand = new AsyncRelayCommand(EditJumpListAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        OpenFileCommand = new AsyncRelayCommand(x => OpenFileAsync((FileSystemPath)x!));
        OpenDirectoryCommand = new AsyncRelayCommand(x => OpenDirectoryAsync((FileSystemPath)x!));
        OpenRegistryKeyCommand = new AsyncRelayCommand(x => OpenRegistryKeyAsync((string)x!));
        ResetCommand = new AsyncRelayCommand(ResetAsync);

        // Create properties
        AsyncLock = new AsyncLock();

        // Create sections
        Sections = new ObservableCollection<ObservableCollection<SettingsSectionViewModel>>()
        {
            new() { new LanguageSettingsSectionViewModel(Data, App), new UserLevelSettingsSectionViewModel(Data), },
            new() { new GeneralSettingsSectionViewModel(Data), },
            new() { new DesignSettingsSectionViewModel(Data), },
            new() { new StartupSettingsSectionViewModel(Data), },
            new() { new FilesSettingsSectionViewModel(Data), },
            new() { new WindowsIntegrationSettingsSectionViewModel(Data, MessageUI), },
            new() { new ProgressionSettingsSectionViewModel(Data), },
            new() { new ArchiveExplorerSettingsSectionViewModel(Data), },
            new() { new ModLoaderSettingsSectionViewModel(Data), },
            new() { new DebugSettingsSectionViewModel(Data), },
        };
        FlatSections = new ObservableCollection<SettingsSectionViewModel>(Sections.SelectMany(x => x));
    }

    #endregion

    #region Commands

    public ICommand EditJumpListCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand OpenDirectoryCommand { get; }
    public ICommand OpenRegistryKeyCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ResetCommand { get; }

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

    public ObservableCollection<ObservableCollection<SettingsSectionViewModel>> Sections { get; }
    public ObservableCollection<SettingsSectionViewModel> FlatSections { get; }

    #endregion

    #region Public Methods

    protected override async Task InitializeAsync()
    {
        await RefreshAsync();
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
    /// <returns>The task</returns>
    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            // Refresh the sections
            foreach (SettingsSectionViewModel section in FlatSections)
                section.Refresh();
        }
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

    #endregion
}