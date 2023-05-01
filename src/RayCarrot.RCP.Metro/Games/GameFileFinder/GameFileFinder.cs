using System.IO;

namespace RayCarrot.RCP.Metro.Games.GameFileFinder;

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

        foreach (FileSystemPath filePath in Directory.EnumerateFiles(DirectoryPath, "*", SearchOption))
        {
            if (ExcludedFiles.Contains(filePath))
                continue;

            FileExtension ext = filePath.FileExtension;
            InstallLocation location = InstallLocation.FromFilePath(filePath);

            foreach (GameFileFinderItem finderItem in FinderItems)
            {
                if (finderItem.RomStructure.SupportedFileExtensions.All(x => x != ext))
                    continue;

                GameLocationValidationResult result = finderItem.RomStructure.IsLocationValid(location);

                if (result.IsValid)
                {
                    finderItem.SetLocation(location);
                    break;
                }
            }
        }

        Logger.Info("The game file finder found {0} items", FinderItems.Count(x => x.HasBeenFound));
    }
}