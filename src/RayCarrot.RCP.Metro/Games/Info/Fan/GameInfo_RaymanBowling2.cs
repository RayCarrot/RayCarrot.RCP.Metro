#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Bowling 2 game info
/// </summary>
public sealed class GameInfo_RaymanBowling2 : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanBowling2;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Fan;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Bowling 2";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Bowling 2";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman Bowling 2.exe";

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => false;

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) =>
        new ProgressionGameViewModel_RaymanBowling2(gameInstallation).Yield();

    #endregion
}