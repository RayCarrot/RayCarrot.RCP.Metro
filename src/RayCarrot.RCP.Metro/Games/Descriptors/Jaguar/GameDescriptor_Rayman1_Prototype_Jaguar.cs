using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Prototype (Jaguar) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Prototype_Jaguar : JaguarGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Prototype_Jaguar";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;
    public override GameType Type => GameType.Prototype;

    public override LocalizedString DisplayName => "Rayman Prototype"; // TODO-LOC
    public override DateTime ReleaseDate => new(1994, 01, 01); // Unknown

    public override GameIconAsset Icon => GameIconAsset.Rayman1_Demo;
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