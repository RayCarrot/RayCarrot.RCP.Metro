using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 1995/12/04 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Demo_19951204_MSDOS : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Demo_19951204_MSDOS";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;
    public override bool IsDemo => true;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Demo_Rayman1_3;

    public override LocalizedString DisplayName => "Rayman Demo (1995/12/04)";
    public override DateTime ReleaseDate => new(1995, 12, 04);

    public override GameIconAsset Icon => GameIconAsset.Rayman1_Demo;

    #endregion

    #region Protected Methods

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("RAYMAN.EXE", GameInstallationPathType.PrimaryExe, required: true),

        // Directories
        new GameInstallationDirectoryPath("PCMAP", GameInstallationPathType.Data, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_R1Demo3_Url),
        })
    });

    #endregion
}