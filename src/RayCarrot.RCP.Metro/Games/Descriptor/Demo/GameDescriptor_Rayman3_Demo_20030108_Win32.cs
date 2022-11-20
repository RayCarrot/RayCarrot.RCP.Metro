﻿using System;
using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Demo 2003/01/08 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Demo_20030108_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "Rayman3_Demo_20030108_Win32";
    public override Game Game => Game.Rayman3;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games? LegacyGame => Games.Demo_Rayman3_5;

    public override string DisplayName => "Rayman 3 Demo (2003/01/08)";
    public override string DefaultFileName => "MainP5Pvf.exe";

    public override GameIconAsset Icon => GameIconAsset.Rayman3_Demo;

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_R3Demo5_Url),
    };

    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_Rayman3_ViewModel(gameInstallation);

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "R3_Setup_DX8D.exe")
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.Rayman3, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation,
            cpaTextureSyncData: null);

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        //@"Gamedatabin\tex16.cnt", // TODO-14: Why is this commented out?
        @"Gamedatabin\tex32.cnt",
        @"Gamedatabin\vignette.cnt",
    };

    #endregion
}