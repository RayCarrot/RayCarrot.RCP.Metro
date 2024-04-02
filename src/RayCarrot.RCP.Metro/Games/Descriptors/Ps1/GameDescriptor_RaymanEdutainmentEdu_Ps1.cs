using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Edutainment Edu (PS1) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanEdutainmentEdu_Ps1 : Ps1GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanEdutainmentEdu_Ps1";
    public override Game Game => Game.RaymanEdutainment;
    public override GameCategory Category => GameCategory.Other;

    public override LocalizedString DisplayName => "Rayman Junior - Math and language with Rayman (Edu)"; // TODO-LOC
    public override string[] SearchKeywords => new[] { "edu", "junior", "brain" };
    public override DateTime ReleaseDate => new(2000, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanEdutainment;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanEducationalPS1", "r1/edu/ps1_eu1", "GB1"));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.RaymanEducational_PS1));
    }

    protected override ProgramInstallationStructure CreateStructure() => new PS1DiscProgramInstallationStructure(new[]
    {
        new Ps1DiscProgramLayout("EU_1", "BE", "SLES-02798", 12, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_027.98;1", ProgramPathType.PrimaryExe, required: true),
        })),
        new Ps1DiscProgramLayout("EU_2", "BE", "SLES-02799", 12, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_027.99;1", ProgramPathType.PrimaryExe, required: true),
        })),
        new Ps1DiscProgramLayout("EU_3", "BE", "SLES-02800", 12, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_028.00;1", ProgramPathType.PrimaryExe, required: true),
        })),
        new Ps1DiscProgramLayout("US", "BA", "SLUS-01265", 12, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLUS_012.65;1", ProgramPathType.PrimaryExe, required: true),
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