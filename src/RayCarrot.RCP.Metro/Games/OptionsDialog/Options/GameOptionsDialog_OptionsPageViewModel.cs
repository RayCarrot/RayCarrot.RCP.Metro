#nullable disable
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace RayCarrot.RCP.Metro;

public class GameOptionsDialog_OptionsPageViewModel : GameOptionsDialog_BasePageViewModel
{
    #region Constructor

    public GameOptionsDialog_OptionsPageViewModel(GameInstallation gameInstallation) 
        : base(new ResourceLocString(nameof(Resources.GameOptions_Options)), GenericIconKind.GameOptions_General)
    {
        // Set properties
        GameInstallation = gameInstallation;
        GameInfoItems = new ObservableCollection<DuoGridItemViewModel>();
        OptionsContent = gameInstallation.GameDescriptor.GetOptionsUI(gameInstallation);

        LaunchModeChangedCommand = new RelayCommand(LaunchModeChanged);

        // Check if the launch mode can be changed
        CanChangeLaunchMode = gameInstallation.GameDescriptor.SupportsGameLaunchMode;

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(GameInfoItems, this);

        // Refresh the game info
        RefreshGameInfo();
    }

    #endregion

    #region Commands

    public ICommand LaunchModeChangedCommand { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The game installation
    /// </summary>
    public GameInstallation GameInstallation { get; }

    /// <summary>
    /// The game info items
    /// </summary>
    public ObservableCollection<DuoGridItemViewModel> GameInfoItems { get; }

    /// <summary>
    /// The game options content
    /// </summary>
    public object OptionsContent { get; }

    /// <summary>
    /// Indicates if the launch mode can be changed
    /// </summary>
    public bool CanChangeLaunchMode { get; }

    public bool HasOptionsOrCanChangeLaunchMode => CanChangeLaunchMode || OptionsContent != null;

    /// <summary>
    /// The game's launch mode
    /// </summary>
    public UserData_GameLaunchMode LaunchMode
    {
        get => GameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
        set => GameInstallation.SetValue(GameDataKey.Win32LaunchMode, value);
    }

    #endregion


    #region Protected Methods

    protected override Task OnGameInfoModifiedAsync()
    {
        RefreshGameInfo();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Refreshes the game info
    /// </summary>
    protected void RefreshGameInfo()
    {
        GameInfoItems.Clear();
        GameInfoItems.AddRange(GameInstallation.GameDescriptor.GetGameInfoItems(GameInstallation));
    }

    protected override object GetPageUI() => new GameOptions_Control()
    {
        DataContext = this
    };

    #endregion

    #region Public Methods

    public void LaunchModeChanged()
    {
        Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
    }

    public override void Dispose()
    {
        // Dispose base
        base.Dispose();

        // Disable collection synchronization
        BindingOperations.DisableCollectionSynchronization(GameInfoItems);
    }

    #endregion
}