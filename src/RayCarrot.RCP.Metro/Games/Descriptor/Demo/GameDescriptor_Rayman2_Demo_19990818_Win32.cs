using System;
using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 Demo 1999/08/18 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Demo_19990818_Win32 : Win32GameDescriptor
{
    #region Protected Properties

    protected override string IconName => "Rayman2Demo";

    #endregion

    #region Public Properties

    public override string Id => "Rayman2_Demo_19990818_Win32";
    public override Game Game => Game.Rayman2;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games LegacyGame => Games.Demo_Rayman2_1;

    public override string DisplayName => "Rayman 2 Demo (1999/08/18)";
    public override string DefaultFileName => "Rayman2Demo.exe";

    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_R2Demo1_Url),
    };

    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(new OpenSpaceSettings(EngineVersion.Rayman2Demo, BinarySerializer.OpenSpace.Platform.PC), gameInstallation);

    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new[]
    {
        installDir + "BinData" + "Textures.cnt",
    };

    #endregion
}