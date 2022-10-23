#nullable disable
using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Tonic Trouble Special Edition game info
/// </summary>
public sealed class GameInfo_TonicTroubleSpecialEdition : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.TonicTroubleSpecialEdition;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Other;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Tonic Trouble Special Edition";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Tonic Trouble Special Edition";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "MaiD3Dvr.exe";

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels => 
        new ProgressionGameViewModel_TonicTrouble(Games.TonicTroubleSpecialEdition).Yield();

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public override string RayMapURL => AppURLs.GetRayMapGameURL("ttse_pc", "ttse_pc");

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
    public override IArchiveDataManager GetArchiveDataManager => new CPACntArchiveDataManager(new OpenSpaceSettings(EngineVersion.TonicTroubleSpecialEdition, Platform.PC), Game);

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
    {
        installDir + "GameData" + "Textures.cnt",
        installDir + "GameData" + "Vignette.cnt",
    };

    #endregion
}