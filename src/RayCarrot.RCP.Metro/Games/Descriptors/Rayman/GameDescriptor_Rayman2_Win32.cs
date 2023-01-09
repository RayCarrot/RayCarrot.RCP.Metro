using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Win32 : Win32GameDescriptor
{
    #region Constant Fields

    private const string SteamId = "15060";

    #endregion

    #region Public Properties

    public override string GameId => "Rayman2_Win32";
    public override Game Game => Game.Rayman2;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Rayman2;

    public override LocalizedString DisplayName => "Rayman 2";
    public override DateTime ReleaseDate => new(1999, 11, 05);

    public override GameIconAsset Icon => GameIconAsset.Rayman2;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman2;

    public override IEnumerable<string> DialogGroupNames => new string[]
    {
        UbiIniFileGroupName
    };

    #endregion

    #region Private Methods

    private static IEnumerable<GameLinksComponent.GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation) => new[]
    {
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_Setup)), 
            Uri: gameInstallation.InstallLocation + "GXSetup.exe"),
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_R2nGlide)), 
            Uri: gameInstallation.InstallLocation + "nglide_config.exe"),
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_R2dgVoodoo)), 
            Uri: gameInstallation.InstallLocation + "dgVoodooCpl.exe"),
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_R2Fix)), 
            Uri: gameInstallation.InstallLocation + "R2FixCfg.exe"),
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new SteamGameClientComponent(SteamId));

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman2(x, "Rayman 2")));
        builder.Register(new GameConfigComponent(x => new Rayman2ConfigViewModel(x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register(new LocalGameLinksComponent(GetLocalGameLinks));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "r2_pc", "r2_pc"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.Rayman2_PC));
        builder.Register<ArchiveComponent>(new CPAArchiveComponent(_ => new[]
        {
            @"Data\Textures.cnt",
            @"Data\Vignette.cnt",
        }));

        builder.Register(new UtilityComponent(x => new Utility_CPATextureSync(x, CPATextureSyncData.FromGameMode(CPAGameMode.Rayman2_PC))));
    }

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Rayman2.exe", GameInstallationPathType.PrimaryExe, required: true),
        new GameInstallationFilePath("GXSetup.exe", GameInstallationPathType.ConfigExe),

        // Directories
        new GameInstallationDirectoryPath("Data", GameInstallationPathType.Data, required: true)
        {
            new GameInstallationDirectoryPath("Options", GameInstallationPathType.Save),
            new GameInstallationDirectoryPath("World", GameInstallationPathType.Data, required: true),
            new GameInstallationDirectoryPath("SaveGame", GameInstallationPathType.Save)
        },
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DiscInstallGameAddAction(this, new GameInstallerInfo(
            discFilesListFileName: "Rayman2",
            gameLogo: GameLogoAsset.Rayman2,
            gifFileNames: new[] { "ASTRO.gif", "CASK.gif", "CHASE.gif", "GLOB.gif", "RODEO.gif", },
            installFolderName: "Rayman 2"))
    });

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_2_the_great_escape"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman-2--the-great-escape/56c4947e88a7e300458b465c.html")
    };

    public override async Task<IList<string>> GetAppliedUtilitiesAsync(GameInstallation gameInstallation)
    {
        // TODO-14: These are not utilities - these are game modifications caused by the config

        // Create the output
        var output = new List<string>();

        if (await Rayman2ConfigViewModel.GetIsWidescreenHackAppliedAsync(gameInstallation) == true)
            output.Add(Resources.Config_WidescreenSupport);

        var dinput = Rayman2ConfigViewModel.GetCurrentDinput(gameInstallation);

        if (dinput == Rayman2ConfigViewModel.R2Dinput.Controller)
            output.Add(Resources.Config_UseController);

        if (dinput == Rayman2ConfigViewModel.R2Dinput.Mapping)
            output.Add(Resources.Config_ButtonMapping);

        // Get other utilities
        output.AddRange(await base.GetAppliedUtilitiesAsync(gameInstallation));

        return output;
    }

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UbiIniFinderQuery(UbiIniData_Rayman2.SectionName),

        new UninstallProgramFinderQuery("Rayman 2"),
        new UninstallProgramFinderQuery("Rayman: 2"),
        new UninstallProgramFinderQuery("Rayman 2 - The Great Escape"),
        new UninstallProgramFinderQuery("Rayman: 2 - The Great Escape"),
        new UninstallProgramFinderQuery("GOG.com Rayman 2"),

        new Win32ShortcutFinderQuery("Rayman 2"),

        new SteamFinderQuery(SteamId),
    };

    #endregion
}