using System;
using System.Text.RegularExpressions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Game data for an educational DOSBox game
/// </summary>
public class UserData_EducationalDosBoxGameData
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="installDir">The game install directory</param>
    /// <param name="launchName">The launch name</param>
    /// <param name="id">The game ID</param>
    public UserData_EducationalDosBoxGameData(FileSystemPath installDir, string launchName, string? id = null)
    {
        InstallDir = installDir;
        LaunchName = launchName;
        ID = id ?? 
             // Create a new ID if we don't have one
             Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
        Name = String.Empty;
        MountPath = FileSystemPath.EmptyPath;
    }

    /// <summary>
    /// The game install directory
    /// </summary>
    public FileSystemPath InstallDir { get; }

    /// <summary>
    /// The launch name
    /// </summary>
    public string LaunchName { get; }

    /// <summary>
    /// The game ID
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// The game name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The launch mode argument
    /// </summary>
    public string? LaunchMode { get; set; }

    /// <summary>
    /// The mount path
    /// </summary>
    public FileSystemPath MountPath { get; set; }
}