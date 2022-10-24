#nullable disable
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 2 (DOSBox) game manager
/// </summary>
public sealed class GameManager_Rayman1Demo2_DOSBox : GameManager_DOSBox
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_Rayman1_2;

    /// <summary>
    /// The executable name for the game. This is independent of the <see cref="GameInfo.DefaultFileName"/> which is used to launch the game.
    /// </summary>
    public override string ExecutableName => "RAYMAN.EXE";

    public override async Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Run base
        await base.PostGameAddAsync(gameInstallation);

        // Set the default mount path if available
        var mountPath = gameInstallation.InstallLocation + "Disc" + "RAY1DEMO.cue";

        if (mountPath.FileExists)
            Services.Data.Game_DosBoxGames[gameInstallation.Game].MountPath = mountPath;
    }

    #endregion
}