#nullable disable
using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 Demo (1999/09/04) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Demo_19990904_Win32 : GameDescriptor_BaseRayman2Demo
{
    #region Public Override Properties

    public override string Id => "Rayman2_Demo_19990904_Win32";
    public override Game Game => Game.Rayman2;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.Demo_Rayman2_2;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 2 Demo (1999/09/04)";

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_R2Demo2_Url),
    };

    #endregion
}