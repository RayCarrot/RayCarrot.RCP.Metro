using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 (PS2) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Ps2 : Ps2GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman2_Ps2";
    public override Game Game => Game.Rayman2;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => "Rayman 2 Revolution"; // TODO-LOC
    public override string[] SearchKeywords => new[] { "r2", "revolution" };
    public override DateTime ReleaseDate => new(2000, 12, 22);

    public override GameIconAsset Icon => GameIconAsset.Rayman2Revolution;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman2;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // TODO-UPDATE: Add progression support
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "r2_ps2", "r2_ps2"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.Rayman2_PS2));
    }

    protected override ProgramInstallationStructure CreateStructure() => new Ps2DiscProgramInstallationStructure(new[]
    {
        new Ps2DiscProgramLayout("EU", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_500.44;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("GAME.BF;1", ProgramPathType.Data, required: true),
        })),
        new Ps2DiscProgramLayout("US", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLUS_201.38;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("GAME.BF;1", ProgramPathType.Data, required: true),
        })),
        new Ps2DiscProgramLayout("JP", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLPS_250.29;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("GAME.BF;1", ProgramPathType.Data, required: true),
        })),
        new Ps2DiscProgramLayout("KR", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLPM_675.19;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("GAME.BF;1", ProgramPathType.Data, required: true),
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