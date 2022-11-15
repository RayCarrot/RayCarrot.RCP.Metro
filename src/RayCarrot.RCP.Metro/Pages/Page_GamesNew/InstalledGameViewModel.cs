using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using NLog;

namespace RayCarrot.RCP.Metro;

public class InstalledGameViewModel : BaseViewModel
{
    #region Constructor

    public InstalledGameViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        DisplayName = gameInstallation.GameDescriptor.DisplayName;

        // TODO-UPDATE: Don't do this here
        IconKind = gameInstallation.GameDescriptor.Platform switch
        {
            GamePlatform.MSDOS => PackIconMaterialKind.DesktopClassic,
            GamePlatform.Win32 => PackIconMaterialKind.MicrosoftWindows,
            GamePlatform.Steam => PackIconMaterialKind.Steam,
            GamePlatform.WindowsPackage => PackIconMaterialKind.Package,
            _ => throw new ArgumentOutOfRangeException()
        };

        // TODO-14: Don't hard-code WPF paths like this as they're hard to find when reorganizing solution
        string bannerFileName = gameInstallation.GameDescriptor.Banner.GetAttribute<ImageFileAttribute>()?.FileName ?? "Default.png";
        GameBannerImageSource = $"{AppViewModel.WPFApplicationBasePath}Img/GameBanners/{bannerFileName}";

        // TODO-UPDATE: Reload all of these when the game info changes since paths etc. might be different
        GamePanels = new ObservableCollection<GamePanelViewModel>();
        AddGamePanels();

        AdditionalLaunchActions = new ObservableActionItemsCollection();
        AddAdditionalLaunchActions();

        // Create commands
        LaunchCommand = new AsyncRelayCommand(LaunchAsync);
        OpenOptionsCommand = new AsyncRelayCommand(OpenOptionsAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand LaunchCommand { get; }
    public ICommand OpenOptionsCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public GameDescriptor GameDescriptor => GameInstallation.GameDescriptor;
    public string DisplayName { get; }
    public PackIconMaterialKind IconKind { get; } // TODO-UPDATE: Use GenericIconKind

    public string IconSource => GameDescriptor.IconSource;
    public bool IsDemo => GameDescriptor.IsDemo;
    public string GameBannerImageSource { get; }

    public ObservableCollection<GamePanelViewModel> GamePanels { get; }
    public ObservableActionItemsCollection AdditionalLaunchActions { get; }

    #endregion

    #region Private Methods

    private void AddGamePanels()
    {
        // Patcher
        if (GameDescriptor.AllowPatching)
            GamePanels.Add(new PatcherGamePanelViewModel(GameInstallation));
        
        // Archive Explorer
        if (GameDescriptor.HasArchives)
            GamePanels.Add(new ArchiveGamePanelViewModel(GameInstallation));

        // Progression
        GameProgressionManager? progressionManager = GameDescriptor.GetGameProgressionManager(GameInstallation);
        if (progressionManager != null)
            GamePanels.Add(new ProgressionGamePanelViewModel(GameInstallation, progressionManager));
    }

    private void AddAdditionalLaunchActions()
    {
        // TODO-UPDATE: Always show this option if available (Win32, but not for Steam, packaged etc.)
        // Add run as admin option
        UserData_GameLaunchMode launchMode = GameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);

        if (launchMode == UserData_GameLaunchMode.AsAdminOption)
            AdditionalLaunchActions.AddGroup(new IconCommandItemViewModel(
                header: Resources.GameDisplay_RunAsAdmin, 
                description: null,
                iconKind: GenericIconKind.GameDisplay_Admin, 
                command: new AsyncRelayCommand(async () => await GameDescriptor.LaunchGameAsync(GameInstallation, true))));

        // Add local uri links
        AdditionalLaunchActions.AddGroup(GameDescriptor.GetLocalUriLinks(GameInstallation).
            Where(x => File.Exists(x.Uri)).
            Select<GameDescriptor.GameUriLink, ActionItemViewModel>(x =>
            {
                // Get the path
                string path = x.Uri;

                // Create the command
                AsyncRelayCommand command = new(async () =>
                    (await Services.File.LaunchFileAsync(path, arguments: x.Arguments))?.Dispose());

                if (x.Icon != GenericIconKind.None)
                    return new IconCommandItemViewModel(
                        header: x.Header,
                        description: path,
                        iconKind: x.Icon,
                        command: command);

                try
                {
                    return new ImageCommandItemViewModel(
                        header: x.Header,
                        description: path,
                        imageSource: WindowsHelpers.GetIconOrThumbnail(path, ShellThumbnailSize.Small).ToImageSource(),
                        command: command);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Getting file icon for drop-down action item");

                    return new IconCommandItemViewModel(
                        header: x.Header,
                        description: path,
                        iconKind: GenericIconKind.None,
                        command: command);
                }

            }));

        // Add external uri links
        AdditionalLaunchActions.AddGroup(GameDescriptor.GetExternalUriLinks(GameInstallation).
            Select(x =>
            {
                // Get the path
                string path = x.Uri;

                // Create the command
                AsyncRelayCommand command = new(async () =>
                    (await Services.File.LaunchFileAsync(path, arguments: x.Arguments))?.Dispose());

                return new IconCommandItemViewModel(
                    header: x.Header,
                    description: path,
                    iconKind: x.Icon,
                    command: command);
            }));

        // Add RayMap link
        GameDescriptor.RayMapInfo? rayMapInfo = GameDescriptor.GetRayMapInfo();
        if (rayMapInfo != null)
        {
            string url = rayMapInfo.GetURL();
            AdditionalLaunchActions.AddGroup(new ImageCommandItemViewModel(
                header: Resources.GameDisplay_Raymap, 
                description: url,
                imageSource: (ImageSource)new ImageSourceConverter().ConvertFrom($"{AppViewModel.WPFApplicationBasePath}Img/RayMap/{rayMapInfo.Viewer}.png")!, 
                command: new AsyncRelayCommand(async () => (await Services.File.LaunchFileAsync(url))?.Dispose())));
        }

        // Add open location (don't add as a group since it's the last item)
        AdditionalLaunchActions.Add(new IconCommandItemViewModel(
            header: Resources.GameDisplay_OpenLocation, 
            description: GameInstallation.InstallLocation.FullPath,
            iconKind: GenericIconKind.GameDisplay_Location, 
            command: new AsyncRelayCommand(async () =>
            {
                // Get the install directory
                FileSystemPath instDir = GameInstallation.InstallLocation;

                // Select the file in Explorer if it exists
                if ((instDir + GameDescriptor.DefaultFileName).FileExists)
                    instDir += GameDescriptor.DefaultFileName;

                // Open the location
                await Services.File.OpenExplorerLocationAsync(instDir);

                Logger.Trace("The Game {0} install location was opened", GameInstallation.Id);
            })));
    }

    #endregion

    #region Public Methods

    public Task LoadAsync()
    {
        return Task.WhenAll(GamePanels.Select(x => x.LoadAsync()));
    }

    public Task LaunchAsync() => GameDescriptor.LaunchGameAsync(GameInstallation, false);
    public Task OpenOptionsAsync() => Services.UI.ShowGameOptionsAsync(GameInstallation);

    #endregion
}