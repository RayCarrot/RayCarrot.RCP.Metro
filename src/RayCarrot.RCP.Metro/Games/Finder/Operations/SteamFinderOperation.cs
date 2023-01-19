using Microsoft.Win32;

namespace RayCarrot.RCP.Metro.Games.Finder;

/// <summary>
/// A finder operation for handling <see cref="SteamFinderQuery"/> queries
/// </summary>
public class SteamFinderOperation : FinderOperation
{
    public override void Run(FinderItem[] finderItems)
    {
        // Make sure there are queries to handle
        if (finderItems.SelectMany(x => x.Queries).All(x => x is not SteamFinderQuery))
            return;

        using RegistryKey? parentKey = RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Default);

        if (parentKey == null)
            return;

        // Enumerate each finder query
        foreach (FinderItem finderItem in finderItems)
        {
            foreach (SteamFinderQuery query in finderItem.Queries.OfType<SteamFinderQuery>())
            {
                if (finderItem.HasBeenFound)
                    break;

                // Attempt to get the uninstall key for this Steam app
                using RegistryKey? steamAppKey = parentKey.OpenSubKey($"Steam App {query.SteamId}");

                if (steamAppKey == null)
                    continue;

                // Attempt to get the install location
                string? location = steamAppKey.GetValue("InstallLocation") as string;

                // Make sure we got a location
                if (location.IsNullOrWhiteSpace())
                    continue;

                // Validate the location
                finderItem.Validate(query, new InstallLocation(location));
            }
        }
    }
}