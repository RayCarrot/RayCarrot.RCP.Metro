using System.Collections.Generic;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends (Steam) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanLegends_Steam : SteamGameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanLegends_Steam";
    public override Game Game => Game.RaymanLegends;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanLegends;

    public override string DisplayName => "Rayman Legends";
    public override string BackupName => "Rayman Legends";
    public override string DefaultFileName => "Rayman Legends.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanLegends;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanLegends;

    public override bool HasArchives => true;

    public override string SteamID => "242550";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_UbiArt_ViewModel(gameInstallation, AppFilePaths.RaymanLegendsRegistryKey);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanLegends(gameInstallation);

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new UbiArtIPKArchiveDataManager(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanLegends, BinarySerializer.UbiArt.Platform.PC), UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed);

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        "Bundle_PC.ipk",
        "persistentLoading_PC.ipk",
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanLegends_UbiRay(gameInstallation),
        new Utility_RaymanLegends_DebugCommands(gameInstallation),
    };

    #endregion
}