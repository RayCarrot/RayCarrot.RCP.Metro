using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 (GameCube) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_GameCube : GameCubeGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3_GameCube";
    public override Game Game => Game.Rayman3;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => "Rayman 3"; // TODO-LOC
    public override string[] SearchKeywords => new[] { "r3" };
    public override DateTime ReleaseDate => new(2003, 02, 21);

    public override GameIconAsset Icon => GameIconAsset.Rayman3;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman3;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman3_GameCube(x, "Rayman 3 - GameCube")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "r3_gc", "r3_gc"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.Rayman3_GC));
    }

    protected override ProgramInstallationStructure CreateStructure() => new GameCubeDiscProgramInstallationStructure(new[]
    {
        new GameCubeProgramLayout("EU", "RAYMAN 3 HOODLUM HAVOC", "GRHP", "41"),
        new GameCubeProgramLayout("US", "RAYMAN 3 HOODLUM HAVOC", "GRHE", "41"),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}