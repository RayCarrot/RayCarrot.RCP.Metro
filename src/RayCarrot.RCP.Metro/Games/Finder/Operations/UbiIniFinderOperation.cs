using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.Finder;

/// <summary>
/// A finder operation for handling <see cref="UbiIniFinderQuery"/> queries
/// </summary>
public class UbiIniFinderOperation : FinderOperation
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override void Run(FinderItem[] finderItems)
    {
        // Make sure there are queries to handle
        if (finderItems.SelectMany(x => x.Queries).All(x => x is not UbiIniFinderQuery))
            return;

        FileSystemPath filePath = AppFilePaths.UbiIniPath;

        // Make sure the file exists
        if (!filePath.FileExists)
        {
            Logger.Info("The ubi.ini file was not found");
            return;
        }

        // Enumerate each finder query
        foreach (FinderItem finderItem in finderItems)
        {
            foreach (UbiIniFinderQuery query in finderItem.Queries.OfType<UbiIniFinderQuery>())
            {
                if (finderItem.HasBeenFound)
                    break;

                // Attempt to get the install location
                string dir;

                try
                {
                    dir = IniNative.GetString(filePath, query.SectionName, "Directory", String.Empty);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Reading ubi.ini file value");
                    
                    // Return since if it failed reading one value then it probably can't read any of them
                    return;
                }

                // Make sure we got a location
                if (dir.IsNullOrWhiteSpace())
                    continue;

                // Validate the location
                finderItem.Validate(query, dir);
            }
        }
    }
}