using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace RayCarrot.RCP.Metro.Games.Finder;

/// <summary>
/// A finder operation for handling <see cref="WindowsPackageFinderQuery"/> queries
/// </summary>
public class WindowsPackageFinderOperation : FinderOperation
{
    public override void Run(FinderItem[] finderItems)
    {
        // Make sure there are queries to handle
        if (finderItems.SelectMany(x => x.Queries).All(x => x is not WindowsPackageFinderQuery))
            return;

        PackageManager packageManager = new();

        // Get the packages
        List<Package> packages = packageManager.FindPackagesForUser(String.Empty).ToList();

        // Enumerate each finder query
        foreach (FinderItem finderItem in finderItems)
        {
            foreach (WindowsPackageFinderQuery query in finderItem.Queries.OfType<WindowsPackageFinderQuery>())
            {
                if (finderItem.HasBeenFound)
                    break;

                // Attempt to find a matching package
                Package? package = packages.Find(x => x.Id.Name == query.PackageName);

                // Make sure we got a package
                if (package == null)
                    continue;

                // Validate the package location
                finderItem.Validate(query, package.InstalledLocation.Path);
            }
        }
    }
}