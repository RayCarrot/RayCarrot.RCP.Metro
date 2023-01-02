using System.IO;

namespace RayCarrot.RCP.Metro.Games.Finder;

/// <summary>
/// A finder operation for handling <see cref="Win32ShortcutFinderQuery"/> queries
/// </summary>
public class Win32ShortcutFinderOperation : FinderOperation
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static string[] GetShortcutsSafe(FileSystemPath directory, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        try
        {
            // Get the shortcut files from the specified directory
            return Directory.GetFiles(directory, "*.lnk", searchOption);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting shortcut files for finder in {0}", directory);

            // Return an empty array
            return Array.Empty<string>();
        }
    }

    private static List<string> GetShortcutFiles()
    {
        List<string> files = new();

        // User start menu
        files.AddRange(GetShortcutsSafe(Environment.SpecialFolder.StartMenu.GetFolderPath(), SearchOption.AllDirectories));

        // Common start menu
        files.AddRange(GetShortcutsSafe(Environment.SpecialFolder.CommonStartMenu.GetFolderPath(), SearchOption.AllDirectories));

        // User desktop
        files.AddRange(GetShortcutsSafe(Environment.SpecialFolder.DesktopDirectory.GetFolderPath()));

        // Common desktop
        files.AddRange(GetShortcutsSafe(Environment.SpecialFolder.CommonDesktopDirectory.GetFolderPath()));

        return files;
    }

    public override void Run(FinderItem[] finderItems)
    {
        // Make sure there are queries to handle
        if (finderItems.SelectMany(x => x.Queries).All(x => x is not Win32ShortcutFinderQuery))
            return;

        List<string> shortcutFiles = GetShortcutFiles();

        // Enumerate each finder query
        foreach (FinderItem finderItem in finderItems)
        {
            foreach (Win32ShortcutFinderQuery query in finderItem.Queries.OfType<Win32ShortcutFinderQuery>())
            {
                if (finderItem.HasBeenFound)
                    break;

                // Enumerate every shortcut file
                foreach (string shortcutFile in shortcutFiles)
                {
                    // Get the file name
                    string? fileName = Path.GetFileNameWithoutExtension(shortcutFile);

                    // Make sure we got a file
                    if (fileName == null)
                        continue;
                    
                    // Check if the file name contains the name provided by the query
                    if (fileName.IndexOf(query.Name, StringComparison.InvariantCultureIgnoreCase) == -1)
                        continue;

                    FileSystemPath targetDir;

                    try
                    {
                        // Attempt to get the shortcut target path
                        targetDir = ((FileSystemPath)WindowsHelpers.GetShortCutTarget(shortcutFile)).Parent;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Getting file shortcut target for finder {0}", shortcutFile);
                        continue;
                    }

                    // Validate the location
                    finderItem.Validate(query, targetDir);
                }
            }
        }
    }
}