#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman ReDesigner game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRedesigner_Win32 : Win32GameDescriptor
{
    #region Descriptor

    public override string Id => "RaymanRedesigner_Win32";
    public override Game Game => Game.RaymanRedesigner;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanRedesigner;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Fan;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman ReDesigner";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => null;

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman ReDesigner.exe";

    /// <summary>
    /// Gets the purchase links for the game for this type
    /// </summary>
    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/Rayman_ReDesigner/539216", GenericIconKind.GameDisplay_Web),
    };

    #endregion
}