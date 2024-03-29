﻿namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// An archive directory, containing an array of <see cref="FileItem"/>
/// </summary>
public class ArchiveDirectory
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="directoryName">The full path of the directory, or empty if the root</param>
    /// <param name="files">The files in the directory</param>
    public ArchiveDirectory(string directoryName, FileItem[] files)
    {
        DirectoryName = directoryName;
        Files = files;
    }

    /// <summary>
    /// The full path of the directory, or empty if the root
    /// </summary>
    public string DirectoryName { get; }

    /// <summary>
    /// The files in the directory
    /// </summary>
    public FileItem[] Files { get; }
}