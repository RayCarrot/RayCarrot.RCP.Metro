using System.Windows;

namespace RayCarrot.RCP.Metro.Games.Settings;

/// <summary>
/// Interaction logic for GameSettingsDialog.xaml
/// </summary>
public partial class GameSettingsDialog : WindowContentControl, IInitializable, IRecipient<RemovedGamesMessage>
{
    #region Constructor

    public GameSettingsDialog(GameSettingsViewModel gameSettingsViewModel)
    {
        // Set up UI
        InitializeComponent();

        // Set the view model
        ViewModel = gameSettingsViewModel;
        DataContext = ViewModel;

        Loaded += GameSettingsDialog_OnLoaded;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _forceClose;

    #endregion

    #region Public Properties

    public override bool IsResizable => true;
    public GameSettingsViewModel ViewModel { get; }

    #endregion

    #region Event Handlers

    private async void GameSettingsDialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= GameSettingsDialog_OnLoaded;

        // Initialize
        bool success = await ViewModel.InitializeAsync();

        if (!success)
            ForceClose();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = ViewModel is GameClientGameSettingsViewModel clientSettings
            ? String.Format(Metro.Resources.GameClientGameSettingsTitle,
                clientSettings.GameClientInstallation.GameClientDescriptor.DisplayName,
                ViewModel.GameInstallation.GetDisplayName())
            : String.Format(Metro.Resources.GameSettingsTitle, 
                ViewModel.GameInstallation.GetDisplayName());
        WindowInstance.Icon = GenericIconKind.Window_GameSettings;
        WindowInstance.MinWidth = 500;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 650;
        WindowInstance.Height = 700;
    }

    protected override async Task<bool> ClosingAsync()
    {
        if (!await base.ClosingAsync())
            return false;

        // Always allow the window to close if it's set to force close
        if (_forceClose)
            return true;

        // Ask user if there are unsaved changes
        if (ViewModel.UnsavedChanges)
            return await Services.MessageUI.DisplayMessageAsync(Metro.Resources.UnsavedChangesQuestion,
                Metro.Resources.UnsavedChangesQuestionHeader, MessageType.Question, true);
        else
            return true;
    }

    #endregion

    #region Public Methods

    public void ForceClose()
    {
        Logger.Trace("Force closing the game settings dialog");

        _forceClose = true;
        WindowInstance.Close();
    }

    public void Initialize()
    {
        Services.Messenger.RegisterAll(this);
    }

    public void Deinitialize()
    {
        Services.Messenger.UnregisterAll(this);
    }

    #endregion

    #region Message Receivers

    void IRecipient<RemovedGamesMessage>.Receive(RemovedGamesMessage message)
    {
        if (message.GameInstallations.Contains(ViewModel.GameInstallation))
            ForceClose();
    }

    #endregion
}