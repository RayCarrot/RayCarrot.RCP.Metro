#nullable disable
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 minigames (Win32) game manager
/// </summary>
public sealed class GameManager_Ray1Minigames_Win32 : GameManager_Win32
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Ray1Minigames;

    public override Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Default to run as admin
        Services.Data.Game_Games[gameInstallation.Game].LaunchMode = UserData_GameLaunchMode.AsAdmin;

        // Call base and return
        return base.PostGameAddAsync(gameInstallation);
    }

    #endregion
}