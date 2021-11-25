#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins (Steam) game manager
/// </summary>
public sealed class GameManager_RaymanOrigins_Steam : GameManager_Steam
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanOrigins;

    /// <summary>
    /// Gets the Steam ID for the game
    /// </summary>
    public override string SteamID => "207490";

    #endregion
}