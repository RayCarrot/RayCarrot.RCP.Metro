using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 (PS2) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Ps2 : Ps2GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3_Ps2";
    public override Game Game => Game.Rayman3;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => "Rayman 3"; // TODO-LOC
    public override string[] SearchKeywords => new[] { "r3" };
    public override DateTime ReleaseDate => new(2003, 03, 14);

    public override GameIconAsset Icon => GameIconAsset.Rayman3;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman3;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // TODO-UPDATE: Add progression support
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "r3_ps2", "r3_ps2"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.Rayman3_PS2));
    }

    protected override ProgramInstallationStructure CreateStructure() => new Ps2DiscProgramInstallationStructure(new[]
    {
        new Ps2DiscProgramLayout("EU", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_512.22;1", ProgramPathType.PrimaryExe, required: true),
        })),
        new Ps2DiscProgramLayout("US", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLUS_206.01;1", ProgramPathType.PrimaryExe, required: true),
        })),
        new Ps2DiscProgramLayout("KR", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLKA_250.78;1", ProgramPathType.PrimaryExe, required: true),
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