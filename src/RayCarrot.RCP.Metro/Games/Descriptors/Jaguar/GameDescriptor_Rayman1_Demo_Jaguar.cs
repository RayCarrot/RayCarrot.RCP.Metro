using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo (Jaguar) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Demo_Jaguar : JaguarGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Demo_Jaguar";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;
    public override GameType Type => GameType.Demo;

    public override LocalizedString DisplayName => "Rayman Demo"; // TODO-LOC
    public override DateTime ReleaseDate => new(1995, 01, 01); // Unknown

    public override GameIconAsset Icon => GameIconAsset.Rayman1Jaguar;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Protected Methods

    protected override ProgramInstallationStructure CreateStructure() => new JaguarRomProgramInstallationStructure();

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}