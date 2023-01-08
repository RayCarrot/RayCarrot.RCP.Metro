using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanM_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanM_Win32";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanM;

    public override LocalizedString DisplayName => "Rayman M";
    public override DateTime ReleaseDate => new(2001, 12, 14);

    public override GameIconAsset Icon => GameIconAsset.RaymanM;

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanMArena(x, "Rayman M", false)));
        builder.Register(new GameConfigComponent(x => new RaymanMConfigViewModel(x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<LocalGameLinksComponent, RaymanMArenaSetupLocalGameLinksComponent>();
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "rm_pc", "rm_pc"));

        builder.Register(new UtilityComponent(x => new Utility_CPATextureSync(x, CPATextureSyncData.FromGameMode(CPAGameMode.RaymanM_PC))));
    }

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("RaymanM.exe", GameInstallationPathType.PrimaryExe, required: true),
        new GameInstallationFilePath("RM_Setup_DX8.exe", GameInstallationPathType.ConfigExe),

        // Directories
        new GameInstallationDirectoryPath("MenuBin", GameInstallationPathType.Data, required: true),
        new GameInstallationDirectoryPath("FishBin", GameInstallationPathType.Data, required: true),
        new GameInstallationDirectoryPath("TribeBin", GameInstallationPathType.Data, required: true),
        new GameInstallationDirectoryPath("MENU", GameInstallationPathType.Save),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DiscInstallGameAddAction(this, new GameInstallerInfo(
            discFilesListFileName: "RaymanM",
            gameLogo: GameLogoAsset.RaymanM,
            gifFileNames: new[] { "ASTRO.gif", "CASK.gif", "CHASE.gif", "GLOB.gif", "RODEO.gif", },
            installFolderName: "Rayman M"))
    });

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.RaymanM, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation,
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.RaymanM_PC));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"FishBin\tex32.cnt",
        @"FishBin\vignette.cnt",
        @"MenuBin\tex32.cnt",
        @"MenuBin\vignette.cnt",
        @"TribeBin\tex32.cnt",
        @"TribeBin\vignette.cnt",
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UbiIniFinderQuery(UbiIniData_RaymanM.SectionName),

        new UninstallProgramFinderQuery("Rayman M"),
        new UninstallProgramFinderQuery("Rayman: M"),

        new Win32ShortcutFinderQuery("Rayman M"),
    };

    #endregion
}