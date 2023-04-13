using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A program install location
/// </summary>
public readonly struct InstallLocation
{
    /// <summary>
    /// Creates a new <see cref="InstallLocation"/> from a directory path
    /// </summary>
    /// <param name="directory">The install directory path</param>
    public InstallLocation(FileSystemPath directory)
    {
        Directory = directory;
    }

    /// <summary>
    /// Creates a new <see cref="InstallLocation"/> from a directory path and a file name
    /// </summary>
    /// <param name="directory">The directory path</param>
    /// <param name="fileName">The program file name in the directory</param>
    [JsonConstructor]
    public InstallLocation(FileSystemPath directory, string? fileName)
    {
        Directory = directory;
        FileName = fileName;
    }

    public FileSystemPath Directory { get; }
    public string? FileName { get; }

    [MemberNotNullWhen(true, nameof(FileName))]
    [JsonIgnore]
    public bool HasFile => FileName != null;

    [JsonIgnore]
    public FileSystemPath FilePath
    {
        get
        {
            if (FileName == null)
                throw new InvalidOperationException("Can't get a file path when there is no file name");

            return Directory + FileName;
        }
    }

    public static InstallLocation FromFilePath(FileSystemPath filePath) => new(filePath.Parent, filePath.Name);

    public string GetRequiredFileName()
    {
        if (FileName == null)
            throw new InvalidOperationException("The location has no file name");

        return FileName;
    }

    public override string ToString() => HasFile ? Directory + FileName : Directory;
}