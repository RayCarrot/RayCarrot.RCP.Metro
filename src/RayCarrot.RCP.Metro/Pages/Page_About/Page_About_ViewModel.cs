using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

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
        IFileManager file, 
        DeployableFilesManager deployableFiles) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        DialogBaseManager = dialogBaseManager ?? throw new ArgumentNullException(nameof(dialogBaseManager));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        File = file ?? throw new ArgumentNullException(nameof(file));
        DeployableFiles = deployableFiles ?? throw new ArgumentNullException(nameof(deployableFiles));

        // Set the credits
        Credits = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("RayCarrot", new ResourceLocString(nameof(Resources.About_Credits_RayCarrot))),
            new("Droolie", new ResourceLocString(nameof(Resources.About_Credits_Droolie))),
            new("Fabiosek", new ResourceLocString(nameof(Resources.About_Credits_Fabiosek))),
            new("Haruka Tavares", new ResourceLocString(nameof(Resources.About_Credits_HarukaTavares))),
            new("ItzalDrake", new ResourceLocString(nameof(Resources.About_Credits_ItzalDrake))),
            new("Juanmv94", new ResourceLocString(nameof(Resources.About_Credits_Janmv94))),
            new("Mark", new ResourceLocString(nameof(Resources.About_Credits_432Hz))),
            new("Marteaufou", new ResourceLocString(nameof(Resources.About_Credits_Marteaufou))),
            new("Noserdog", new ResourceLocString(nameof(Resources.About_Credits_Noserdog))),
            new("PluMGMK", new ResourceLocString(nameof(Resources.About_Credits_PluMGMK))),
            new("RayActivity", new ResourceLocString(nameof(Resources.About_Credits_RayActivity))),
            new("Rayman Universe - Рэйман и его Вселенная", new ResourceLocString(nameof(Resources.About_Credits_RaymanUniverse))),
            new("RibShark", new ResourceLocString(nameof(Resources.About_Credits_RibShark))),
            new("Robin", new ResourceLocString(nameof(Resources.About_Credits_Robin))),
            new("Snagglebee", new ResourceLocString(nameof(Resources.About_Credits_Snagglebee))),
            new("XanderNT", new ResourceLocString(nameof(Resources.About_Credits_XanderNT))),
            new("ZetaXO", new ResourceLocString(nameof(Resources.About_Credits_ZetaXO))),
        };

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

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand OpenUrlCommand { get; }
    public ICommand ShowVersionHistoryCommand { get; }
    public ICommand CheckForUpdatesCommand { get; }
    public ICommand UninstallCommand { get; }

    #endregion

    #region Services

    private AppUserData Data { get; }
    private IDialogBaseManager DialogBaseManager { get; }
    private IMessageUIManager MessageUI { get; }
    private IFileManager File { get; }
    private DeployableFilesManager DeployableFiles { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.About;

    public ObservableCollection<DuoGridItemViewModel> Credits { get; }

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

        FileSystemPath filePath;

        try
        {
            filePath = DeployableFiles.DeployFile(DeployableFilesManager.DeployableFile.Uninstaller);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Deploying uninstaller");
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.DeployFilesError);
            return;
        }

        // Run the uninstaller
        if (await File.LaunchFileAsync(filePath, true, $"\"{Assembly.GetEntryAssembly()?.Location}\"") == null)
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