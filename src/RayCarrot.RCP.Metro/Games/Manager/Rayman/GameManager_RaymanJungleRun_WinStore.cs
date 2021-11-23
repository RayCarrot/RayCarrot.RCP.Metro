namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Jungle Run (WinStore) game manager
/// </summary>
public sealed class GameManager_RaymanJungleRun_WinStore : GameManager_WinStore
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanJungleRun;

    /// <summary>
    /// Gets the package name for the game
    /// </summary>
    public override string PackageName => "UbisoftEntertainment.RaymanJungleRun";

    /// <summary>
    /// Gets the full package name for the game
    /// </summary>
    public override string FullPackageName => "UbisoftEntertainment.RaymanJungleRun_dbgk1hhpxymar";

    /// <summary>
    /// Gets store ID for the game
    /// </summary>
    public override string StoreID => "9WZDNCRFJ13P";

    #endregion
}