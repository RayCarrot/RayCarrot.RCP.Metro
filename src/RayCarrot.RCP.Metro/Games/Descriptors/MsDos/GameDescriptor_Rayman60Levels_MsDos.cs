using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 60 Levels (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman60Levels_MsDos : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman60Levels_MsDos";
    public override string LegacyGameId => "Rayman60Levels";
    public override Game Game => Game.Rayman60Levels;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman60Levels_MsDos_Title));
    public override DateTime ReleaseDate => new(1999, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.Rayman60Levels;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman60Levels_MsDos(x, "Rayman 60 Levels")));
        builder.Register<GameValidationCheckComponent, Ray1MsDosGameDataGameValidationCheckComponent>();
        builder.Register(new GameConfigComponent(x => new RaymanByHisFansConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, SetRay1MsDosDataOnGameAddedComponent>(ComponentPriority.High);
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<LaunchArgumentsComponent, Ray1LaunchArgumentsComponent>();
        builder.Register<MsDosGameRequiresDiscComponent>();
        builder.Register(new GameOptionsComponent(x => new Ray1MsDosGameOptionsViewModel(x)));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "Rayman60LevelsPC", "r1/pc_60n"));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.Rayman60Levels_PC));
        builder.Register<ArchiveComponent, Ray1MsDosArchiveComponent>();
    }

    protected override ProgramInstallationStructure CreateStructure() => new DirectoryProgramInstallationStructure(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("RAYPLUS.EXE", GameInstallationPathType.PrimaryExe, required: true),

        // Directories
        new GameInstallationDirectoryPath("PCMAP", GameInstallationPathType.Data, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateDirectoryGameAddAction(this),
    };

    #endregion
}