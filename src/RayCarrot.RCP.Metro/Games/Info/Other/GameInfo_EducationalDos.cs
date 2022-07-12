#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Educational Dos game info
/// </summary>
public sealed class GameInfo_EducationalDos : GameInfo
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.EducationalDos;

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

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_RaymanEduDos_ViewModel(Game);

    /// <summary>
    /// The options UI, if any is available
    /// </summary>
    public override FrameworkElement OptionsUI => new GameOptions_EducationalDos_UI();

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels
    {
        get
        {
            return Services.Data.Game_EducationalDosBoxGames.
                Where(x => !x.LaunchMode.IsNullOrWhiteSpace()).
                Select(x => new ProgressionGameViewModel_EducationalDos(x));
        }
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
    public override IArchiveDataManager GetArchiveDataManager => new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Edu));

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public override Emulator Emulator => new Emulator_DOSBox(Game, GameType.EducationalDosBox);

    // Don't allow patching for now since this game actually contains multiple games and the
    // patching system doesn't support that right now.
    public override bool AllowPatching => false;

    #endregion
}