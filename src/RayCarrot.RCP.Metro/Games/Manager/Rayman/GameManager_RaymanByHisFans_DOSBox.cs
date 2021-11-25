#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman by his Fans (DOSBox) game manager
/// </summary>
public sealed class GameManager_RaymanByHisFans_DOSBox : GameManager_DOSBox
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanByHisFans;

    /// <summary>
    /// The executable name for the game. This is independent of the <see cref="GameInfo.DefaultFileName"/> which is used to launch the game.
    /// </summary>
    public override string ExecutableName => "RAYFAN.EXE";

    /// <summary>
    /// The Rayman Forever folder name, if available
    /// </summary>
    public override string RaymanForeverFolderName => "RayFan";

    /// <summary>
    /// Gets the purchase links for the game
    /// </summary>
    public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
    {
        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_forever"),
        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    #endregion
}