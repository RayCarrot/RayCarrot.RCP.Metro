using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 Demo 1999/08/18 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Demo_19990818_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "Rayman2_Demo_19990818_Win32";
    public override Game Game => Game.Rayman2;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games? LegacyGame => Games.Demo_Rayman2_1;

    public override string DisplayName => "Rayman 2 Demo (1999/08/18)";
    public override string DefaultFileName => "Rayman2Demo.exe";

    public override GameIconAsset Icon => GameIconAsset.Rayman2_Demo;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman2;

    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_R2Demo1_Url),
        })
    });

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.Rayman2Demo, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation, 
            cpaTextureSyncData: null);

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"BinData\Textures.cnt",
    };

    #endregion
}