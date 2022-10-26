#nullable disable
using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Demo 4 game info
/// </summary>
public sealed class GameDescriptor_Rayman3Demo4 : GameDescriptor_BaseRayman3Demo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_Rayman3_4;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 3 Demo (2003/01/29)";

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_R3Demo4_Url),
    };

    #endregion
}