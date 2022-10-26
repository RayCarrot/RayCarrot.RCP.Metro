﻿#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Jungle Run game info
/// </summary>
public sealed class GameDescriptor_RaymanJungleRun : GameDescriptor
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanJungleRun;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Jungle Run";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Jungle Run";

    /// <summary>
    /// Gets the default file name for launching the game, if available
    /// </summary>
    public override string DefaultFileName => "RO1Mobile.exe";

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanJungleRun_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanJungleRun(gameInstallation).Yield();

    public override bool AllowPatching => false;

    #endregion
}