#nullable disable
using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Tonic Trouble game info
/// </summary>
public sealed class GameDescriptor_TonicTrouble : GameDescriptor
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.TonicTrouble;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Other;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Tonic Trouble";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Tonic Trouble";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "TonicTrouble.exe";

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_TonicTrouble(gameInstallation).Yield();

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public override string RayMapURL => AppURLs.GetRayMapGameURL("tt_pc", "tt_pc");

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_R2dgVoodoo, gameInstallation.InstallLocation + "dgVoodooCpl.exe"),
    };

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public override bool HasArchives => true;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public override IArchiveDataManager GetArchiveDataManager(GameInstallation gameInstallation) => 
        new CPACntArchiveDataManager(new OpenSpaceSettings(EngineVersion.TonicTrouble, Platform.PC), gameInstallation);

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
    {
        installDir + "gamedata" + "Textures.cnt",
        installDir + "gamedata" + "Vignette.cnt",
    };

    #endregion

    #region Public Override Methods

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_TonicTrouble_GameSyncTextureInfo(gameInstallation),
    };

    #endregion
}