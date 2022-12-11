using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanOrigins_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanOrigins_Win32";
    public override Game Game => Game.RaymanOrigins;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanOrigins;

    public override string DisplayName => "Rayman Origins";
    public override string DefaultFileName => "Rayman Origins.exe";
    public override System.DateTime ReleaseDate => new(2012, 03, 29);

    public override GameIconAsset Icon => GameIconAsset.RaymanOrigins;

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanOrigins(x, "Rayman Origins")));
        builder.Register(new GameConfigComponent(x => new UbiArtConfigViewModel(x, AppFilePaths.RaymanOriginsRegistryKey)));

        builder.Register(new UtilityComponent(x => new Utility_RaymanOrigins_HQVideos(x)));
        builder.Register(new UtilityComponent(x => new Utility_RaymanOrigins_DebugCommands(x)));
        builder.Register(new UtilityComponent(x => new Utility_RaymanOrigins_Update(x)));
    }

    #endregion

    #region Public Methods

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new UbiArtIPKArchiveDataManager(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanOrigins, BinarySerializer.UbiArt.Platform.PC), UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed);

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"GameData\bundle_PC.ipk",
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_origins"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman-origins/56c4948888a7e300458b47dc.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Origins", new[]
    {
        "Rayman Origins",
        "Rayman: Origins",
    });

    #endregion
}