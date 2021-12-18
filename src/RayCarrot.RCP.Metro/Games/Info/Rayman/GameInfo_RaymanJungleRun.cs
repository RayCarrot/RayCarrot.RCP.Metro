﻿#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Jungle Run game info
/// </summary>
public sealed class GameInfo_RaymanJungleRun : GameInfo
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

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_RaymanJungleRun_ViewModel();

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels => new ProgressionGameViewModel_RaymanJungleRun().Yield();

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

    #endregion
}