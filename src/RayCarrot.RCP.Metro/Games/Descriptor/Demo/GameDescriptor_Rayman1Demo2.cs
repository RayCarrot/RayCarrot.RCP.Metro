#nullable disable
using System;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 2 game info
/// </summary>
public sealed class GameDescriptor_Rayman1Demo2 : GameDescriptor
{
    #region Protected Override Properties

    protected override string IconName => $"Rayman1Demo";

    #endregion

    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_Rayman1_2;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Demo;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 1 Demo (1996/02/15)";

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
        new Uri(AppURLs.Games_R1Demo2_Url),
    };

    /// <summary>
    /// The type of game if it can be downloaded
    /// </summary>
    public override GameType DownloadType => GameType.DosBox;

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_Rayman1_ViewModel(gameInstallation);

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => new GameOptions_DOSBox_Control(Game);

    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public override Emulator Emulator => new Emulator_DOSBox(Game, GameType.DosBox);

    #endregion
}