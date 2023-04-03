namespace RayCarrot.RCP.Metro.Games.Finder;

/// <summary>
/// A finder operation for handling <see cref="PreviouslyDownloadedGameFinderQuery"/> queries
/// </summary>
public class PreviouslyDownloadedGameFinderOperation : FinderOperation
{
    public override void Run(FinderItem[] finderItems)
    {
        // Make sure there are queries to handle
        if (finderItems.SelectMany(x => x.Queries).All(x => x is not PreviouslyDownloadedGameFinderQuery))
            return;

        // Enumerate each finder query
        foreach (FinderItem finderItem in finderItems)
        {
            foreach (PreviouslyDownloadedGameFinderQuery query in finderItem.Queries.OfType<PreviouslyDownloadedGameFinderQuery>())
            {
                if (finderItem.HasBeenFound)
                    break;

                FileSystemPath gameDir = AppFilePaths.GamesBaseDir + query.GameId;

                if (!gameDir.DirectoryExists)
                {
                    gameDir = AppFilePaths.GamesBaseDir + query.LegacyGameId;

                    if (!gameDir.DirectoryExists)
                        continue;
                }

                // Validate the location
                finderItem.Validate(query, new InstallLocation(gameDir));
            }
        }
    }
}