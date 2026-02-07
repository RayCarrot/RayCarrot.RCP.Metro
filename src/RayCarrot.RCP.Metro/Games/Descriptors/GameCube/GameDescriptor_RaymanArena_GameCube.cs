using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Arena (GameCube) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanArena_GameCube : GameCubeGameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanArena_GameCube";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => "Rayman Arena"; // TODO-LOC
    public override string[] SearchKeywords => new[] { "ra" };
    public override DateTime ReleaseDate => new(2002, 09, 24);

    public override GameIconAsset Icon => GameIconAsset.RaymanArena;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanM;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.RaymanArena_GC));
    }

    protected override ProgramInstallationStructure CreateStructure() => new GameCubeDiscProgramInstallationStructure(new[]
    {
        new GameCubeProgramLayout("US", "RAYMAN ARENA", "GRYE", "41"),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}