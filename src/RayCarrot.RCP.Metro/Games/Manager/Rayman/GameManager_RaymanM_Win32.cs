#nullable disable
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M (Win32) game manager
/// </summary>
public sealed class GameManager_RaymanM_Win32 : GameManager_Win32
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanM;

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(RMUbiIniHandler.SectionName, "Rayman M", new string[]
    {
        "Rayman M",
        "Rayman: M",
    });

    #endregion
}