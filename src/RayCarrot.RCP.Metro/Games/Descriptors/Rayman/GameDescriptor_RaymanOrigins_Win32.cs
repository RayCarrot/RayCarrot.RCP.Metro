using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanOrigins_Win32 : Win32GameDescriptor
{
    #region Constant Fields

    private const string SteamId = "207490";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanOrigins_Win32";
    public override string LegacyGameId => "RaymanOrigins";
    public override Game Game => Game.RaymanOrigins;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => "Rayman Origins";
    public override DateTime ReleaseDate => new(2012, 03, 29);

    public override GameIconAsset Icon => GameIconAsset.RaymanOrigins;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanOrigins;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new SteamGameClientComponent(SteamId));

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanOrigins(x, "Rayman Origins")));
        builder.Register(new GameConfigComponent(x => new UbiArtConfigViewModel(x, AppFilePaths.RaymanOriginsRegistryKey)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<BinaryGameModeComponent>(new UbiArtGameModeComponent(UbiArtGameMode.RaymanOrigins_PC));
        builder.Register<ArchiveComponent>(new UbiArtArchiveComponent(_ => new[]
        {
            @"GameData\bundle_PC.ipk",
        }));

        builder.Register(new UtilityComponent(x => new Utility_RaymanOrigins_HQVideos(x)));
        builder.Register(new UtilityComponent(x => new Utility_RaymanOrigins_DebugCommands(x)));
        builder.Register(new UtilityComponent(x => new Utility_RaymanOrigins_Update(x)));
    }

    protected override ProgramInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Rayman Origins.exe", GameInstallationPathType.PrimaryExe, required: true),
        
        // Directories
        new GameInstallationDirectoryPath("GameData", GameInstallationPathType.Data, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_origins"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman-origins/56c4948888a7e300458b47dc.html"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_Steam)), SteamHelpers.GetStorePageURL(SteamId)),
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UninstallProgramFinderQuery("Rayman Origins"),
        new UninstallProgramFinderQuery("Rayman: Origins"),

        new Win32ShortcutFinderQuery("Rayman Origins"),

        new SteamFinderQuery(SteamId),
    };

    #endregion
}