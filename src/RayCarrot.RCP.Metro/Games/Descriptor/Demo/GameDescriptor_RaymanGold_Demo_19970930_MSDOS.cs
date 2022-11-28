using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Gold Demo 1997/09/30 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanGold_Demo_19970930_MSDOS : MSDOSGameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanGold_Demo_19970930_MSDOS";
    public override Game Game => Game.RaymanDesigner;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games? LegacyGame => Games.Demo_RaymanGold;

    public override string DisplayName => "Rayman Gold Demo (1997/09/30)";
    public override string DefaultFileName => "Rayman.bat";

    public override GameIconAsset Icon => GameIconAsset.RaymanGold_Demo;
    
    public override bool HasArchives => true;

    public override bool RequiresMounting => false;
    public override string ExecutableName => "RAYKIT.EXE";

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_RGoldDemo_Url),
        })
    };

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_RaymanDesigner_ViewModel(this, gameInstallation);

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Kit));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"PCMAP\COMMON.DAT",
        @"PCMAP\SNDD8B.DAT",
        @"PCMAP\SNDH8B.DAT",
        @"PCMAP\VIGNET.DAT",

        @"PCMAP\AL\SNDSMP.DAT",
        @"PCMAP\AL\SPECIAL.DAT",

        @"PCMAP\FR\SNDSMP.DAT",
        @"PCMAP\FR\SPECIAL.DAT",

        @"PCMAP\USA\SNDSMP.DAT",
        @"PCMAP\USA\SPECIAL.DAT",
    };

    #endregion
}