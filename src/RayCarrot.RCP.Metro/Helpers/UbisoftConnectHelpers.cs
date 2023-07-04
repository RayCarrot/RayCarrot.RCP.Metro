using System.IO;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Clients.UbisoftConnect;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public static class UbisoftConnectHelpers
{
    /// <summary>
    /// Gets the Ubisoft Connect save directory path for the game. If none is found then <see cref="FileSystemPath.EmptyPath"/> is returned.
    /// </summary>
    /// <param name="gameInstallation">The game to get the path for</param>
    /// <returns>The save directory path or an empty path if none was found</returns>
    public static FileSystemPath GetSaveDirectory(GameInstallation gameInstallation)
    {
        GameClientInstallation? gameClientInstallation = Services.GameClients.GetAttachedGameClient(gameInstallation);
        string gameId = gameInstallation.GetRequiredComponent<UbisoftConnectGameClientComponent>().GameId;

        // Attempt to get the path from the game client installation
        if (gameClientInstallation?.GameClientDescriptor is UbisoftConnectGameClientDescriptor)
        {
            string? userId = gameClientInstallation.GetValue<string>(GameClientDataKey.UbisoftConnect_UserId);

            if (userId != null)
                return gameClientInstallation.InstallLocation.Directory + "savegames" + userId + gameId;
        }

        // Fallback to first added Ubisoft Connect game client installation
        gameClientInstallation = Services.GameClients.GetInstalledGameClients().
            FirstOrDefault(x => x.GameClientDescriptor is UbisoftConnectGameClientDescriptor);

        if (gameClientInstallation != null)
        {
            string? userId = gameClientInstallation.GetValue<string>(GameClientDataKey.UbisoftConnect_UserId);

            if (userId != null)
                return gameClientInstallation.InstallLocation.Directory + "savegames" + userId + gameId;
        }

        // Fallback to the default location
        FileSystemPath saveGamesDir = @"C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\savegames";

        if (saveGamesDir.DirectoryExists)
        {
            string[] subDirs = Directory.GetDirectories(saveGamesDir);

            if (subDirs.Length > 0)
            {
                FileSystemPath userSaveDir = subDirs[0];
                return userSaveDir + gameId;
            }
        }

        return FileSystemPath.EmptyPath;
    }

    public static string GetStorePageURL(string productId)
    {
        return $"https://store.ubisoft.com/game?pid={productId}";
    }

    public static string GetGameLaunchURI(string gameId)
    {
        return $@"uplay://launch/{gameId}/0";
    }
}