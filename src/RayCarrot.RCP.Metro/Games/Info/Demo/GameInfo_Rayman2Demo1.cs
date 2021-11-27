﻿#nullable disable
using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 Demo 1 game info
/// </summary>
public sealed class GameInfo_Rayman2Demo1 : GameInfo_BaseRayman2Demo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_Rayman2_1;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 2 Demo (1999/08/18)";

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_R2Demo1_Url),
    };

    #endregion
}