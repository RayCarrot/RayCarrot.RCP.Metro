using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Clients;

// TODO-14: Use components here as well
public abstract class GameClientDescriptor : IComparable<GameClientDescriptor>
{
    public abstract string GameClientId { get; }

    /// <summary>
    /// The game platforms which this emulator supports
    /// </summary>
    public abstract GamePlatform[] SupportedPlatforms { get; }

    /// <summary>
    /// The game client's display name
    /// </summary>
    public abstract LocalizedString DisplayName { get; }

    public abstract GameClientIconAsset Icon { get; }

    public virtual void RegisterComponents(IGameComponentBuilder builder)
    {
        builder.Register<OnGameRemovedComponent, DeselectClientOnGameRemovedComponent>();
    }

    public virtual GameClientGameConfigViewModel? GetGameConfigViewModel(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) => null;

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

    public virtual Task OnGameClientSelectedAsync(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) => 
        Task.CompletedTask;

    public virtual Task OnGameClientDeselectedAsync(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) => 
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
            if (gameInstallation.GetValue<string>(GameDataKey.Client_SelectedClient) == gameClientInstallation.InstallationId)
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