#nullable disable
using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

// TODO-14: Create a Steam variant

/// <summary>
/// The Rayman 2 game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "Rayman2_Win32";
    public override Game Game => Game.Rayman2;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.Rayman2;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 2";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman 2";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman2.exe";

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_Rayman2_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) =>
        new ProgressionGameViewModel_Rayman2(gameInstallation).Yield();

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public override string RayMapURL => AppURLs.GetRayMapGameURL("r2_pc", "r2_pc");

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "GXSetup.exe"),
        new(Resources.GameLink_R2nGlide, gameInstallation.InstallLocation + "nglide_config.exe"),
        new(Resources.GameLink_R2dgVoodoo, gameInstallation.InstallLocation + "dgVoodooCpl.exe"),
        new(Resources.GameLink_R2Fix, gameInstallation.InstallLocation + "R2FixCfg.exe"),
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
        new CPACntArchiveDataManager(new OpenSpaceSettings(EngineVersion.Rayman2, BinarySerializer.OpenSpace.Platform.PC), gameInstallation);

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
    {
        installDir + "Data" + "Textures.cnt",
        installDir + "Data" + "Vignette.cnt",
    };

    #endregion

    #region Public Override Methods

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_Rayman2_GameSyncTextureInfo(gameInstallation),
    };

    public override async Task<IList<string>> GetAppliedUtilitiesAsync(GameInstallation gameInstallation)
    {
        // TODO-14: These are not utilities - these are game modifications caused by the config

        // Create the output
        var output = new List<string>();

        if (await Config_Rayman2_ViewModel.GetIsWidescreenHackAppliedAsync(gameInstallation) == true)
            output.Add(Resources.Config_WidescreenSupport);

        var dinput = Config_Rayman2_ViewModel.GetCurrentDinput(gameInstallation);

        if (dinput == Config_Rayman2_ViewModel.R2Dinput.Controller)
            output.Add(Resources.Config_UseController);

        if (dinput == Config_Rayman2_ViewModel.R2Dinput.Mapping)
            output.Add(Resources.Config_ButtonMapping);

        // Get other utilities
        output.AddRange(await base.GetAppliedUtilitiesAsync(gameInstallation));

        return output;
    }

    #endregion
}