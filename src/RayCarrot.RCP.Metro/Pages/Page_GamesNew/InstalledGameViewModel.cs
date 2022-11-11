using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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

        GamePanels = new ObservableCollection<GamePanelViewModel>();
        AddGamePanels();

        AdditionalLaunchActions = new ObservableCollection<ActionItemViewModel>();
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
    public ObservableCollection<ActionItemViewModel> AdditionalLaunchActions { get; }

    #endregion

    #region Private Methods

    private void AddGamePanels()
    {
        if (GameDescriptor.AllowPatching)
            GamePanels.Add(new PatcherGamePanelViewModel(GameInstallation));
        
        if (GameDescriptor.HasArchives)
            GamePanels.Add(new ArchiveGamePanelViewModel(GameInstallation));

        // TODO-UPDATE: Have these be conditional as well
        GamePanels.Add(new ProgressionGamePanelViewModel(GameInstallation));
        GamePanels.Add(new LinksGamePanelViewModel(GameInstallation));
    }

    private void AddAdditionalLaunchActions()
    {
        // TODO-UPDATE: Always show this option if available (Win32, but not for Steam, packaged etc.)
        // Add run as admin option
        UserData_GameLaunchMode launchMode = GameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);

        if (launchMode == UserData_GameLaunchMode.AsAdminOption)
        {
            AdditionalLaunchActions.Add(new IconCommandItemViewModel(
                header: Resources.GameDisplay_RunAsAdmin, 
                iconKind: GenericIconKind.GameDisplay_Admin, 
                command: new AsyncRelayCommand(async () => await GameDescriptor.LaunchGameAsync(GameInstallation, true))));

            AdditionalLaunchActions.Add(new SeparatorItemViewModel());
        }

        // Get the Game links
        var links = GameDescriptor.GetGameFileLinks(GameInstallation).Where(x => x.Path.FileExists).ToArray();

        // Add links if there are any
        if (links.Any())
        {
            AdditionalLaunchActions.AddRange(links.
                Select<GameDescriptor.GameFileLink, ActionItemViewModel>(x =>
                {
                    // Get the path
                    string path = x.Path;

                    // Create the command
                    AsyncRelayCommand command = new(async () => 
                        (await Services.File.LaunchFileAsync(path, arguments: x.Arguments))?.Dispose());

                    if (x.Icon != GenericIconKind.None)
                        return new IconCommandItemViewModel(
                            header: x.Header, 
                            iconKind: x.Icon, 
                            command: command);

                    try
                    {
                        return new ImageCommandItemViewModel(
                            header: x.Header, 
                            imageSource: WindowsHelpers.GetIconOrThumbnail(x.Path, ShellThumbnailSize.Small).ToImageSource(), 
                            command: command);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Getting file icon for drop-down action item");

                        return new IconCommandItemViewModel(
                            header: x.Header,
                            iconKind: GenericIconKind.None,
                            command: command);
                    }
                }));

            AdditionalLaunchActions.Add(new SeparatorItemViewModel());
        }

        // Add open location
        AdditionalLaunchActions.Add(new IconCommandItemViewModel(
            header: Resources.GameDisplay_OpenLocation, 
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