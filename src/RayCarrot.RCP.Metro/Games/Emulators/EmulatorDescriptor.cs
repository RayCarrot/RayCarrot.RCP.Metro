﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro.Games.Emulators;

public abstract class EmulatorDescriptor
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

    public virtual GameOptionsDialog_EmulatorConfigPageViewModel? GetGameConfigViewModel(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) => null;

    public abstract Task<bool> LaunchGameAsync(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation);

    public virtual IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) =>
        Enumerable.Empty<DuoGridItemViewModel>();

    public abstract void CreateGameShortcut(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation, FileSystemPath shortcutName, FileSystemPath destinationDirectory);

    public abstract IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation);

    // TODO-14: Call this
    public virtual Task OnEmulatorSelected(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) => Task.CompletedTask;

    // TODO-14: Call this
    public virtual Task OnEmulatorDeselected(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) => Task.CompletedTask;
}