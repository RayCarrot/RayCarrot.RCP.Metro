#nullable disable
using System.IO;
using System.Security.Cryptography;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="FileSystemPath"/>
/// </summary>
public static class FileSystemPathExtensions
{
    private static FileSystemPath VirtualStorePath { get; } = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "VirtualStore";

    /// <summary>
    /// Gets the file system info for the file or directory
    /// </summary>
    /// <returns>The file system info, either a <see cref="GetFileInfo"/> or a <see cref="DirectoryInfo"/></returns>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission</exception>
    /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters</exception>
    /// <exception cref="UnauthorizedAccessException">Access to the path is denied</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    /// <exception cref="NotSupportedException">The path contains a colon (:) in the middle of the string</exception>
    public static FileSystemInfo GetFileSystemInfo(this FileSystemPath path)
    {
        switch (path.FileSystemType)
        {
            case FileSystemType.File:
                return path.GetFileInfo();

            case FileSystemType.Directory:
                return path.GetDirectoryInfo();

            case FileSystemType.Relative:
            case FileSystemType.Unknown:
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets the file info
    /// </summary>
    /// <returns>The file info</returns>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission</exception>
    /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters</exception>
    /// <exception cref="UnauthorizedAccessException">Access to the path is denied</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    /// <exception cref="NotSupportedException">The path contains a colon (:) in the middle of the string</exception>
    public static FileInfo GetFileInfo(this FileSystemPath path) => new FileInfo(path);

    /// <summary>
    /// Gets the directory info
    /// </summary>
    /// <returns>The directory info</returns>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission</exception>
    /// <exception cref="ArgumentException">The path contains invalid characters such as ", &lt;, &gt;, or |</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    public static DirectoryInfo GetDirectoryInfo(this FileSystemPath path) => new DirectoryInfo(path);

    /// <summary>
    /// Gets the size of a file system path file or directory in bytes
    /// </summary>
    /// <param name="path">The path of the file or directory</param>
    /// <returns>The size, or 0 if not found</returns>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission</exception>
    /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters</exception>
    /// <exception cref="UnauthorizedAccessException">Access to the path is denied</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    /// <exception cref="NotSupportedException">The path contains a colon (:) in the middle of the string</exception>
    /// <exception cref="IOException">A device such as a disk drive is not ready</exception>
    public static long GetSize(this FileSystemPath path)
    {
        if (path.FileExists)
            return path.GetFileInfo().Length;
        else if (path.DirectoryExists)
            return GetDirectorySize(path);
        else
            return default;

        // Recursive method for getting the size of a directory
        static long GetDirectorySize(FileSystemPath dirPath)
        {
            DirectoryInfo dirInfo = dirPath.GetDirectoryInfo();
            long size = 0;

            // Add file sizes
            size = dirInfo.GetFiles().Aggregate(size, (current, fi) => current + fi.Length);

            // Add sub directory sizes
            size = dirInfo.GetDirectories().Aggregate(size, (current, di) => current + GetDirectorySize(di.FullName));

            return size;
        }
    }

    /// <summary>
    /// Removes the root from an absolute path
    /// </summary>
    /// <param name="path">The path to remove the root from. Must be an absolute, rooted path.</param>
    /// <returns>The path without the root</returns>
    /// <exception cref="IOException">The path or basePath is not an absolute, rooted path</exception>
    public static string RemoveRoot(this FileSystemPath path)
    {
        if (!Path.IsPathRooted(path.FullPath.Trim('\\')))
            throw new IOException("The path is not an absolute, rooted path");

        try
        {
            return path.FullPath.Substring(Path.GetPathRoot(path.FullPath).Length);
        }
        catch (Exception ex)
        {
            throw new IOException("The path is not an absolute, rooted path", ex);
        }
    }

    /// <summary>
    /// Replaces the current file extension with the provided one
    /// </summary>
    /// <param name="fileSystemPath">The path</param>
    /// <param name="extension">The file extension</param>
    /// <param name="includeMultipleExtensions">Indicates if all extensions should be replaced if there are multiple file extensions</param>
    /// <returns>The file path with the specified extension</returns>
    /// <exception cref="ArgumentNullException"/>
    public static FileSystemPath ChangeFileExtension(this FileSystemPath fileSystemPath, FileExtension extension, bool includeMultipleExtensions = false)
    {
        if (extension == null)
            throw new ArgumentNullException(nameof(extension));
            
        return fileSystemPath.RemoveFileExtension(includeMultipleExtensions).AppendFileExtension(extension);
    }

    /// <summary>
    /// Appends a file extension to the file
    /// </summary>
    /// <param name="fileSystemPath">The path</param>
    /// <param name="extension">The file extension</param>
    /// <returns>The file path with the specified extension appended</returns>
    /// <exception cref="ArgumentNullException"/>
    public static FileSystemPath AppendFileExtension(this FileSystemPath fileSystemPath, FileExtension extension)
    {
        if (extension == null)
            throw new ArgumentNullException(nameof(extension));
            
        return fileSystemPath.FullPath + extension.FileExtensions;
    }

    /// <summary>
    /// Removes the file extension if there is one
    /// </summary>
    /// <param name="fileSystemPath">The path</param>
    /// <param name="includeMultipleExtensions">Indicates if all extensions should be removed if there are multiple file extensions</param>
    /// <returns>The file path without the extension, or the file path if no extension was found</returns>
    public static FileSystemPath RemoveFileExtension(this FileSystemPath fileSystemPath, bool includeMultipleExtensions = false)
    {
        if (!fileSystemPath.FileExtensions.AllFileExtensions.Any())
            return fileSystemPath;

        var fileExtSize = includeMultipleExtensions
            ? fileSystemPath.FileExtensions.FileExtensions.Length
            : fileSystemPath.FileExtensions.PrimaryFileExtension.Length;

        return fileSystemPath.FullPath.Substring(0, fileSystemPath.FullPath.Length - fileExtSize);
    }

    /// <summary>
    /// Returns true if all directories in the list exist
    /// </summary>
    /// <param name="directories"></param>
    /// <returns>True if all directories exist, false if not</returns>
    /// <exception cref="ArgumentNullException"/>
    public static bool DirectoriesExist(this IEnumerable<FileSystemPath> directories)
    {
        if (directories == null)
            throw new ArgumentNullException(nameof(directories));

        return directories.All(dir => dir.FileSystemType == FileSystemType.Directory);
    }

    /// <summary>
    /// Returns true if all files in the list exist
    /// </summary>
    /// <param name="files"></param>
    /// <returns>True if all files exist, false if not</returns>
    /// <exception cref="ArgumentNullException"/>
    public static bool FilesExist(this IEnumerable<FileSystemPath> files)
    {
        if (files == null)
            throw new ArgumentNullException(nameof(files));

        return files.All(file => file.FileSystemType == FileSystemType.File);
    }

    /// <summary>
    /// Returns a non-existing file name based on the given file name in the same directory
    /// </summary>
    /// <param name="filePath">The file path to base the new path from</param>
    /// <returns>The non-existing file path</returns>
    public static FileSystemPath GetNonExistingFileName(this FileSystemPath filePath)
    {
        if (!filePath.Exists)
            return filePath;

        int index = 1;

        FileSystemPath newFilePath;

        do
        {
            newFilePath = $"{filePath.RemoveFileExtension().FullPath} ({index++})";
            newFilePath = newFilePath.AppendFileExtension(filePath.FileExtensions);
        } while (newFilePath.Exists);

        return newFilePath;
    }

    /// <summary>
    /// Returns a non-existing directory name based on the given directory name in the same parent directory
    /// </summary>
    /// <param name="dirPath">The directory path to base the new path from</param>
    /// <returns>The non-existing directory path</returns>
    public static FileSystemPath GetNonExistingDirectoryName(this FileSystemPath dirPath)
    {
        if (!dirPath.Exists)
            return dirPath;

        int index = 1;

        FileSystemPath newDirPath;

        do
        {
            newDirPath = $"{dirPath.FullPath} ({index++})";
        } while (newDirPath.Exists);

        return newDirPath;
    }

    /// <summary>
    /// Gets the SHA256 checksum for a file
    /// </summary>
    /// <param name="filePath">The file to get the checksum for</param>
    /// <returns>The checksum as a string</returns>
    public static string GetSHA256CheckSum(this FileSystemPath filePath)
    {
        using FileStream fileStream = File.OpenRead(filePath);
        using BufferedStream bufferedStream = new(File.OpenRead(filePath), Math.Min(1200000, (int)fileStream.Length));
        using SHA256Managed sha = new();
        
        byte[] checksum = sha.ComputeHash(bufferedStream);
        return BitConverter.ToString(checksum).Replace("-", String.Empty);
    }

    /// <summary>
    /// Gets the SHA256 checksum for a collection of files
    /// </summary>
    /// <param name="filePaths">The files to get the checksum for</param>
    /// <returns>The checksum as a string</returns>
    public static string GetSHA256CheckSum(this IEnumerable<FileSystemPath> filePaths)
    {
        using (SHA256Managed sha = new SHA256Managed())
        {
            // Get a collection of the hashes
            var hashes = new List<byte[]>();

            // Get the hash for every file
            foreach (var file in filePaths)
            {
                using (var stream = new BufferedStream(File.OpenRead(file), 1200000))
                    hashes.Add(sha.ComputeHash(stream));
            }

            // Get the total hash
            return BitConverter.ToString(sha.ComputeHash(hashes.SelectMany(x => x).ToArray())).Replace("-", String.Empty);
        }
    }

    /// <summary>
    /// Deletes a file if it exists
    /// </summary>
    /// <param name="filePath">The file path</param>
    public static void DeleteFile(this FileSystemPath filePath)
    {
        // Check if the file exists
        if (!filePath.FileExists)
            return;

        // Get the file info
        FileInfo info = filePath.GetFileInfo();

        // Make sure it's not read-only
        info.IsReadOnly = false;

        // Delete the file
        info.Delete();
    }

    /// <summary>
    /// Deletes a directory recursively if it exists
    /// </summary>
    /// <param name="dirPath">The directory path</param>
    public static void DeleteDirectory(this FileSystemPath dirPath)
    {
        // Check if the directory exists
        if (!dirPath.DirectoryExists)
            return;

        // Use "\\?\" to support long paths. TODO: Maybe we should do that elsewhere too? Is there a drawback to it?
        Directory.Delete($@"\\?\{dirPath}", true);
    }

    public static FileSystemPath GetVirtualStorePath(this FileSystemPath path)
    {
        return VirtualStorePath + path.RemoveRoot();
    }
}