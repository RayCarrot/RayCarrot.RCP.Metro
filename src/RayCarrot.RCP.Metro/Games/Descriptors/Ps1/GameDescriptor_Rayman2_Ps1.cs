using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 (PS1) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Ps1 : Ps1GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman2_Ps1";
    public override Game Game => Game.Rayman2;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => "Rayman 2"; // TODO-LOC
    public override string[] SearchKeywords => new[] { "r2" };
    public override DateTime ReleaseDate => new(2000, 09, 07);

    public override GameIconAsset Icon => GameIconAsset.Rayman2;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman2;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman2_Ps1(x, "Rayman 2 - PS1")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "r2_ps1", "r2_ps1"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.Rayman2_PS1));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.Rayman2_PS1));

        builder.Register(new RuntimeModificationsGameManagersComponent(EmulatedPlatform.Ps1, _ =>
            new[]
            {
                new Ray1GameManager(
                    displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R2_PS1_Proto)),
                    getOffsetsFunc: () => Ray1MemoryData.Offsets_PS1_R2)
            }));
    }

    protected override ProgramInstallationStructure CreateStructure() => new PS1DiscProgramInstallationStructure(new[]
    {
        new Ps1DiscProgramLayout("EU_EnEsIt", "BE", "SLES-02906", 2, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_029.06;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("COMBIN.DAT;1", ProgramPathType.Data, required: true),
        })),
        new Ps1DiscProgramLayout("EU_FrDe", "BE", "SLES-02905", 2, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_029.05;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("COMBIN.DAT;1", ProgramPathType.Data, required: true),
        })),
        new Ps1DiscProgramLayout("US", "BA", "SLUS-01235", 2, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLUS_012.35;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramFilePath("COMBIN.DAT;1", ProgramPathType.Data, required: true),
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