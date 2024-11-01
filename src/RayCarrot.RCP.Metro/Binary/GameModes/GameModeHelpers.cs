using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public static class GameModeHelpers
{
    public static GameInstallation? FindGameInstallation(GamesManager gamesManager, Enum gameMode) =>
        gamesManager.GetInstalledGames().
            OrderBy(x => x.GameDescriptor.Type).
            FirstOrDefault(x => x.GetComponents<BinaryGameModeComponent>().Any(c => c.GameMode.Equals(gameMode)));
}