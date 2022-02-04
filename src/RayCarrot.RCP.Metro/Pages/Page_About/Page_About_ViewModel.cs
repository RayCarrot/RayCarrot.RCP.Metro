using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the about page
/// </summary>
public class Page_About_ViewModel : BasePageViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Page_About_ViewModel(
        AppViewModel app, 
        AppUserData data, 
        IDialogBaseManager dialogBaseManager, 
        IMessageUIManager messageUi, 
        IFileManager file) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        DialogBaseManager = dialogBaseManager ?? throw new ArgumentNullException(nameof(dialogBaseManager));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        File = file ?? throw new ArgumentNullException(nameof(file));

        // Create commands
        OpenUrlCommand = new RelayCommand(x =>
        {
            if (x is not null)
                App.OpenUrl(x.ToString());
        });
        ShowVersionHistoryCommand = new AsyncRelayCommand(ShowVersionHistoryAsync);
        CheckForUpdatesCommand = new AsyncRelayCommand(async () => await App.CheckForUpdatesAsync(true));
        UninstallCommand = new AsyncRelayCommand(UninstallAsync);

        // Refresh the update badge property based on if new update is available
        Data.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Data.Update_IsUpdateAvailable))
                OnPropertyChanged(nameof(UpdateBadge));
        };
    }

    #endregion

    #region Commands

    public ICommand OpenUrlCommand { get; }
    public ICommand ShowVersionHistoryCommand { get; }
    public ICommand CheckForUpdatesCommand { get; }
    public ICommand UninstallCommand { get; }

    #endregion

    #region Services

    public AppUserData Data { get; }
    public IDialogBaseManager DialogBaseManager { get; }
    public IMessageUIManager MessageUI { get; }
    public IFileManager File { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.About;

    /// <summary>
    /// The update badge, indicating if new updates are available
    /// </summary>
    public string? UpdateBadge => Data.Update_IsUpdateAvailable ? "1" : null;

    #endregion

    #region Public Methods

    /// <summary>
    /// Shows the application version history
    /// </summary>
    public async Task ShowVersionHistoryAsync()
    {
        await DialogBaseManager.ShowWindowAsync(new AppNewsDialog());
    }

    /// <summary>
    /// Runs the uninstaller
    /// </summary>
    /// <returns>The task</returns>
    public async Task UninstallAsync()
    {
        // Confirm
        if (!await MessageUI.DisplayMessageAsync(Resources.About_ConfirmUninstall, Resources.About_ConfirmUninstallHeader, MessageType.Question, true))
            return;

        // Run the uninstaller
        if (await File.LaunchFileAsync(AppFilePaths.UninstallFilePath, true, $"\"{Assembly.GetEntryAssembly()?.Location}\"") == null)
        {
            string[] appDataLocations = 
            {
                AppFilePaths.UserDataBaseDir,
                AppFilePaths.RegistryBaseKey
            };

            await MessageUI.DisplayMessageAsync(String.Format(Resources.About_UninstallFailed, appDataLocations.JoinItems(Environment.NewLine)), MessageType.Error);

            return;
        }

        // Shut down the app
        await Metro.App.Current.ShutdownAppAsync(true);
    }

    #endregion
}