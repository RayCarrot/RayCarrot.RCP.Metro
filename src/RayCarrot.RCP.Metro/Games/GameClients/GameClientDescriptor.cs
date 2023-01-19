using RayCarrot.RCP.Metro.Games.Clients.DosBox.Data;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Clients;

/// <summary>
/// A descriptor for defining a game client. This can be a client like Steam or an
/// emulator like DOSBox. Basically any program which handles launching a game.
/// </summary>
public abstract class GameClientDescriptor : IComparable<GameClientDescriptor>
{
    public abstract string GameClientId { get; }

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
    /// Indicates if the game client is valid
    /// </summary>
    /// <param name="installLocation">The game client install location</param>
    /// <returns>True if the game client is valid, otherwise false</returns>
    public bool IsValid(InstallLocation installLocation)
    {
        // TODO-14: Improve this validation?
        if (installLocation.HasFile)
            return (installLocation.Directory + installLocation.FileName).FileExists;
        else
            return installLocation.Directory.DirectoryExists;
    }

    public int CompareTo(GameClientDescriptor? other)
    {
        if (this == other)
            return 0;
        if (other == null)
            return 1;

        // TODO-14: Add proper sorting based on platforms

        return String.Compare(GameClientId, other.GameClientId, StringComparison.Ordinal);
    }
}