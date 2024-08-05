using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Arena (PS2) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanArena_Ps2 : Ps2GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanArena_Ps2";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.RaymanArena_Ps2_Title));
    public override string[] SearchKeywords => new[] { "ra" };
    public override DateTime ReleaseDate => new(2002, 09, 24);

    public override GameIconAsset Icon => GameIconAsset.RaymanArena;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanM;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanMArena_Ps2(x, "Rayman Arena - PS2")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "ra_ps2", "ra_ps2"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.RaymanArena_PS2));
    }

    protected override ProgramInstallationStructure CreateStructure() => new Ps2DiscProgramInstallationStructure(new[]
    {
        new Ps2DiscProgramLayout("US", "BA", "SLUS-20272", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLUS_202.72;1", ProgramPathType.PrimaryExe, required: true),
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