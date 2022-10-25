#nullable disable
using System.Collections.Generic;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends game info
/// </summary>
public sealed class GameInfo_RaymanLegends : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanLegends;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Legends";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Legends";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman Legends.exe";

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_UbiArt_ViewModel(gameInstallation, AppFilePaths.RaymanLegendsRegistryKey);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanLegends(gameInstallation).Yield();

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public override bool HasArchives => true;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public override IArchiveDataManager GetArchiveDataManager => new UbiArtIPKArchiveDataManager(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanLegends, Platform.PC), UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed);

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
    {
        installDir + "Bundle_PC.ipk",
        installDir + "persistentLoading_PC.ipk",
    };

    #endregion
}