using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Finder;

public class PreviouslyDownloadedGameFinderQuery : FinderQuery
{
    public PreviouslyDownloadedGameFinderQuery(string gameId, string legacyGameId)
    {
        GameId = gameId;
        LegacyGameId = legacyGameId;

        ConfigureInstallation = x =>
        {
            // Set the install info so that it can be uninstalled
            RCPGameInstallData installData = new(x.InstallLocation.Directory, RCPGameInstallData.RCPInstallMode.Download, DateTime.Now);
            x.SetObject(GameDataKey.RCP_GameInstallData, installData);

            return Task.CompletedTask;
        };
    }

    public string GameId { get; }
    public string LegacyGameId { get; }
}