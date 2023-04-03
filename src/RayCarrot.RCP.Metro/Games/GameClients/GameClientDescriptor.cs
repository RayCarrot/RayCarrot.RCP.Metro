using RayCarrot.RCP.Metro.Games.Clients.DosBox.Data;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro.Games.Clients;

/// <summary>
/// A descriptor for defining a game client. This can be a client like Steam or an
/// emulator like DOSBox. Basically any program which handles launching a game.
/// </summary>
public abstract class GameClientDescriptor : IComparable<GameClientDescriptor>
{
    public abstract string GameClientId { get; }

    /// <summary>
    /// Indicates if the installation requires the location to have a file specified
    /// </summary>
    public virtual bool InstallationRequiresFile => false;

    /// <summary>
    /// The game client's display name
    /// </summary>
    public abstract LocalizedString DisplayName { get; }

    public abstract GameClientIconAsset Icon { get; }

    public virtual void RegisterComponents(IGameComponentBuilder builder)
    {
        builder.Register<OnGameRemovedComponent, DetachClientOnGameRemovedComponent>();
    }

    public virtual bool SupportsGame(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation)
    {
        RequiredGameInstallations? requiredGames = gameClientInstallation.GetObject<RequiredGameInstallations>(GameClientDataKey.RCP_RequiredGameInstallations);

        if (requiredGames != null && !requiredGames.GameInstallationIds.Contains(gameInstallation.InstallationId))
            return false;

        return true;
    }

    public virtual GameClientOptionsViewModel? GetGameClientOptionsViewModel(GameClientInstallation gameClientInstallation) => null;

    public virtual IEnumerable<DuoGridItemViewModel> GetGameClientInfoItems(GameClientInstallation gameClientInstallation) => new[]
    {
        new DuoGridItemViewModel(
            header: "Game client id:",
            text: GameClientId,
            minUserLevel: UserLevel.Debug),
        new DuoGridItemViewModel(
            header: "Installation id:",
            text: gameClientInstallation.InstallationId,
            minUserLevel: UserLevel.Debug),
        new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.GameInfo_InstallDir)),
            text: gameClientInstallation.InstallLocation.ToString()),
    };

    public virtual Task OnGameClientAddedAsync(GameClientInstallation gameClientInstallation) => Task.CompletedTask;

    /// <summary>
    /// Refreshes the games which use the specified game client
    /// </summary>
    /// <param name="gameClientInstallation">The game client to check for if the games use</param>
    public void RefreshGames(GameClientInstallation gameClientInstallation)
    {
        List<GameInstallation> gamesToRefresh = new();

        foreach (GameInstallation gameInstallation in Services.Games.GetInstalledGames())
        {
            if (gameInstallation.GetValue<string>(GameDataKey.Client_AttachedClient) == gameClientInstallation.InstallationId)
                gamesToRefresh.Add(gameInstallation);
        }

        Services.Messenger.Send(new ModifiedGamesMessage(gamesToRefresh));
    }

    /// <summary>
    /// Gets the queries to use when finding the game client
    /// </summary>
    /// <returns>The queries</returns>
    public virtual FinderQuery[] GetFinderQueries() => Array.Empty<FinderQuery>();

    /// <summary>
    /// Gets the finder item for this descriptor or null if there is none
    /// </summary>
    /// <returns>The finder item or null if there is none</returns>
    public GameClientFinderItem? GetFinderItem()
    {
        FinderQuery[] queries = GetFinderQueries();

        if (queries.Length == 0)
            return null;

        return new GameClientFinderItem(this, queries);
    }

    /// <summary>
    /// Indicates if the game client is valid
    /// </summary>
    /// <param name="installLocation">The game client install location</param>
    /// <returns>True if the game client is valid, otherwise false</returns>
    public bool IsValid(InstallLocation installLocation)
    {
        if (InstallationRequiresFile)
            return installLocation.HasFile && (installLocation.Directory + installLocation.FileName).FileExists;
        else
            return installLocation.Directory.DirectoryExists;
    }

    public int CompareTo(GameClientDescriptor? other)
    {
        if (this == other)
            return 0;
        if (other == null)
            return 1;

        // TODO: Ideally we'd add some proper sorting like for game descriptors, but it's harder here since a client
        //       can support multiple types of platforms at once, so what do we sort on?

        // Id
        return String.Compare(GameClientId, other.GameClientId, StringComparison.Ordinal);
    }
}