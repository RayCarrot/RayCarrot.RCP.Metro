﻿using System;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo (1995/12/04) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Demo_19951204_MSDOS : MSDOSGameDescriptor
{
    #region Protected Override Properties

    public override string Id => "Rayman1_Demo_19951204_MSDOS";
    public override Game Game => Game.Rayman1;

    protected override string IconName => $"Rayman1Demo";

    #endregion

    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.Demo_Rayman1_3;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Demo;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 1 Demo (1995/12/04)";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "RAYMAN.EXE";

    /// <summary>
    /// Indicates if the game is a demo
    /// </summary>
    public override bool IsDemo => true;

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => true;

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_R1Demo3_Url),
    };

    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public override Emulator Emulator => new Emulator_DOSBox();

    #endregion
}