using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanLegends_Win32 : Win32GameDescriptor
{
    #region Constant Fields

    private const string SteamId = "242550";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanLegends_Win32";
    public override string LegacyGameId => "RaymanLegends";
    public override Game Game => Game.RaymanLegends;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.RaymanLegends_Win32_Title));
    public override string[] SearchKeywords => new[] { "rl" };
    public override DateTime ReleaseDate => new(2013, 08, 29);

    public override GameIconAsset Icon => GameIconAsset.RaymanLegends;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanLegends;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new SteamGameClientComponent(SteamId));

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanLegends_Win32(x, "Rayman Legends")));
        builder.Register(new GameConfigComponent(x => new UbiArtConfigViewModel(x, AppFilePaths.RaymanLegendsRegistryKey)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<BinaryGameModeComponent>(new UbiArtGameModeComponent(UbiArtGameMode.RaymanLegends_PC));
        builder.Register<ArchiveComponent>(new UbiArtArchiveComponent(_ => new[]
        {
            "Bundle_PC.ipk",
            "persistentLoading_PC.ipk",
            "patch_PC.ipk",
        }));

        builder.Register(new UtilityComponent(x => new Utility_RaymanLegends_UbiRay(x)));
        builder.Register(new UtilityComponent(x => new Utility_RaymanLegends_DebugCommands(x)));
    }

    protected override ProgramInstallationStructure GetStructure() => new DirectoryProgramInstallationStructure(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Rayman Legends.exe", GameInstallationPathType.PrimaryExe, required: true),
        new GameInstallationFilePath("Bundle_PC.ipk", GameInstallationPathType.Data, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateDirectoryGameAddAction(this),
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman--legends/56c4948888a7e300458b47da.html"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_Steam)), SteamHelpers.GetStorePageURL(SteamId)),
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UninstallProgramFinderQuery("Rayman Legends"),
        new UninstallProgramFinderQuery("Rayman: Legends"),

        new Win32ShortcutFinderQuery("Rayman Legends"),

        new SteamFinderQuery(SteamId),
    };

    #endregion
}