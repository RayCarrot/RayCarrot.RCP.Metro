﻿#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Big Bang game info
/// </summary>
public sealed class GameDescriptor_RabbidsBigBang : GameDescriptor
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RabbidsBigBang;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rabbids;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rabbids Big Bang";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rabbids Big Bang";

    /// <summary>
    /// Gets the default file name for launching the game, if available
    /// </summary>
    public override string DefaultFileName => "Template.exe";

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RabbidsBigBang(gameInstallation).Yield();

    public override bool AllowPatching => false;

    #endregion
}