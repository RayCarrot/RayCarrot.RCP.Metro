using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M (PS2) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanM_Ps2 : Ps2GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanM_Ps2";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => "Rayman M"; // TODO-LOC
    public override string[] SearchKeywords => new[] { "rm" };
    public override DateTime ReleaseDate => new(2001, 11, 30);

    public override GameIconAsset Icon => GameIconAsset.RaymanM;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanM;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanMArena_Ps2(x, "Rayman M - PS2")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "rm_ps2", "rm_ps2"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.RaymanM_PS2));
    }

    protected override ProgramInstallationStructure CreateStructure() => new Ps2DiscProgramInstallationStructure(new[]
    {
        new Ps2DiscProgramLayout("EU", "BE", "SLES-50457", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_504.57;1", ProgramPathType.PrimaryExe, required: true),
        })),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}