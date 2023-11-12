using System.Diagnostics.CodeAnalysis;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Options;

/// <summary>
/// View model for the client selection game options
/// </summary>
public class GameClientSelectionGameOptionsViewModel : GameOptionsViewModel, IInitializable,
    IRecipient<AddedGameClientsMessage>, IRecipient<RemovedGameClientsMessage>, IRecipient<ModifiedGameClientsMessage>,
    IRecipient<SortedGameClientsMessage>
{
    #region Constructor

    public GameClientSelectionGameOptionsViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    #endregion

    #region Private Fields

    private GameClientViewModel? _selectedGameClient;

    #endregion

    #region Public Properties

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
                if (value?.GameClientInstallation == null)
                    await Services.GameClients.DetachGameClientAsync(GameInstallation);
                else
                    await Services.GameClients.AttachGameClientAsync(GameInstallation, value.GameClientInstallation);
            }
        }
    }

    #endregion

    #region Private Methods

    private void SetSelectedGameClient(GameClientViewModel? gameClient)
    {
        _selectedGameClient = gameClient;
        OnPropertyChanged(nameof(SelectedGameClient));
    }

    private void Load()
    {
        SetSelectedGameClient(null);

        var gameClients = Services.GameClients.GetInstalledGameClients().
            Where(x => x.GameClientDescriptor.SupportsGame(GameInstallation, x)).
            Select(x => new GameClientViewModel(x));
        GameClients = new ObservableCollection<GameClientViewModel>(gameClients);
        GameClients.Insert(0, new GameClientViewModel(null));

        string? clientId = GameInstallation.GetValue<string>(GameDataKey.Client_AttachedClient);

        if (clientId != null)
            SetSelectedGameClient(GameClients.FirstOrDefault(x =>
                x.GameClientInstallation?.InstallationId == clientId));
        else
            SetSelectedGameClient(GameClients.First());
    }

    #endregion

    #region Public Methods

    public void Initialize()
    {
        Load();
        Services.Messenger.RegisterAll(this);
    }

    public void Deinitialize()
    {
        Services.Messenger.UnregisterAll(this);
    }

    #endregion

    #region Message Receivers

    void IRecipient<AddedGameClientsMessage>.Receive(AddedGameClientsMessage message)
    {
        // Refresh if any added game clients support this game
        if (message.GameClientInstallations.Any(x => x.GameClientDescriptor.SupportsGame(GameInstallation, x)))
            Load();
    }
    void IRecipient<RemovedGameClientsMessage>.Receive(RemovedGameClientsMessage message)
    {
        // Refresh if any removed game clients support this game
        if (message.GameClientInstallations.Any(x => x.GameClientDescriptor.SupportsGame(GameInstallation, x)))
            Load();
    }
    void IRecipient<ModifiedGameClientsMessage>.Receive(ModifiedGameClientsMessage message)
    {
        if (GameClients == null)
            return;

        foreach (GameClientViewModel gameClientViewModel in GameClients)
        {
            if (gameClientViewModel.GameClientInstallation != null &&
                message.GameClientInstallations.Contains(gameClientViewModel.GameClientInstallation))
            {
                gameClientViewModel.Refresh();
            }
        }
    }
    void IRecipient<SortedGameClientsMessage>.Receive(SortedGameClientsMessage message) => Load();

    #endregion

    #region Classes

    public class GameClientViewModel : BaseViewModel
    {
        public GameClientViewModel(GameClientInstallation? gameClientInstallation)
        {
            GameClientInstallation = gameClientInstallation;
            Refresh();
        }

        public GameClientInstallation? GameClientInstallation { get; }
        public LocalizedString DisplayName { get; private set; }
        public GameClientIconAsset? Icon => GameClientInstallation?.GameClientDescriptor.Icon;

        [MemberNotNull(nameof(DisplayName))]
        public void Refresh() => DisplayName = GameClientInstallation?.GetDisplayName() ??
                                               new ResourceLocString(nameof(Resources.GameClients_SelectionNone));
    }

    #endregion
}