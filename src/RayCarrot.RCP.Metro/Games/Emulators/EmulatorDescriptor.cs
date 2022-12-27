using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Emulators;

// TODO-14: Use components here as well
public abstract class EmulatorDescriptor : IComparable<EmulatorDescriptor>
{
    public abstract string EmulatorId { get; }

    /// <summary>
    /// The game platforms which this emulator supports
    /// </summary>
    public abstract GamePlatform[] SupportedPlatforms { get; }

    /// <summary>
    /// The emulator display name
    /// </summary>
    public abstract LocalizedString DisplayName { get; }

    public abstract EmulatorIconAsset Icon { get; }

    public virtual void RegisterComponents(IGameComponentBuilder builder)
    {
        builder.Register<OnGameRemovedComponent, DeselectClientOnGameRemovedComponent>();
    }

    public virtual EmulatorGameConfigViewModel? GetGameConfigViewModel(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) => null;

    public virtual EmulatorOptionsViewModel? GetEmulatorOptionsViewModel(EmulatorInstallation emulatorInstallation) => null;

    public virtual IEnumerable<DuoGridItemViewModel> GetEmulatorInfoItems(EmulatorInstallation emulatorInstallation) => new[]
    {
        new DuoGridItemViewModel(
            header: "Emulator id:",
            text: EmulatorId,
            minUserLevel: UserLevel.Debug),
        new DuoGridItemViewModel(
            header: "Installation id:",
            text: emulatorInstallation.InstallationId,
            minUserLevel: UserLevel.Debug),
        new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.GameInfo_InstallDir)),
            text: emulatorInstallation.InstallLocation.FullPath),
    };

    public virtual Task OnEmulatorSelectedAsync(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) => 
        Task.CompletedTask;

    public virtual Task OnEmulatorDeselectedAsync(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) => 
        Task.CompletedTask;

    public void RefreshEmulatedGames(EmulatorInstallation emulatorInstallation)
    {
        List<GameInstallation> gamesToRefresh = new();

        foreach (GameInstallation gameInstallation in Services.Games.GetInstalledGames())
        {
            if (gameInstallation.GetValue<string>(GameDataKey.Client_SelectedClient) == emulatorInstallation.InstallationId)
                gamesToRefresh.Add(gameInstallation);
        }

        Services.Messenger.Send(new ModifiedGamesMessage(gamesToRefresh));
    }

    public int CompareTo(EmulatorDescriptor? other)
    {
        if (this == other)
            return 0;
        if (other == null)
            return 1;

        // TODO-14: Add proper sorting based on platforms

        return String.Compare(EmulatorId, other.EmulatorId, StringComparison.Ordinal);
    }
}