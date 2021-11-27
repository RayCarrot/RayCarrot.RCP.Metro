#nullable disable
using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Demo 2 game info
/// </summary>
public sealed class GameInfo_Rayman3Demo2 : GameInfo_BaseRayman3Demo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_Rayman3_2;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 3 Demo (2002/10/21)";

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_R3Demo2_Url),
    };

    #endregion
}