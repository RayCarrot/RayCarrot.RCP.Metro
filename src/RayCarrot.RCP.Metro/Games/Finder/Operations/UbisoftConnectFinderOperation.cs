using Microsoft.Win32;

namespace RayCarrot.RCP.Metro.Games.Finder;

/// <summary>
/// A finder operation for handling <see cref="UbisoftConnectFinderQuery"/> queries
/// </summary>
public class UbisoftConnectFinderOperation : FinderOperation
{
    public override void Run(FinderItem[] finderItems)
    {
        // Make sure there are queries to handle
        if (finderItems.SelectMany(x => x.Queries).All(x => x is not UbisoftConnectFinderQuery))
            return;

        using RegistryKey? parentKey = RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Registry32);

        if (parentKey == null)
            return;

        // Enumerate each finder query
        foreach (FinderItem finderItem in finderItems)
        {
            foreach (UbisoftConnectFinderQuery query in finderItem.Queries.OfType<UbisoftConnectFinderQuery>())
            {
                if (finderItem.HasBeenFound)
                    break;

                // Attempt to get the uninstall key for this Ubisoft Connect installation
                using RegistryKey? ubisoftConnectInstallKey = parentKey.OpenSubKey($"Uplay Install {query.UbisoftConnectGameId}");

                if (ubisoftConnectInstallKey == null)
                    continue;

                // Attempt to get the install location
                string? location = ubisoftConnectInstallKey.GetValue("InstallLocation") as string;

                // Make sure we got a location
                if (location.IsNullOrWhiteSpace())
                    continue;

                // Validate the location
                finderItem.Validate(query, new InstallLocation(location));
            }
        }
    }
}