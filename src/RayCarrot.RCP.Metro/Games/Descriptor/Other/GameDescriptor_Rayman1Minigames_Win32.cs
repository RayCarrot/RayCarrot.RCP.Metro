﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 minigames (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1Minigames_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "Rayman1Minigames_Win32";
    public override Game Game => Game.Rayman1Minigames;
    public override GameCategory Category => GameCategory.Other;
    public override Games? LegacyGame => Games.Ray1Minigames;

    public override string DisplayName => "Rayman 1 Minigames";
    public override string DefaultFileName => "RayGames.exe";

    public override bool CanBeLocated => false;
    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_Ray1Minigames_Url),
    };

    #endregion

    #region Public Methods

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_Ray1Minigames_Controls(new GameOptions_Ray1Minigames_ViewModel(gameInstallation));

    public override Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Default to run as admin
        gameInstallation.SetValue(GameDataKey.Win32LaunchMode, UserData_GameLaunchMode.AsAdmin);

        // Call base and return
        return base.PostGameAddAsync(gameInstallation);
    }

    #endregion
}