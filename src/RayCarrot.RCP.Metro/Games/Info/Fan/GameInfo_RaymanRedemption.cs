#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Redemption game info
/// </summary>
public sealed class GameInfo_RaymanRedemption : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanRedemption;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Fan;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Redemption";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Redemption";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman Redemption.exe";

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => false;

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanRedemption(gameInstallation).Yield();

    #endregion
}