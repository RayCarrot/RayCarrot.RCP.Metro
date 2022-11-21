using System;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro;

public class GameViewModel : BaseViewModel
{
    #region Constructor

    public GameViewModel(GameDescriptor gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
        DisplayName = gameDescriptor.DisplayName;

        // Get and set platform info
        GamePlatformInfoAttribute platformInfo = gameDescriptor.Platform.GetInfo();
        PlatformDisplayName = platformInfo.DisplayName;
        PlatformIconSource = $"{AppViewModel.WPFApplicationBasePath}Img/GamePlatformIcons/{platformInfo.Icon.GetAttribute<ImageFileAttribute>()!.FileName}";

        // Create commands
        LocateCommand = new AsyncRelayCommand(LocateAsync);

        // Refresh
        Refresh();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand LocateCommand { get; }

    #endregion

    #region Public Properties

    public GameDescriptor GameDescriptor { get; }
    public LocalizedString DisplayName { get; }

    public LocalizedString PlatformDisplayName { get; }
    public string PlatformIconSource { get; }

    public bool IsAvailable { get; private set; }
    public bool CanLocate => GameDescriptor.CanBeLocated;

    #endregion

    #region Private Methods

    private void Refresh()
    {
        // TODO-UPDATE: Some games need to be disabled. Downloadable games can only be downloaded once, packaged apps can only be installed once and are not available on Windows 7
        IsAvailable = true;
    }

    #endregion

    #region Public Methods

    public async Task LocateAsync()
    {
        try
        {
            // Locate the game and get the path
            FileSystemPath? path = await GameDescriptor.LocateAsync();

            if (path == null)
                return;

            // Add the game
            GameInstallation gameInstallation = await Services.Games.AddGameAsync(GameDescriptor, path.Value);

            // TODO-UPDATE: Select game once added
            // Refresh
            Refresh();
            await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(gameInstallation, RefreshFlags.GameCollection));

            Logger.Info("The game {0} has been added", gameInstallation.Id);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(
                $"The game {GameDescriptor.DisplayName} was successfully added");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Locating game");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.LocateGame_Error, Resources.LocateGame_ErrorHeader);
        }
    }

    #endregion
}