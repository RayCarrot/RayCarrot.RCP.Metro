using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 (Jaguar) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Jaguar : JaguarGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Jaguar";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman1_Jaguar_Title));
    public override string[] SearchKeywords => new[] { "r1", "jag" };
    public override DateTime ReleaseDate => new(1995, 09, 09);

    public override GameIconAsset Icon => GameIconAsset.Rayman1Jaguar;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman_Jaguar(x, "Rayman 1 - Jaguar")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanJaguar", "r1_jaguar/r1"));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.Rayman1_Jaguar));
    }

    protected override ProgramInstallationStructure CreateStructure() => new JaguarRomProgramInstallationStructure();

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}