using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Search pattern to use for IO operations
    /// </summary>
    public class IOSearchPattern
    {
        /// <summary>
        /// Constructor for using the entire directory
        /// </summary>
        /// <param name="dirPath">The directory path</param>
        public IOSearchPattern(FileSystemPath dirPath)
        {
            DirPath = dirPath;
            SearchOption = SearchOption.AllDirectories;
            SearchPattern = "*";
        }

        /// <summary>
        /// Constructor for specifying the search options
        /// </summary>
        /// <param name="dirPath">The directory path</param>
        /// <param name="searchOption">The search option to use when finding files and sub directories</param>
        /// <param name="searchPattern">The search pattern to use when finding files and sub directories</param>
        public IOSearchPattern(FileSystemPath dirPath, SearchOption searchOption, string searchPattern)
        {
            DirPath = dirPath;
            SearchOption = searchOption;
            SearchPattern = searchPattern;
        }

        /// <summary>
        /// The directory path
        /// </summary>
        public FileSystemPath DirPath { get; }

        /// <summary>
        /// The search option to use when finding files and sub directories
        /// </summary>
        public SearchOption SearchOption { get; }

        /// <summary>
        /// The search pattern to use when finding files and sub directories
        /// </summary>
        public string SearchPattern { get; }

        /// <summary>
        /// Gets a value indicating if the current instance should include the entire directory and its sub content
        /// </summary>
        /// <returns>True if the current instance should include the entire directory and its sub content, otherwise false</returns>
        public bool IsEntireDir() => (SearchPattern == null || SearchPattern == "*") && SearchOption == SearchOption.AllDirectories;
    }
}