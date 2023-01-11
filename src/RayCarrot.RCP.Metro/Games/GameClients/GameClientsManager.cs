using RayCarrot.RCP.Metro.Games.Clients.DosBox;
using RayCarrot.RCP.Metro.Games.Clients.Steam;

namespace RayCarrot.RCP.Metro.Games.Clients;

public class GameClientsManager
{
    #region Constructor

    public GameClientsManager(AppUserData data, IMessenger messenger, GamesManager gamesManager)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));

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
    private GamesManager GamesManager { get; }

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
        foreach (GameInstallation gameInstallation in GamesManager.GetInstalledGames())
        {
            if (gameInstallation.GameDescriptor.DefaultToUseGameClient &&
                descriptor.SupportsGame(gameInstallation) &&
                GetAttachedGameClient(gameInstallation) == null)
            {
                await AttachGameClientAsync(gameInstallation, installation);
            }
        }

        return installation;
    }

    public async Task RemoveGameClientAsync(GameClientInstallation gameClientInstallation)
    {
        Data.Game_GameClientInstallations.Remove(gameClientInstallation);

        // Deselect this game client from any games which use it
        foreach (GameInstallation gameInstallation in GamesManager.GetInstalledGames())
        {
            if (gameInstallation.GetValue<string>(GameDataKey.Client_AttachedClient) == gameClientInstallation.InstallationId)
            {
                if (gameInstallation.GameDescriptor.DefaultToUseGameClient)
                {
                    bool success = await AttachDefaultGameClientAsync(gameInstallation);

                    if (!success)
                        await DetachGameClientAsync(gameInstallation);
                }
                else
                {
                    await DetachGameClientAsync(gameInstallation);
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

    #region Game Methods

    /// <summary>
    /// Gets the game client installation attached to the game installation, or null if there is none
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the attached client for</param>
    /// <returns>The attached client installation or null if none was found</returns>
    public GameClientInstallation? GetAttachedGameClient(GameInstallation gameInstallation)
    {
        string? clientInstallationId = gameInstallation.GetValue<string>(GameDataKey.Client_AttachedClient);

        if (clientInstallationId == null)
            return null;

        return GetInstalledGameClient(clientInstallationId);
    }

    /// <summary>
    /// Gets the game client installation attached to the game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the attached client for</param>
    /// <returns>The attached client installation</returns>
    public GameClientInstallation GetRequiredAttachedGameClient(GameInstallation gameInstallation) =>
        GetAttachedGameClient(gameInstallation) ?? throw new Exception("The game does not have an attached game client");

    /// <summary>
    /// Detaches any currently attached game client from the game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to detach the game client for</param>
    public async Task DetachGameClientAsync(GameInstallation gameInstallation)
    {
        // Get the previous client installation and invoke it being detached
        GameClientInstallation? prevClient = GetAttachedGameClient(gameInstallation);
        if (prevClient != null)
            await prevClient.GameClientDescriptor.OnGameClientDetachedAsync(gameInstallation, prevClient);

        // Detach the client for the game
        gameInstallation.SetValue<string?>(GameDataKey.Client_AttachedClient, null);

        // Rebuild the game components since the client change might change which components get registered
        gameInstallation.RebuildComponents();

        // Refresh the game
        Messenger.Send(new ModifiedGamesMessage(gameInstallation));
    }

    /// <summary>
    /// Attaches the first available game client to the game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to attach the game client to</param>
    /// <returns>True if there was a game client to attach, otherwise false</returns>
    public async Task<bool> AttachDefaultGameClientAsync(GameInstallation gameInstallation)
    {
        // Get the first available game client
        GameClientInstallation? gameClientInstallation = GetInstalledGameClients().
            FirstOrDefault(x => x.GameClientDescriptor.SupportsGame(gameInstallation));

        if (gameClientInstallation == null)
            return false;

        await AttachGameClientAsync(gameInstallation, gameClientInstallation);
        return true;
    }

    /// <summary>
    /// Attaches the specified game client to the game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to attach the game client to</param>
    /// <param name="gameClientInstallation">The game client to attach</param>
    public async Task AttachGameClientAsync(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation)
    {
        if (gameInstallation == null)
            throw new ArgumentNullException(nameof(gameInstallation));
        if (gameClientInstallation == null)
            throw new ArgumentNullException(nameof(gameClientInstallation));

        // Get the previous client installation and invoke it being detached
        GameClientInstallation? prevClient = GetAttachedGameClient(gameInstallation);
        if (prevClient != null)
            await prevClient.GameClientDescriptor.OnGameClientDetachedAsync(gameInstallation, prevClient);

        // Set the client for the game
        gameInstallation.SetValue(GameDataKey.Client_AttachedClient, gameClientInstallation.InstallationId);

        // Rebuild the game components since the client change might change which components get registered
        gameInstallation.RebuildComponents();

        // Invoke the new client being selected
        await gameClientInstallation.GameClientDescriptor.OnGameClientAttachedAsync(gameInstallation, gameClientInstallation);

        // Refresh the game
        Messenger.Send(new ModifiedGamesMessage(gameInstallation));
    }

    #endregion
}