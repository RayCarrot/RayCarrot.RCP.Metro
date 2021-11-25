#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Big Bang (WinStore) game manager
/// </summary>
public sealed class GameManager_RabbidsBigBang_WinStore : GameManager_WinStore
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RabbidsBigBang;

    /// <summary>
    /// Gets the package name for the game
    /// </summary>
    public override string PackageName => "UbisoftEntertainment.RabbidsBigBang";

    /// <summary>
    /// Gets the full package name for the game
    /// </summary>
    public override string FullPackageName => "UbisoftEntertainment.RabbidsBigBang_dbgk1hhpxymar";

    /// <summary>
    /// Gets store ID for the game
    /// </summary>
    public override string StoreID => "9WZDNCRFJCS3";

    #endregion
}