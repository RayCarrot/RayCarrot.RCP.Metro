#nullable disable
using System.Collections.Generic;
using static RayCarrot.RCP.Metro.GameManager;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Bowling 2 game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanBowling2_Win32 : Win32GameDescriptor
{
    #region Descriptor

    public override string Id => "RaymanBowling2_Win32";
    public override Game Game => Game.RaymanBowling2;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanBowling2;

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

    /// <summary>
    /// Gets the purchase links for the game for this type
    /// </summary>
    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/rayman_bowling_2/532563", GenericIconKind.GameDisplay_Web),
    };

    #endregion
}