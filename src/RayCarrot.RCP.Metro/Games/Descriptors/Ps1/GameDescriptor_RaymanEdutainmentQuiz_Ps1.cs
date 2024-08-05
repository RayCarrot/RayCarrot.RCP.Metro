using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Edutainment Qui (PS1) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanEdutainmentQuiz_Ps1 : Ps1GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanEdutainmentQui_Ps1";
    public override Game Game => Game.RaymanEdutainment;
    public override GameCategory Category => GameCategory.Other;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.RaymanEdutainmentQui_Ps1_Title));
    public override string[] SearchKeywords => new[] { "edu", "quiz", "junior" };
    public override DateTime ReleaseDate => new(2000, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanEdutainment;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanQuizPS1", "r1/quiz/ps1_eu1", "FG1"));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.RaymanEducational_PS1));
    }

    protected override ProgramInstallationStructure CreateStructure() => new Ps1DiscProgramInstallationStructure(new[]
    {
        new Ps1DiscProgramLayout("EU", "BE", "SLES-02797", 12, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_027.97;1", ProgramPathType.PrimaryExe, required: true),
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