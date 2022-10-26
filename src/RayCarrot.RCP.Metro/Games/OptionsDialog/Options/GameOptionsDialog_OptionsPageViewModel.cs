﻿#nullable disable
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

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

        LaunchModeChangedCommand = new AsyncRelayCommand(LaunchModeChangedAsync);

        // Check if the launch mode can be changed
        CanChangeLaunchMode = gameInstallation.GameManager.SupportsGameLaunchMode;

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(GameInfoItems, this);

        // Refresh the game info
        RefreshGameInfo();

        // Refresh the game data on certain events
        App.RefreshRequired += App_RefreshRequiredAsync;
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

    #region Event Handlers

    private Task App_RefreshRequiredAsync(object sender, RefreshRequiredEventArgs e)
    {
        if (e.GameInfoModified)
            // Refresh the game info
            RefreshGameInfo();

        return Task.CompletedTask;
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Refreshes the game info
    /// </summary>
    protected void RefreshGameInfo()
    {
        GameInfoItems.Clear();
        GameInfoItems.AddRange(GameInstallation.GameManager.GetGameInfoItems(GameInstallation));
    }

    protected override object GetPageUI() => new GameOptions_Control()
    {
        DataContext = this
    };

    #endregion

    #region Public Methods

    public async Task LaunchModeChangedAsync()
    {
        await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(GameInstallation, RefreshFlags.LaunchInfo));
    }

    public override void Dispose()
    {
        // Dispose base
        base.Dispose();

        // Unsubscribe events
        App.RefreshRequired -= App_RefreshRequiredAsync;

        // Disable collection synchronization
        BindingOperations.DisableCollectionSynchronization(GameInfoItems);
    }

    #endregion
}