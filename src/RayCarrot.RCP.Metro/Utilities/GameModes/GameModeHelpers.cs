using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public static class GameModeHelpers
{
    public static GameInstallation? FindGameInstallation(GamesManager gamesManager, Enum gameMode) =>
        gamesManager.GetInstalledGames().
            OrderBy(x => x.GameDescriptor.IsDemo ? 1 : 0).
            FirstOrDefault(x => x.GetComponent<BinaryGameModeComponent>()?.GameMode.Equals(gameMode) == true);
}