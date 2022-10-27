#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Redemption game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRedemption_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "RaymanRedemption_Win32";
    public override Game Game => Game.RaymanRedemption;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanRedemption;

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