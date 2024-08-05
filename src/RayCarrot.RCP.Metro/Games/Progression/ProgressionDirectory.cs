using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionDirectory
{
    #region Constructor

    /// <summary>
    /// Constructor with parameters for each search pattern value
    /// </summary>
    /// <param name="dirPath">The directory path</param>
    /// <param name="searchOption">The search option to use when finding files and sub directories</param>
    /// <param name="searchPattern">The search pattern to use when finding files and sub directories</param>
    public ProgressionDirectory(FileSystemPath dirPath, SearchOption searchOption, string? searchPattern)
    {
        DirPath = dirPath;
        SearchOption = searchOption;
        SearchPattern = searchPattern ?? "*";
        
        if (dirPath != FileSystemPath.EmptyPath)
            VirtualStoreDirPath = DirPath.GetVirtualStorePath();
        
        HasVirtualStoreVersion = VirtualStoreDirPath.DirectoryExists;
    }

    /// <summary>
    /// Constructor with parameters for a search pattern
    /// </summary>
    /// <param name="searchPattern">The search pattern</param>
    public ProgressionDirectory(IOSearchPattern searchPattern) : this(searchPattern.DirPath, searchPattern.SearchOption, searchPattern.SearchPattern) 
    { }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    /// <summary>
    /// The directory path
    /// </summary>
    private FileSystemPath DirPath { get; }

    private FileSystemPath VirtualStoreDirPath { get; }

    /// <summary>
    /// The search option to use when finding files and sub directories
    /// </summary>
    private SearchOption SearchOption { get; }

    /// <summary>
    /// The search pattern to use when finding files and sub directories
    /// </summary>
    private string SearchPattern { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if there is a VirtualStore version of this directory
    /// </summary>
    public bool HasVirtualStoreVersion { get; }

    #endregion

    #region Private Methods

    private static DateTime GetLastWriteTime(IOSearchPattern dir) => dir.GetFiles().
        Select(x => new FileInfo(x).LastWriteTime).
        OrderByDescending(x => x).
        FirstOrDefault();

    private FileSystemPath GetBackupDirPath(ProgramDataSource source)
    {
        Logger.Trace("Getting directory for {0} with source {1}", DirPath, source);

        if (source == ProgramDataSource.Default)
        {
            Logger.Trace("Determined to use default directory based on source value");
            return DirPath;
        }

        if (source == ProgramDataSource.VirtualStore)
        {
            Logger.Trace("Determined to use VirtualStore directory based on source value");
            return VirtualStoreDirPath;
        }

        if (!VirtualStoreDirPath.DirectoryExists)
        {
            Logger.Trace("Determined to use default directory due to the VirtualStore directory not existing");
            return DirPath;
        }

        if (!DirPath.DirectoryExists)
        {
            Logger.Trace("Determined to use VirtualStore directory due to the default directory not existing");
            return VirtualStoreDirPath;
        }

        try
        {
            DateTime originalTime = GetLastWriteTime(new IOSearchPattern(DirPath, SearchOption, SearchPattern));
            DateTime virtualStoreTime = GetLastWriteTime(new IOSearchPattern(VirtualStoreDirPath, SearchOption, SearchPattern));

            if (originalTime > virtualStoreTime)
            {
                Logger.Trace("Determined to use default directory based on last write time");
                return DirPath;
            }
            else
            {
                Logger.Trace("Determined to use VirtualStore directory based on last write time");
                return VirtualStoreDirPath;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Determining directory to use based on last write time");

            // Use the VirtualStore path in case of error since this is most likely to work then
            return VirtualStoreDirPath;
        }
    }

    #endregion

    #region Public Methods

    public IOSearchPattern GetReadSearchPattern(ProgramDataSource source)
    {
        // Find and return a single matching directory path to use
        return new IOSearchPattern(GetBackupDirPath(source), SearchOption, SearchPattern);
    }

    public IEnumerable<IOSearchPattern> GetWriteSearchPatterns(ProgramDataSource source)
    {
        if (source == ProgramDataSource.Auto)
        {
            // Return both paths if available. If the source if auto then we want to write to both locations.
            yield return new IOSearchPattern(DirPath, SearchOption, SearchPattern);

            if (HasVirtualStoreVersion)
                yield return new IOSearchPattern(VirtualStoreDirPath, SearchOption, SearchPattern);
        }
        else
        {
            yield return GetReadSearchPattern(source);
        }
    }

    #endregion
}