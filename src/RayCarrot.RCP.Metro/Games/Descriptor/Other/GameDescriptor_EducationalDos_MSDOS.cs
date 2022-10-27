#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using BinarySerializer.Ray1;
using NLog;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;
using static RayCarrot.RCP.Metro.GameManager_Win32;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Educational Dos game descriptor
/// </summary>
public sealed class GameDescriptor_EducationalDos_MSDOS : MSDOSGameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Overrides

    public override string Id => "EducationalDos_MSDOS";
    public override Game Game => Game.EducationalDos;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.EducationalDos;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Other;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Educational Games";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => throw new Exception("A generic backup name can not be obtained for an educational DOS game due to it being a collection of multiple games");

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => Services.Data.Game_EducationalDosBoxGames?.FirstOrDefault()?.LaunchName;

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanEduDos_ViewModel(gameInstallation);

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => new GameOptions_EducationalDos_Control();

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation)
    {
        return Services.Data.Game_EducationalDosBoxGames.
            Where(x => !x.LaunchMode.IsNullOrWhiteSpace()).
            Select(x => new ProgressionGameViewModel_EducationalDos(gameInstallation, x));
    }

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public override string RayMapURL => AppURLs.GetRay1MapGameURL("RaymanEducationalPC", "r1/edu/pc_gb", "GB1");

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public override bool HasArchives => true;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public override IArchiveDataManager GetArchiveDataManager(GameInstallation gameInstallation) => 
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Edu));

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public override Emulator Emulator => new Emulator_DOSBox();

    // Don't allow patching for now since this game actually contains multiple games and the
    // patching system doesn't support that right now.
    public override bool AllowPatching => false;

    /// <summary>
    /// Gets the additional overflow button items for the game
    /// </summary>
    public override IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems() => 
        Services.Data.Game_EducationalDosBoxGames.Select(x => new OverflowButtonItemViewModel(x.Name, new BitmapImage(new Uri(AppViewModel.WPFApplicationBasePath + @"img\GameIcons\EducationalDos.png")), new AsyncRelayCommand(async () =>
        {
            Logger.Trace("The educational game {0} is being launched...", x.Name);

            // Verify that the game can launch
            if (!await this.GetLegacyManager<GameManager_EducationalDos_EducationalDOSBox>().VerifyCanLaunchAsync(x))
            {
                Logger.Info("The educational game {0} could not be launched", x.Name);
                return;
            }

            // Get the launch info
            GameLaunchInfo launchInfo = this.GetLegacyManager<GameManager_EducationalDos_EducationalDOSBox>().GetLaunchInfo(x);

            Logger.Trace("The educational game {0} launch info has been retrieved as Path = {1}, Args = {2}", x.Name, launchInfo.Path, launchInfo.Args);

            // Launch the game
            var launchMode = LegacyGame.GetInstallation().GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
            var process = await Services.File.LaunchFileAsync(launchInfo.Path, launchMode == UserData_GameLaunchMode.AsAdmin, launchInfo.Args);

            Logger.Info("The educational game {0} has been launched", x.Name);

            if (process != null)
                // Run any post launch operations on the process
                await this.GetLegacyManager<GameManager_EducationalDos_EducationalDOSBox>().PostLaunchAsync(process);

        }))).ToArray();

    #endregion
}