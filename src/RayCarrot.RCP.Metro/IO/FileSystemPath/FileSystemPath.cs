using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NLog;

namespace RayCarrot.RCP.Metro;

// TODO: Move to separate library? This was originally a part of the RayCarrot.IO library but was moved into RCP with version 13.0
// TODO: Use Path.DirectorySeparatorChar
// TODO: Clean up and improve performance. Remove IsRelative bool.
/// <summary>
/// A path on a file system
/// </summary>
[Serializable]
[TypeConverter(typeof(FileSystemPathConverter))]
[DebuggerDisplay("Path = {FullPath}, Type = {FileSystemType}")]
public struct FileSystemPath : ISerializable
{
    #region Constructors

    /// <summary>
    /// Creates a new <see cref="FileSystemPath"/> from a string
    /// </summary>
    /// <param name="path">The path of the file or directory</param>
    /// <param name="relative">True if the path is relative, false if it's not</param>
    public FileSystemPath(string? path, bool relative)
    {
        if (path.IsNullOrWhiteSpace())
            path = String.Empty;

        IsRelative = relative;
        _fullPath = IsRelative ? path.Trim() : NormalizePath(path.Trim());
    }

    /// <summary>
    /// Creates a new <see cref="FileSystemPath"/> from a string
    /// </summary>
    /// <param name="path">The path of the file or directory</param>
    public FileSystemPath(string? path)
    {
        if (path.IsNullOrWhiteSpace())
            path = String.Empty;

        IsRelative = !path.Contains(Path.VolumeSeparatorChar.ToString());
        _fullPath = IsRelative ? path.Trim() : NormalizePath(path.Trim());
    }

    /// <summary>
    /// Creates a new <see cref="FileSystemPath"/> from serialization info
    /// </summary>
    /// <param name="info">The serialization info</param>
    /// <param name="context">The streaming context</param>
    public FileSystemPath(SerializationInfo info, StreamingContext context)
    {
        _fullPath = info.GetValue<string>(nameof(FullPath));
        IsRelative = info.GetValue<bool>(nameof(IsRelative));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly string _fullPath;

    #endregion

    #region Public Properties

    /// <summary>
    /// The full normalized path
    /// </summary>
    public string FullPath => _fullPath ?? String.Empty;

    /// <summary>
    /// True if the path is relative, false if it's not
    /// </summary>
    public bool IsRelative { get; }

    /// <summary>
    /// The type of path
    /// </summary>
    public FileSystemType FileSystemType
    {
        get
        {
            if (IsRelative)
                return FileSystemType.Relative;

            if (File.Exists(FullPath))
                return FileSystemType.File;

            else if (Directory.Exists(FullPath))
                return FileSystemType.Directory;

            else
                return FileSystemType.Unknown;
        }
    }

    /// <summary>
    /// The name of the file or directory, or <see cref="String.Empty"/> if one could not be found
    /// </summary>
    public string Name => GetPathName(FullPath);

    /// <summary>
    /// The parent directory or an empty path if one was not found
    /// </summary>
    public FileSystemPath Parent => GetParentPath(FullPath);

    /// <summary>
    /// True if the path is an existing file or directory,
    /// false if it's not found or if it's a relative path
    /// </summary>
    public bool Exists => !IsRelative && (FileSystemType == FileSystemType.Directory || FileSystemType == FileSystemType.File);

    /// <summary>
    /// True if the path is an existing file.
    /// Returns false if it is relative.
    /// </summary>
    public bool FileExists => !IsRelative && FileSystemType == FileSystemType.File;

    /// <summary>
    /// True if the path is an existing directory.
    /// Returns false if it is relative.
    /// </summary>
    public bool DirectoryExists => !IsRelative && FileSystemType == FileSystemType.Directory;

    /// <summary>
    /// Gets the file extensions for the path
    /// </summary>
    [Obsolete("Use FileSystemPath.FileExtension instead as support for multiple file extensions will be removed")]
    public FileExtension FileExtensions => new FileExtension(Name, multiple: true);

    /// <summary>
    /// Gets the file extension for the path
    /// </summary>
    public FileExtension FileExtension => new FileExtension(Name);

    /// <summary>
    /// Checks if the path is a root of a drive (such as C:\, D:\ etc.)
    /// </summary>
    /// <returns>True if the path is a directory root, false if not</returns>
    public bool IsDirectoryRoot => IsPathDirectoryRoot(FullPath);

    #endregion

    #region Private Static Methods

    /// <summary>
    /// Normalizes a path
    /// </summary>
    /// <param name="path">The path to normalize</param>
    /// <returns></returns>
    private static string NormalizePath(string path)
    {
        // Make sure the path is not empty
        if (path.IsNullOrWhiteSpace())
            return path;

        // Trim the path
        path = path.Trim(' ', Path.DirectorySeparatorChar);

        // Re-add backslash if a drive
        if (path.EndsWith(Path.VolumeSeparatorChar.ToString()))
            path += Path.DirectorySeparatorChar;

        // Expand the environmental variables
        path = Environment.ExpandEnvironmentVariables(path);

        // TODO: Allow this to be configured
        // Correct the path casing if set to do so
        //if (false)
        //    path = CorrectPathCasing(path);

        // Return the path
        return path;
    }

    /// <summary>
    /// Gets the parent directory or <see cref="String.Empty"/> if one was not found
    /// </summary>
    /// <param name="fullPath">The path to get the parent directory for</param>
    /// <returns>The parent directory or <see cref="String.Empty"/> if one was not found</returns>
    private static string GetParentPath(string fullPath)
    {
        try
        {
            var path = Path.GetDirectoryName(fullPath);

            if (path.IsNullOrWhiteSpace())
                return String.Empty;

            return path;
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting parent directory for {0}", fullPath);
            return String.Empty;
        }
    }

    /// <summary>
    /// Gets the name of the file or directory, or <see cref="String.Empty"/> if one could not be found
    /// </summary>
    /// <param name="path">The path to get the name for</param>
    /// <returns>The name of the file or directory, or <see cref="String.Empty"/> if one could not be found</returns>
    private static string GetPathName(string path)
    {
        try
        {
            return Path.GetFileName(path);
        }
        catch (ArgumentException ex)
        {
            Logger.Warn(ex, "Getting file name for {0}", path);
            return String.Empty;
        }
    }

    /// <summary>
    /// Checks if the path is a root of a drive (such as C:\, D:\ etc.)
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>True if the path is a directory root, false if not</returns>
    private static bool IsPathDirectoryRoot(string path)
    {
        if (!Directory.Exists(path))
            return false;

        try
        {
            return new DirectoryInfo(path).Parent == null;
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting directory info for root checking for {0}", path);
            return false;
        }
    }

    /// <summary>
    /// Corrects the character casing for a path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The same path, but with the correct path casing</returns>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    private static string CorrectPathCasing(string path)
    {
        try
        {
            // Get the absolute path
            path = Path.GetFullPath(path);
        }
        catch (ArgumentNullException ex)
        {
            Logger.Warn(ex, "Getting absolute path for {0}", path);

            // The path is null
            return path;
        }
        catch (ArgumentException ex)
        {
            Logger.Warn(ex, "Getting absolute path for {0}", path);

            // The path contains an invalid character
            return path;
        }
        catch (NotSupportedException ex)
        {
            Logger.Warn(ex, "Getting absolute path for {0}", path);

            // The path contains an extra colon
            return path;
        }

        // Return the path if it doesn't exist
        if (!(File.Exists(path) || Directory.Exists(path)))
            return path;

        // If it's a root, return the path to upper case
        if (IsPathDirectoryRoot(path))
            return path.ToUpper();

        // Get the parent
        var parent = GetParentPath(path);

        // Get the path name
        var name = GetPathName(path);

        // If it's not a root, get the path name through a GetFileSystemInfos call, combined with a recursive call of its parent
        return Path.Combine(CorrectPathCasing(parent), new DirectoryInfo(parent).GetFileSystemInfos(name).First().Name);
    }

    #endregion

    #region Public Methods

    public bool ContainsPath(FileSystemPath basePath)
    {
        return FullPath.StartsWith(basePath);
    }

    /// <summary>
    /// Corrects the character casing
    /// </summary>
    /// <returns>The same path, but with the correct path casing</returns>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    public FileSystemPath CorrectPathCasing() => CorrectPathCasing(FullPath);

    /// <summary>
    /// Checks if the other instance is equals to the current one
    /// </summary>
    /// <param name="other">The other instance to compare to the current one</param>
    /// <returns>True if the other instance is equals to the current one, false if not</returns>
    public bool Equals(FileSystemPath other)
    {
        string a = FullPath;
        string b = other.FullPath;

        return a == b && other.IsRelative == IsRelative;
    }

    #endregion

    #region Static Implicit Operators

    /// <summary>
    /// Creates a new <see cref="FileSystemPath"/> from a <see cref="String"/>
    /// </summary>
    /// <param name="path">The path as a <see cref="String"/></param>
    public static implicit operator FileSystemPath(string? path) =>
        new FileSystemPath(path);

    /// <summary>
    /// Creates a new <see cref="String"/> from a <see cref="FileSystemPath"/>
    /// </summary>
    /// <param name="path">The path as a <see cref="FileSystemPath"/></param>
    public static implicit operator string(FileSystemPath path) =>
        path.FullPath;

    #endregion

    #region Static Operators

    /// <summary>
    /// Checks if the two paths are the same
    /// </summary>
    /// <param name="a">The first path</param>
    /// <param name="b">The second path</param>
    /// <returns>True if they are the same, false if not</returns>
    public static bool operator ==(FileSystemPath a, FileSystemPath b)
    {
        return a.Equals((object)b);
    }

    /// <summary>
    /// Checks if the two paths are not the same
    /// </summary>
    /// <param name="a">The first path</param>
    /// <param name="b">The second path</param>
    /// <returns>True if they are not the same, false if they are</returns>
    public static bool operator !=(FileSystemPath a, FileSystemPath b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Checks if the <see cref="FileSystemPath"/> is the same path as the specified <see cref="String"/>
    /// </summary>
    /// <param name="a">The first path, as a <see cref="FileSystemPath"/></param>
    /// <param name="b">The second path, as a <see cref="String"/></param>
    /// <returns>True if they are the same, false if not</returns>
    public static bool operator ==(FileSystemPath a, string b)
    {
        return a.FullPath == b;
    }

    /// <summary>
    /// Checks if the <see cref="FileSystemPath"/> is not the same path as the specified <see cref="String"/>
    /// </summary>
    /// <param name="a">The first path, as a <see cref="FileSystemPath"/></param>
    /// <param name="b">The second path, as a <see cref="String"/></param>
    /// <returns>True if they are not the same, false if they are</returns>
    public static bool operator !=(FileSystemPath a, string b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Adds a relative path to the existing <see cref="FileSystemPath"/>
    /// </summary>
    /// <param name="a">The base path</param>
    /// <param name="b">The relative path to add to the base path</param>
    /// <returns>The combined path</returns>
    /// <exception cref="ArgumentException">One of the paths contains one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/></exception>
    public static FileSystemPath operator +(FileSystemPath a, FileSystemPath b)
    {
        return Path.Combine(a.FullPath, b.FullPath);
    }

    /// <summary>
    /// Adds a relative path as a <see cref="String"/> to the existing <see cref="FileSystemPath"/>
    /// </summary>
    /// <param name="a">The base path</param>
    /// <param name="b">The relative path to add to the base path, as a <see cref="String"/></param>
    /// <returns>The combined path</returns>
    /// <exception cref="ArgumentException">One of the paths contains one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/></exception>
    /// <exception cref="ArgumentNullException"/>
    public static FileSystemPath operator +(FileSystemPath a, string b)
    {
        if (b == null)
            throw new ArgumentNullException(nameof(b));

        return Path.Combine(a.FullPath, b);
    }

    /// <summary>
    /// Gets the relative path from a full path based on the specified base path
    /// </summary>
    /// <param name="path">The path to get the relative path from. Must be an absolute, rooted path.</param>
    /// <param name="basePath">The base path to use when getting the relative path. Must be an absolute, rooted path.</param>
    /// <returns>The relative path</returns>
    /// <exception cref="ArgumentException">The specified path does not contain the base path</exception>
    /// <exception cref="IOException">The path or basePath is not an absolute, rooted path</exception>
    public static FileSystemPath operator -(FileSystemPath path, FileSystemPath basePath)
    {
        if (!Path.IsPathRooted(path.FullPath.Trim('\\')))
            throw new IOException("The path is not an absolute, rooted path");

        if (!Path.IsPathRooted(basePath.FullPath.Trim('\\')))
            throw new IOException("The basePath is not an absolute, rooted path");

        if (!path.ContainsPath(basePath))
            throw new ArgumentException("The specified path does not contain the base path", nameof(basePath));

        return path.FullPath.Remove(0, basePath.FullPath.Length).Trim('\\');
    }

    /// <summary>
    /// Gets the relative path from a full path as a <see cref="String"/> based on the specified base path
    /// </summary>
    /// <param name="path">The path to get the relative path from as a <see cref="String"/>. Must be an absolute, rooted path.</param>
    /// <param name="basePath">The base path to use when getting the relative path. Must be an absolute, rooted path.</param>
    /// <returns>The relative path</returns>
    /// <exception cref="ArgumentException">The specified path does not contain the base path</exception>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="IOException">The path or basePath is not an absolute, rooted path</exception>
    public static FileSystemPath operator -(FileSystemPath path, string basePath)
    {
        if (basePath == null)
            throw new ArgumentNullException(nameof(basePath));

        return path - new FileSystemPath(basePath);
    }

    #endregion

    #region Public Static Properties

    /// <summary>
    /// An empty path
    /// </summary>
    public static FileSystemPath EmptyPath => String.Empty;

    #endregion

    #region Overrides

    /// <summary>
    /// Returns the full path
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return FullPath;
    }

    /// <summary>
    /// True if the specified object equals the current instance
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        return obj is FileSystemPath path && Equals(path);
    }

    /// <summary>
    /// Returns the hash code for this instance
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance</returns>
    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + FullPath.GetHashCode();
        hash = (hash * 7) + IsRelative.GetHashCode();
        return hash;
    }

    #endregion

    #region Interface Implementations

    /// <summary>
    /// Get object data for serializing
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue<string>(nameof(FullPath), FullPath);
        info.AddValue<bool>(nameof(IsRelative), IsRelative);
    }

    #endregion
}