using System.Collections.Generic;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins (Steam) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanOrigins_Steam : SteamGameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanOrigins_Steam";
    public override Game Game => Game.RaymanOrigins;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanOrigins;

    public override string DisplayName => "Rayman Origins";
    public override string BackupName => "Rayman Origins";
    public override string DefaultFileName => "Rayman Origins.exe";

    public override bool HasArchives => true;

    public override string SteamID => "207490";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_UbiArt_ViewModel(gameInstallation, AppFilePaths.RaymanOriginsRegistryKey);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanOrigins(gameInstallation);

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new UbiArtIPKArchiveDataManager(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanOrigins, BinarySerializer.UbiArt.Platform.PC), UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed);

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"GameData\bundle_PC.ipk",
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanOrigins_HQVideos(gameInstallation),
        new Utility_RaymanOrigins_DebugCommands(gameInstallation),
        new Utility_RaymanOrigins_Update(gameInstallation),
    };

    #endregion
}