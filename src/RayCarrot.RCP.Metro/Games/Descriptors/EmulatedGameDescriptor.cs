using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Emulators;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for an emulated game
/// </summary>
public abstract class EmulatedGameDescriptor : GameDescriptor
{
    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(
            // Emulator config page
            new GameOptionsDialogPageComponent(
                objFactory: x => new EmulatorGameConfigPageViewModel(x, this),
                isAvailableFunc: _ => true,
                priority: GameOptionsDialogPageComponent.PagePriority.Normal));
    }

    protected override async Task<bool> LaunchAsync(GameInstallation gameInstallation)
    {
        EmulatorInstallation? emu = GetEmulator(gameInstallation);

        if (emu == null)
        {
            // TODO-UPDATE: Show error message
            return false;
        }

        return await emu.EmulatorDescriptor.LaunchGameAsync(gameInstallation, emu);
    }

    #endregion

    #region Public Methods

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        EmulatorInstallation? emu = GetEmulator(gameInstallation);

        if (emu == null)
            return base.GetGameInfoItems(gameInstallation);

        return base.GetGameInfoItems(gameInstallation).Concat(emu.EmulatorDescriptor.GetGameInfoItems(gameInstallation, emu));
    }

    public override void CreateGameShortcut(GameInstallation gameInstallation, FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        EmulatorInstallation? emu = GetEmulator(gameInstallation);

        if (emu == null)
            // TODO-UPDATE: Show error message
            return;

        emu.EmulatorDescriptor.CreateGameShortcut(gameInstallation, emu, shortcutName, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation)
    {
        EmulatorInstallation? emu = GetEmulator(gameInstallation);

        if (emu == null)
            return Enumerable.Empty<JumpListItemViewModel>();

        return emu.EmulatorDescriptor.GetJumpListItems(gameInstallation, emu);
    }

    public override Task PostGameAddAsync(GameInstallation gameInstallation) =>
        SetEmulatorAsync(gameInstallation, null);

    public override async Task PostGameRemovedAsync(GameInstallation gameInstallation)
    {
        await base.PostGameRemovedAsync(gameInstallation);

        // Get the previous emulator installation and invoke it being deselected
        EmulatorInstallation? prevEmulatorInstallation = GetEmulator(gameInstallation);
        if (prevEmulatorInstallation != null)
            await prevEmulatorInstallation.EmulatorDescriptor.OnEmulatorDeselectedAsync(gameInstallation, prevEmulatorInstallation);
    }

    /// <summary>
    /// Gets the emulator installation associated with this game installation or null if none was found
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the emulator for</param>
    /// <returns>The associated emulator installation or null if none was found</returns>
    public EmulatorInstallation? GetEmulator(GameInstallation gameInstallation)
    {
        string? emuId = gameInstallation.GetValue<string>(GameDataKey.Emu_InstallationId);

        if (emuId == null)
            return null;

        return Services.Emulators.GetInstalledEmulator(emuId);
    }

    public async Task SetEmulatorAsync(GameInstallation gameInstallation, EmulatorInstallation? emulatorInstallation)
    {
        // Get the previous emulator installation and invoke it being deselected
        EmulatorInstallation? prevEmulatorInstallation = GetEmulator(gameInstallation);
        if (prevEmulatorInstallation != null)
            await prevEmulatorInstallation.EmulatorDescriptor.OnEmulatorDeselectedAsync(gameInstallation, prevEmulatorInstallation);
        
        // If the provided installation is null we attempt to find the first available emulator to use
        emulatorInstallation ??= Services.Emulators.GetInstalledEmulators()
            .FirstOrDefault(x => x.EmulatorDescriptor.SupportedPlatforms.Contains(Platform));

        // Set the emulator for the game
        gameInstallation.SetValue(GameDataKey.Emu_InstallationId, emulatorInstallation?.InstallationId);

        // Invoke the new emulator being selected
        if (emulatorInstallation != null)
            await emulatorInstallation.EmulatorDescriptor.OnEmulatorSelectedAsync(gameInstallation, emulatorInstallation);

        // Refresh the game
        Services.Messenger.Send(new ModifiedGamesMessage(gameInstallation));
    }

    #endregion
}