﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Go Home game info
/// </summary>
public sealed class GameDescriptor_RabbidsGoHome : GameDescriptor
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RabbidsGoHome;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rabbids;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rabbids Go Home";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rabbids Go Home";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => Services.Data.Game_RabbidsGoHomeLaunchData == null ? "Launcher.exe" : "LyN_f.exe";

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RabbidsGoHome_ViewModel();

    #endregion
}