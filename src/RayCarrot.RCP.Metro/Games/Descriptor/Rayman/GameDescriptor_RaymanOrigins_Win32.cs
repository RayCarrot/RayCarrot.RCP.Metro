using System.Collections.Generic;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanOrigins_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanOrigins_Win32";
    public override Game Game => Game.RaymanOrigins;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanOrigins;

    public override string DisplayName => "Rayman Origins";
    public override string DefaultFileName => "Rayman Origins.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanOrigins;

    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_UbiArt_ViewModel(gameInstallation, AppFilePaths.RaymanOriginsRegistryKey);

    public override IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanOrigins(gameInstallation, "Rayman Origins").Yield();

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

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanOrigins_HQVideos(gameInstallation),
        new Utility_RaymanOrigins_DebugCommands(gameInstallation),
        new Utility_RaymanOrigins_Update(gameInstallation),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Origins", new[]
    {
        "Rayman Origins",
        "Rayman: Origins",
    });

    #endregion
}