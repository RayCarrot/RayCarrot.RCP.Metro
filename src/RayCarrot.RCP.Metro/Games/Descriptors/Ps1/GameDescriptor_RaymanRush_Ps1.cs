using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Rush (PS1) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRush_Ps1 : Ps1GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRush_Ps1";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.RaymanRush_Ps1_Title));
    public override string[] SearchKeywords => new[] { "rr" };
    public override DateTime ReleaseDate => new(2002, 03, 08);

    public override GameIconAsset Icon => GameIconAsset.RaymanRush;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanM;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanRush_Ps1(x, "Rayman Rush - PS1")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "rr_ps1", "rr_ps1"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.RaymanRush_PS1));
    }

    protected override ProgramInstallationStructure CreateStructure() => new Ps1DiscProgramInstallationStructure(new[]
    {
        new Ps1DiscProgramLayout("EU", "BE", "SLES-03812", 1, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_038.12;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("MAPS.DAT;1", ProgramPathType.Data, required: true),
        })),
        new Ps1DiscProgramLayout("US", "BA", "SLUS-01458", 1, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLUS_014.58;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("MAPS.DAT;1", ProgramPathType.Data, required: true),
        })),
        new Ps1DiscProgramLayout("KR", "BA", "SLUS-01458", 1, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLPM_885.03;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("MAPS.DAT;1", ProgramPathType.Data, required: true),
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