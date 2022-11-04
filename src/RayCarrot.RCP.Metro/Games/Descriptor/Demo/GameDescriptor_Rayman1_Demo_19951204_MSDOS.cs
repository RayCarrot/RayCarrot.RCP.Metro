using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 1995/12/04 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Demo_19951204_MSDOS : MSDOSGameDescriptor
{
    #region Protected Properties

    protected override string IconName => "Rayman1Demo";

    #endregion

    #region Public Properties

    public override string Id => "Rayman1_Demo_19951204_MSDOS";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games? LegacyGame => Games.Demo_Rayman1_3;

    public override string DisplayName => "Rayman 1 Demo (1995/12/04)";
    public override string DefaultFileName => "RAYMAN.EXE";

    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_R1Demo3_Url),
    };

    public override bool RequiresMounting => false;
    public override string ExecutableName => "RAYMAN.EXE";

    #endregion
}