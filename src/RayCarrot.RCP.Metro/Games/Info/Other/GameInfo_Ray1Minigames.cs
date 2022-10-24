#nullable disable
using System;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 minigames game info
/// </summary>
public sealed class GameInfo_Ray1Minigames : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Ray1Minigames;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Other;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 1 Minigames";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => null;

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "RayGames.exe";

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
        new Uri(AppURLs.Games_Ray1Minigames_Url),
    };

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => 
        new GameOptions_Ray1Minigames_Controls(new GameOptions_Ray1Minigames_ViewModel(gameInstallation));

    #endregion
}