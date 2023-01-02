using IniParser;
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

        FileSystemPath filePath = AppFilePaths.UbiIniPath1;

        // Make sure the file exists
        if (!filePath.FileExists)
        {
            Logger.Info("The ubi.ini file was not found");
            return;
        }

        IDictionary<string, string?>? gameDirs;

        try
        {
            // Get the sections and the directory for each one
            gameDirs = new FileIniDataParser(new UbiIniDataParser()).
                // Read the primary ubi.ini file
                ReadFile(filePath).
                // Get the sections
                Sections.
                // Create a dictionary
                ToDictionary(x => x.SectionName, x => x.Keys.GetKeyData("Directory")?.Value);

            Logger.Info("The ubi.ini file data was parsed");
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Reading ubi.ini file");
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
                string? location = gameDirs.TryGetValue(query.SectionName);

                // Make sure we got a location
                if (location.IsNullOrWhiteSpace())
                    continue;

                // Validate the location
                finderItem.Validate(query, location);
            }
        }
    }
}