#nullable disable
using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Demo (2003/01/08) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Demo_20030108_Win32 : GameDescriptor_BaseRayman3Demo
{
    #region Public Override Properties

    public override string Id => "Rayman3_Demo_20030108_Win32";
    public override Game Game => Game.Rayman3;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.Demo_Rayman3_5;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 3 Demo (2003/01/08)";

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_R3Demo5_Url),
    };

    #endregion
}