using System.ComponentModel;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class GameClientGameConfigPageViewModel : GameOptionsDialogPageViewModel, 
    IRecipient<AddedGameClientsMessage>, IRecipient<RemovedGameClientsMessage>, IRecipient<ModifiedGamesMessage>
{
    #region Constructor

    public GameClientGameConfigPageViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        ConfigureGameClientsCommand = new AsyncRelayCommand(ConfigureGameClientsAsync);

        // TODO-14: Only do this once page is first loaded!
        // Register for messages
        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private GameClientViewModel? _selectedGameClient;

    #endregion

    #region Commands

    public ICommand ConfigureGameClientsCommand { get; }

    #endregion

    #region Public Properties

    public override LocalizedString PageName => "Game client"; // TODO-UPDATE: Localize
    public override GenericIconKind PageIcon => GenericIconKind.GameOptions_GameClient;
    public override bool CanSave => GameClientGameConfig?.CanSave ?? false;
    public override bool CanUseRecommended => GameClientGameConfig?.CanUseRecommended ?? false;

    public GameInstallation GameInstallation { get; }

    public ObservableCollection<GameClientViewModel>? GameClients { get; set; }

    public GameClientViewModel? SelectedGameClient
    {
        get => _selectedGameClient;
        set
        {
            _selectedGameClient = value;

            Invoke();
            async void Invoke()
            {
                await GameInstallation.GameDescriptor.SetGameClientAsync(GameInstallation, value?.GameClientInstallation);
                await SetSelectedGameClientAsync(value);
            }
        }
    }

    public GameClientGameConfigViewModel? GameClientGameConfig { get; set; }

    #endregion

    #region Private Methods

    private async Task SetSelectedGameClientAsync(GameClientViewModel? gameClient)
    {
        _selectedGameClient = gameClient;
        OnPropertyChanged(nameof(SelectedGameClient));

        if (GameClientGameConfig != null)
            GameClientGameConfig.PropertyChanged -= GameClientGameConfig_PropertyChanged;

        // Set the config
        GameClientGameConfig = gameClient?.GameClientInstallation.GameClientDescriptor.GetGameConfigViewModel(GameInstallation, gameClient.GameClientInstallation);

        // Update page properties to match the config
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanUseRecommended));

        UnsavedChanges = GameClientGameConfig?.UnsavedChanges ?? false;
        if (GameClientGameConfig != null)
            GameClientGameConfig.PropertyChanged += GameClientGameConfig_PropertyChanged;

        try
        {
            GameClientGameConfig?.Load();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading game client game config");

            // TODO-14: Show an error message to the user

            GameClientGameConfig = null;
        }
    }

    private void GameClientGameConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameClientGameConfig.UnsavedChanges))
            UnsavedChanges = GameClientGameConfig?.UnsavedChanges ?? false;
    }

    #endregion

    #region Protected Methods

    protected override async Task LoadAsync()
    {
        await SetSelectedGameClientAsync(null);

        var gameClients = Services.GameClients.GetInstalledGameClients().
            Where(x => x.GameClientDescriptor.SupportsGame(GameInstallation)).
            Select(x => new GameClientViewModel(x));
        GameClients = new ObservableCollection<GameClientViewModel>(gameClients);

        string? clientId = GameInstallation.GetValue<string>(GameDataKey.Client_SelectedClient);

        if (clientId != null)
            await SetSelectedGameClientAsync(GameClients.FirstOrDefault(x => x.GameClientInstallation.InstallationId == clientId));
    }

    protected override async Task<bool> SaveAsync()
    {
        if (GameClientGameConfig != null)
            return await GameClientGameConfig.SaveAsync();
        else
            return true;
    }

    protected override void UseRecommended() => GameClientGameConfig?.UseRecommended();

    #endregion

    #region Public Methods

    public Task ConfigureGameClientsAsync() => Services.UI.ShowGameClientsSetupAsync();

    public async void Receive(AddedGameClientsMessage message)
    {
        // Refresh if any added game clients support this game
        if (message.GameClientInstallations.Any(x => x.GameClientDescriptor.SupportsGame(GameInstallation)))
            await LoadPageAsync();
    }
    public async void Receive(RemovedGameClientsMessage message)
    {
        // Refresh if any removed game clients support this game
        if (message.GameClientInstallations.Any(x => x.GameClientDescriptor.SupportsGame(GameInstallation)))
            await LoadPageAsync();
    }
    public async void Receive(ModifiedGamesMessage message)
    {
        if (message.GameInstallations.Contains(GameInstallation))
        {
            string? clientId = GameInstallation.GetValue<string>(GameDataKey.Client_SelectedClient);

            if (clientId != SelectedGameClient?.GameClientInstallation.InstallationId)
                await SetSelectedGameClientAsync(GameClients?.FirstOrDefault(x => x.GameClientInstallation.InstallationId == clientId));
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        // Unregister for messages
        Services.Messenger.UnregisterAll(this);
    }

    #endregion

    #region Classes

    public class GameClientViewModel : BaseViewModel
    {
        public GameClientViewModel(GameClientInstallation gameClientInstallation)
        {
            GameClientInstallation = gameClientInstallation;
        }

        public GameClientInstallation GameClientInstallation { get; }
        public LocalizedString DisplayName => GameClientInstallation.GameClientDescriptor.DisplayName;
        public GameClientIconAsset Icon => GameClientInstallation.GameClientDescriptor.Icon;
    }

    #endregion
}