using Microsoft.Win32;

namespace RayCarrot.RCP.Metro.Games.Finder;

/// <summary>
/// A finder operation for handling <see cref="UninstallProgramFinderQuery"/> queries
/// </summary>
public class UninstallProgramFinderOperation : FinderOperation
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static Dictionary<string, string> GetProgramDirectories()
    {
        Dictionary<string, string> programDirs = new();

        Logger.Info("Getting installed programs from the Registry");

        // Get 64-bit location if on 64-bit system
        RegistryKey?[] keys = Environment.Is64BitOperatingSystem
            ? new[]
            {
                RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Registry32),
                RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Registry64)
            }
            : new[]
            {
                RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Registry32),
            };

        // Enumerate the uninstall keys
        foreach (RegistryKey? key in keys)
        {
            if (key == null)
                continue;

            // Dispose key when done
            using RegistryKey registryKey = key;

            // Enumerate the sub keys
            foreach (string subKeyName in registryKey.GetSubKeyNames())
            {
                // Make sure it's not a Windows update
                if (subKeyName.StartsWith("KB") && subKeyName.Length == 8)
                    continue;

                // Open the sub key
                using RegistryKey? subKey = registryKey.OpenSubKey(subKeyName);

                // Make sure the key is not null    
                if (subKey == null)
                    continue;

                // Make sure it is not a system component
                if (subKey.GetValue("SystemComponent") as int? == 1)
                    continue;

                if (subKey.GetValue("WindowsInstaller") as int? == 1)
                    continue;

                // Make sure it has an uninstall string
                if (subKey.GetValue("UninstallString") == null)
                    continue;

                if (subKey.GetValue("ParentKeyName") != null)
                    continue;

                // Make sure it has a display name
                if (subKey.GetValue("DisplayName") is not string dn)
                    continue;

                // Make sure it has an install location
                if (subKey.GetValue("InstallLocation") is not string dir)
                    continue;

                programDirs[dn.ToLowerInvariant()] = dir.
                    // Replace the separator character as Uplay games use forward slashes
                    Replace("/", @"\");
            }
        }

        return programDirs;
    }

    public override void Run(FinderItem[] finderItems)
    {
        // Make sure there are queries to handle
        if (finderItems.SelectMany(x => x.Queries).All(x => x is not UninstallProgramFinderQuery))
            return;

        // Get the program directories
        Dictionary<string, string> programDirs = GetProgramDirectories();

        // Enumerate each finder query
        foreach (FinderItem finderItem in finderItems)
        {
            foreach (UninstallProgramFinderQuery query in finderItem.Queries.OfType<UninstallProgramFinderQuery>())
            {
                if (finderItem.HasBeenFound)
                    break;

                // Attempt to get the install location
                string? location = programDirs.TryGetValue(query.DisplayName.ToLowerInvariant());

                // Make sure we got a location
                if (location.IsNullOrWhiteSpace())
                    continue;

                // Validate the location
                finderItem.Validate(query, location);
            }
        }
    }
}