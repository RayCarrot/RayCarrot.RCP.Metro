using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for an emulated game
/// </summary>
public abstract class EmulatedGameDescriptor : GameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the emulator installation associated with this game installation or null if none was found
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the emulator for</param>
    /// <returns>The associated emulator installation or null if none was found</returns>
    private EmulatorInstallation? GetEmulator(GameInstallation gameInstallation)
    {
        string? emuId = gameInstallation.GetValue<string>(GameDataKey.EmulatorInstallationId);

        if (emuId == null)
            return null;

        return Services.Games.GetEmulatorInstallation(emuId);
    }

    #endregion

    #region Protected Methods

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

    public override async Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // TODO-14: Set default emu
    }

    public override async Task PostGameRemovedAsync(GameInstallation gameInstallation)
    {
        // TODO-14: Remove emu
    }

    #endregion
}