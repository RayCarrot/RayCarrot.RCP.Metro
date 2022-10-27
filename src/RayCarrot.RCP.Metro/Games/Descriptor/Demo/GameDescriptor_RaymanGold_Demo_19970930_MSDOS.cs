#nullable disable
using System;
using System.Collections.Generic;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Gold Demo (1997/09/30) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanGold_Demo_19970930_MSDOS : MSDOSGameDescriptor
{
    #region Protected Override Properties

    public override string Id => "RaymanGold_Demo_19970930_MSDOS";
    public override Game Game => Game.RaymanDesigner;

    protected override string IconName => $"RaymanGoldDemo";

    #endregion

    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.Demo_RaymanGold;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Demo;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Gold Demo (1997/09/30)";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman.bat";

    /// <summary>
    /// Indicates if the game is a demo
    /// </summary>
    public override bool IsDemo => true;

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => true;

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_RGoldDemo_Url),
    };

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanDesigner_ViewModel(gameInstallation);

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public override bool HasArchives => true;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public override IArchiveDataManager GetArchiveDataManager(GameInstallation gameInstallation) => 
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Kit));

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public override Emulator Emulator => new Emulator_DOSBox();

    #endregion
}