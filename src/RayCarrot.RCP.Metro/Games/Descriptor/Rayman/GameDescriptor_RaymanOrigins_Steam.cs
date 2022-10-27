#nullable disable
using System.Collections.Generic;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanOrigins_Steam : SteamGameDescriptor
{
    #region Public Override Properties

    public override string Id => "RaymanOrigins_Steam";
    public override Game Game => Game.RaymanOrigins;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanOrigins;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Origins";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Origins";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman Origins.exe";

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_UbiArt_ViewModel(gameInstallation, AppFilePaths.RaymanOriginsRegistryKey);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanOrigins(gameInstallation).Yield();

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public override bool HasArchives => true;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public override IArchiveDataManager GetArchiveDataManager(GameInstallation gameInstallation) => 
        new UbiArtIPKArchiveDataManager(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanOrigins, BinarySerializer.UbiArt.Platform.PC), UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed);

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
    {
        installDir + "GameData" + "bundle_PC.ipk",
    };

    #endregion

    #region Platform Properties

    public override string SteamID => "15060";

    #endregion

    #region Public Override Methods

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanOrigins_HQVideos(gameInstallation),
        new Utility_RaymanOrigins_DebugCommands(gameInstallation),
        new Utility_RaymanOrigins_Update(gameInstallation),
    };

    #endregion
}