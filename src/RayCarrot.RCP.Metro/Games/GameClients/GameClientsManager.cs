using RayCarrot.RCP.Metro.Games.Clients.DosBox;
using RayCarrot.RCP.Metro.Games.Clients.Steam;

namespace RayCarrot.RCP.Metro.Games.Clients;

public class GameClientsManager
{
    #region Constructor

    public GameClientsManager(AppUserData data, IMessenger messenger)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        GameClientDescriptors = new GameClientDescriptor[]
        {
            new DosBoxGameClientDescriptor(),
            new SteamGameClientDescriptor(),
        }.ToDictionary(x => x.GameClientId);
        SortedGameClientDescriptors = GameClientDescriptors.Values.OrderBy(x => x).ToArray();
    }

    #endregion

    // TODO-14: Add logging

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Services

    private AppUserData Data { get; }
    private IMessenger Messenger { get; }

    #endregion

    #region Private Properties

    private Dictionary<string, GameClientDescriptor> GameClientDescriptors { get; }
    private GameClientDescriptor[] SortedGameClientDescriptors { get; }

    #endregion

    #region Game Client Descriptor Methods

    /// <summary>
    /// Gets the available game client descriptors
    /// </summary>
    /// <returns>The game client descriptors</returns>
    public IReadOnlyList<GameClientDescriptor> GetGameCientDescriptors() => SortedGameClientDescriptors;

    /// <summary>
    /// Gets a game client descriptor from the id
    /// </summary>
    /// <param name="gameClientId">The game client id</param>
    /// <returns>The matching game client descriptor</returns>
    public GameClientDescriptor GetGameClientDescriptor(string gameClientId)
    {
        if (gameClientId == null)
            throw new ArgumentNullException(nameof(gameClientId));

        if (!GameClientDescriptors.TryGetValue(gameClientId, out GameClientDescriptor descriptor))
            throw new ArgumentException($"No game client descriptor found for the provided game client id {gameClientId}", nameof(gameClientId));

        return descriptor;
    }

    public bool SupportsGameClient(GameInstallation gameInstallation) => 
        SortedGameClientDescriptors.Any(x => x.SupportsGame(gameInstallation));

    #endregion

    #region Game Client Installation Methods

    public async Task<GameClientInstallation> AddGameClientAsync(GameClientDescriptor descriptor, FileSystemPath installLocation)
    {
        GameClientInstallation installation = new(descriptor, installLocation);

        Data.Game_GameClientInstallations.AddSorted(installation);

        Messenger.Send(new AddedGameClientsMessage(installation));
        
        // Attempt to use this game client on games without one and which default to use one
        foreach (GameInstallation gameInstallation in Services.Games.GetInstalledGames())
        {
            if (gameInstallation.GameDescriptor.DefaultToUseGameClient &&
                descriptor.SupportsGame(gameInstallation) &&
                gameInstallation.GameDescriptor.GetAttachedGameClient(gameInstallation) == null)
            {
                await gameInstallation.GameDescriptor.AttachGameClientAsync(gameInstallation, installation);
            }
        }

        return installation;
    }

    public async Task RemoveGameClientAsync(GameClientInstallation gameClientInstallation)
    {
        Data.Game_GameClientInstallations.Remove(gameClientInstallation);

        // Deselect this game client from any games which use it
        foreach (GameInstallation gameInstallation in Services.Games.GetInstalledGames())
        {
            if (gameInstallation.GetValue<string>(GameDataKey.Client_AttachedClient) == gameClientInstallation.InstallationId)
            {
                if (gameInstallation.GameDescriptor.DefaultToUseGameClient)
                {
                    bool success = await gameInstallation.GameDescriptor.AttachDefaultGameClientAsync(gameInstallation);

                    if (!success)
                        await gameInstallation.GameDescriptor.DetachGameClientAsync(gameInstallation);
                }
                else
                {
                    await gameInstallation.GameDescriptor.DetachGameClientAsync(gameInstallation);
                }
            }
        }

        Messenger.Send(new RemovedGameClientsMessage(gameClientInstallation));
    }

    /// <summary>
    /// Gets a collection of the currently installed game clients
    /// </summary>
    /// <returns>The game client installations</returns>
    public IReadOnlyList<GameClientInstallation> GetInstalledGameClients()
    {
        // Copy to a list to avoid issues with it being modified when enumerating
        return Data.Game_GameClientInstallations.ToList();
    }

    /// <summary>
    /// Gets a game client installation from the installation id
    /// </summary>
    /// <param name="installationId">The game client installation id</param>
    /// <returns>The matching game client installation or null if not found</returns>
    public GameClientInstallation? GetInstalledGameClient(string installationId)
    {
        if (installationId == null)
            throw new ArgumentNullException(nameof(installationId));

        return Data.Game_GameClientInstallations.FirstOrDefault(x => x.InstallationId == installationId);
    }

    #endregion
}