using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Arena (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanArena_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanArena_Win32";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanArena;

    public override LocalizedString DisplayName => "Rayman Arena";
    public override string DefaultFileName => "R_Arena.exe";
    public override DateTime ReleaseDate => new(2002, 09, 24);

    public override GameIconAsset Icon => GameIconAsset.RaymanArena;

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanMArena(x, "Rayman Arena", false)));
        builder.Register(new GameConfigComponent(x => new RaymanArenaConfigViewModel(x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<LocalGameLinksComponent, RaymanMArenaSetupLocalGameLinksComponent>();
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "ra_pc", "ra_pc"));

        builder.Register(new UtilityComponent(x => new Utility_CPATextureSync(x, CPATextureSyncData.FromGameMode(CPAGameMode.RaymanArena_PC))));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DiscInstallGameAddAction(this, new GameInstallerInfo(
            discFilesListFileName: "RaymanArena",
            gameLogo: GameLogoAsset.RaymanArena,
            gifFileNames: new[] { "ASTRO.gif", "CASK.gif", "CHASE.gif", "GLOB.gif", "RODEO.gif", },
            installFolderName: "Rayman Arena"))
    });

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.RaymanM, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation, 
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.RaymanArena_PC));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"MenuBin\tex32.cnt",
        @"MenuBin\vignette.cnt",
        @"MenuBin\Sound.cnt",

        @"FishBin\tex32.cnt",
        @"FishBin\vignette.cnt",
        @"FishBin\Sound.cnt",

        @"TribeBin\tex32.cnt",
        @"TribeBin\vignette.cnt",
        @"TribeBin\Sound.cnt",
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UbiIniFinderQuery(UbiIniData_RaymanArena.SectionName),

        new UninstallProgramFinderQuery("Rayman Arena"),
        new UninstallProgramFinderQuery("Rayman: Arena"),

        new Win32ShortcutFinderQuery("Rayman Arena"),
    };

    #endregion
}