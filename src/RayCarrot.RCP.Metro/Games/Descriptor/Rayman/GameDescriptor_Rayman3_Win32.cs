﻿#nullable disable
using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Win32 : Win32GameDescriptor
{
    #region Descriptor

    public override string Id => "Rayman3_Win32";
    public override Game Game => Game.Rayman3;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.Rayman3;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman 3";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman 3";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman3.exe";

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_Rayman3_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) =>
        new ProgressionGameViewModel_Rayman3(gameInstallation).Yield();

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public override string RayMapURL => AppURLs.GetRayMapGameURL("r3_pc", "r3_pc");

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "R3_Setup_DX8.exe")
    };

    /// <summary>
    /// The group names to use for the options, config and utility dialog
    /// </summary>
    public override IEnumerable<string> DialogGroupNames => new string[]
    {
        UbiIniFileGroupName
    };

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public override bool HasArchives => true;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public override IArchiveDataManager GetArchiveDataManager(GameInstallation gameInstallation) => 
        new CPACntArchiveDataManager(new OpenSpaceSettings(EngineVersion.Rayman3, BinarySerializer.OpenSpace.Platform.PC), gameInstallation);

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
    {
        installDir + "Gamedatabin" + "tex32_1.cnt",
        installDir + "Gamedatabin" + "tex32_2.cnt",
        installDir + "Gamedatabin" + "vignette.cnt",
    };

    /// <summary>
    /// Gets the purchase links for the game
    /// </summary>
    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_3_hoodlum_havoc"),
        new(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--3--hoodlum-havoc/5800b15eef3aa5ab3e8b4567.html")
    };

    #endregion

    #region Public Override Methods

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_Rayman3_GameSyncTextureInfo(gameInstallation),
        new Utility_Rayman3_DirectPlay(gameInstallation),
    };

    #endregion
}