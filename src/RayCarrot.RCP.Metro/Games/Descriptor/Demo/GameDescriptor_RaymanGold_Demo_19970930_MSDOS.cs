﻿using System;
using System.Collections.Generic;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Gold Demo 1997/09/30 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanGold_Demo_19970930_MSDOS : MSDOSGameDescriptor
{
    #region Protected Properties

    protected override string IconName => "RaymanGoldDemo";

    #endregion

    #region Public Properties

    public override string Id => "RaymanGold_Demo_19970930_MSDOS";
    public override Game Game => Game.RaymanDesigner;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games? LegacyGame => Games.Demo_RaymanGold;

    public override string DisplayName => "Rayman Gold Demo (1997/09/30)";
    public override string DefaultFileName => "Rayman.bat";

    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_RGoldDemo_Url),
    };

    public override bool HasArchives => true;

    public override bool RequiresMounting => false;
    public override string ExecutableName => "RAYKIT.EXE";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_RaymanDesigner_ViewModel(gameInstallation);

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Kit));

    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => 
        Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

    #endregion
}