using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Clients;

// TODO-14: Move methods and such to components
/// <summary>
/// A descriptor for defining a game client. This can be a client like Steam or an
/// emulator like DOSBox. Basically any program which handles launching a game.
/// </summary>
public abstract class GameClientDescriptor : IComparable<GameClientDescriptor>
{
    public abstract string GameClientId { get; }

    // TODO-14: Allow user to rename this like we did with game installations
    /// <summary>
    /// The game client's display name
    /// </summary>
    public abstract LocalizedString DisplayName { get; }

    public abstract GameClientIconAsset Icon { get; }

    public virtual void RegisterComponents(IGameComponentBuilder builder)
    {
        builder.Register<OnGameRemovedComponent, DeselectClientOnGameRemovedComponent>();
    }

    public abstract bool SupportsGame(GameInstallation gameInstallation);

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
            text: gameClientInstallation.InstallLocation.FullPath),
    };

    public virtual Task OnGameClientAttachedAsync(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) => 
        Task.CompletedTask;

    public virtual Task OnGameClientDetachedAsync(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) => 
        Task.CompletedTask;

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