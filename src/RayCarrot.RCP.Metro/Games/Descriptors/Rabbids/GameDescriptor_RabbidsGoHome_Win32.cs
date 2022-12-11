﻿using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Go Home (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RabbidsGoHome_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string GameId => "RabbidsGoHome_Win32";
    public override Game Game => Game.RabbidsGoHome;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RabbidsGoHome;

    public override string DisplayName => "Rabbids Go Home";
    public override string DefaultFileName => Services.Data.Game_RabbidsGoHomeLaunchData == null ? "Launcher.exe" : "LyN_f.exe";
    public override DateTime ReleaseDate => new(2009, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RabbidsGoHome;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameConfigComponent(x => new RabbidsGoHomeConfigViewModel()));
    }

    protected override string? GetLaunchArgs(GameInstallation gameInstallation) =>
        Services.Data.Game_RabbidsGoHomeLaunchData?.ToString();

    #endregion

    #region Public Methods

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rabbids Go Home", new[]
    {
        "Rabbids Go Home",
        "Rabbids Go Home - DVD",
        "Rabbids: Go Home",
    });

    public override Task PostGameRemovedAsync(GameInstallation gameInstallation)
    {
        // Remove the game specific data
        Services.Data.Game_RabbidsGoHomeLaunchData = null;

        return base.PostGameRemovedAsync(gameInstallation);
    }

    #endregion
}