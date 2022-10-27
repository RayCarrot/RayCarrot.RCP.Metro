#nullable disable
using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;
// IDEA: Add backup info

/// <summary>
/// The Rayman Raving Rabbids Activity Center game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbidsActivityCenter_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "RaymanRavingRabbidsActivityCenter_Win32";
    public override Game Game => Game.RaymanRavingRabbidsActivityCenter;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanRavingRabbidsActivityCenter;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Other;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Raving Rabbids Activity Center";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => null;

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman.exe";

    /// <summary>
    /// Indicates if the game can be located. If set to false the game is required to be downloadable.
    /// </summary>
    public override bool CanBeLocated => false;

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => true;

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_RavingRabbidsActivityCenter_Url)
    };

    #endregion
}