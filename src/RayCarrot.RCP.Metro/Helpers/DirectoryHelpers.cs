using System.IO;

namespace RayCarrot.RCP.Metro;

public static class DirectoryHelpers
{
    /// <summary>
    /// Enumerates directories in a specific path without throwing on <see cref="UnauthorizedAccessException"/> or <see cref="PathTooLongException"/>
    /// </summary>
    /// <param name="path">The path to get the directories for</param>
    /// <param name="searchPattern">The search pattern to use</param>
    /// <param name="searchOption">The search option</param>
    /// <returns>The found directories</returns>
    public static IEnumerable<string> EnumerateDirectoriesSafe(string path, string searchPattern, SearchOption searchOption)
    {
        if (searchOption == SearchOption.AllDirectories)
        {
            string[]? directories = null;

            try
            {
                directories = Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }

            if (directories != null)
            {
                foreach (string dir in directories)
                {
                    foreach (string file in EnumerateDirectoriesSafe(dir, searchPattern, searchOption))
                    {
                        yield return file;
                    }
                }
            }
        }

        string[] files;

        try
        {
            files = Directory.GetFiles(path, searchPattern);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }

        foreach (string file in files)
            yield return file;
    }
}