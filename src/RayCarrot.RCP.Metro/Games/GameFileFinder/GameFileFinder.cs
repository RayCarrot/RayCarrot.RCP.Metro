using System.IO;

namespace RayCarrot.RCP.Metro.Games.GameFileFinder;

// TODO: This is rather un-optimized as it'll read the same file multiple times. We should group based on type.

/// <summary>
/// Finder for finding single-file games, such as ROMs and discs
/// </summary>
public class GameFileFinder
{
    public GameFileFinder(
        FileSystemPath directoryPath, 
        SearchOption searchOption, 
        IReadOnlyList<FileSystemPath> excludedFiles, 
        IReadOnlyList<GameFileFinderItem> finderItems)
    {
        DirectoryPath = directoryPath;
        SearchOption = searchOption;
        ExcludedFiles = excludedFiles;
        FinderItems = finderItems;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public FileSystemPath DirectoryPath { get; }
    public SearchOption SearchOption { get; }
    public IReadOnlyList<FileSystemPath> ExcludedFiles { get; }
    public IReadOnlyList<GameFileFinderItem> FinderItems { get; }

    public void Run()
    {
        Logger.Info("Running the game file finder with {0} finder items", FinderItems.Count);

        foreach (FileSystemPath filePath in DirectoryHelpers.EnumerateDirectoriesSafe(DirectoryPath, "*", SearchOption))
        {
            if (ExcludedFiles.Contains(filePath))
                continue;

            FileExtension ext = filePath.FileExtension;
            InstallLocation location = InstallLocation.FromFilePath(filePath);

            foreach (GameFileFinderItem finderItem in FinderItems)
            {
                if (finderItem.Structure.SupportedFileExtensions.All(x => x != ext))
                    continue;

                // Validate the location
                GameLocationValidationResult result = finderItem.Structure.IsLocationValid(location);
                if (!result.IsValid) 
                    continue;

                // Validate the layout if any are defined
                if (finderItem.Structure.HasLayouts && finderItem.Structure.FindMatchingLayout(location) == null)
                    continue;
                
                finderItem.AddLocation(location);
            }
        }

        Logger.Info("The game file finder found {0} items", FinderItems.Count(x => x.HasBeenFound));
    }
}