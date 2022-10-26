#nullable disable
using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M game info
/// </summary>
public sealed class GameDescriptor_RaymanM : GameDescriptor
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanM;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman M";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman M";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "RaymanM.exe";

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanM_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanMArena(gameInstallation, false).Yield();

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public override string RayMapURL => AppURLs.GetRayMapGameURL("rm_pc", "rm_pc");

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "RM_Setup_DX8.exe")
    };

    /// <summary>
    /// The group names to use for the options, config and utility dialog
    /// </summary>
    public override IEnumerable<string> DialogGroupNames => new string[]
    {
        UbiIniFileGroupName
    };

    /// <summary>
    /// Indicates if the game can be installed from a disc in this program
    /// </summary>
    public override bool CanBeInstalledFromDisc => true;

    /// <summary>
    /// The .gif files to use during the game installation if installing from a disc
    /// </summary>
    public override string[] InstallerGifs
    {
        get
        {
            var basePath = $"{AppViewModel.WPFApplicationBasePath}Installer/InstallerGifs/";

            return new string[]
            {
                basePath + "ASTRO.gif",
                basePath + "CASK.gif",
                basePath + "CHASE.gif",
                basePath + "GLOB.gif",
                basePath + "RODEO.gif",
            };
        }
    }

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public override bool HasArchives => true;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public override IArchiveDataManager GetArchiveDataManager(GameInstallation gameInstallation) => 
        new CPACntArchiveDataManager(new OpenSpaceSettings(EngineVersion.RaymanM, Platform.PC), gameInstallation);

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
    {
        installDir + "FishBin" + "tex32.cnt",
        installDir + "FishBin" + "vignette.cnt",
        installDir + "MenuBin" + "tex32.cnt",
        installDir + "MenuBin" + "vignette.cnt",
        installDir + "TribeBin" + "tex32.cnt",
        installDir + "TribeBin" + "vignette.cnt",
    };

    #endregion

    #region Public Override Methods

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanM_GameSyncTextureInfo(gameInstallation),
    };

    #endregion
}