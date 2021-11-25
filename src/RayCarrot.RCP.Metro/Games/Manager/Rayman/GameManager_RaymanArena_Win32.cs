#nullable disable
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Arena (Win32) game manager
/// </summary>
public sealed class GameManager_RaymanArena_Win32 : GameManager_Win32
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanArena;

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(RAUbiIniHandler.SectionName, "Rayman Arena", new string[]
    {
        "Rayman Arena",
        "Rayman: Arena",
    });

    #endregion
}