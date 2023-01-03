using System.Text;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Games.Data;

/// <summary>
/// The launch data for Rabbids Go Home
/// </summary>
public class RabbidsGoHomeLaunchData
{
    /// <summary>
    /// Constructor for default values
    /// </summary>
    public RabbidsGoHomeLaunchData()
    {
        BigFile = "RGH_defrag.bf";
        Language = "en";
        ResolutionX = 800;
        ResolutionY = 600;
        IsVSyncEnabled = true;
        IsFullscreen = true;
        VersionIndex = 5;
        OptionalCommands = Array.Empty<string>();
    }

    /// <summary>
    /// Constructor for specified values
    /// </summary>
    [JsonConstructor]
    public RabbidsGoHomeLaunchData(
        string bigFile, 
        string language, 
        int resolutionX, 
        int resolutionY, 
        bool isVSyncEnabled, 
        bool isFullscreen, 
        int versionIndex, 
        string[] optionalCommands)
    {
        BigFile = bigFile;
        Language = language;
        ResolutionX = resolutionX;
        ResolutionY = resolutionY;
        IsVSyncEnabled = isVSyncEnabled;
        IsFullscreen = isFullscreen;
        VersionIndex = versionIndex;
        OptionalCommands = optionalCommands;
    }

    /// <summary>
    /// The big file name or path
    /// </summary>
    public string BigFile { get; }

    /// <summary>
    /// The language
    /// </summary>
    public string Language { get; }

    /// <summary>
    /// The horizontal resolution
    /// </summary>
    public int ResolutionX { get; }

    /// <summary>
    /// The vertical resolution
    /// </summary>
    public int ResolutionY { get; }

    /// <summary>
    /// Indicates if V-Sync is enabled
    /// </summary>
    public bool IsVSyncEnabled { get; }

    /// <summary>
    /// Indicates if the game should run in fullscreen
    /// </summary>
    public bool IsFullscreen { get; }

    /// <summary>
    /// The version index
    /// </summary>
    public int VersionIndex { get; }

    /// <summary>
    /// Optional commands
    /// </summary>
    public string[] OptionalCommands { get; }

    /// <summary>
    /// Gets the string representation of the launch data to use as a launch argument
    /// </summary>
    /// <returns>The string representation to use as a launch argument</returns>
    public override string ToString()
    {
        // Create the string builder
        StringBuilder sb = new();

        // Add the big file path
        sb.Append($"\"{BigFile}\"");

        // Add engine config
        sb.Append(" /binload/fe");

        // Add language
        sb.Append($" /lang/{Language}");

        // Add V-Sync
        if (IsVSyncEnabled)
            sb.Append(" /vsync");

        // Add fullscreen
        if (IsFullscreen)
            sb.Append(" /fullscreen");

        // Add resolution
        sb.Append($" /res{ResolutionX}x{ResolutionY}");

        // Add version index
        sb.Append($" /versionIndex:{VersionIndex}");

        // Add optional commands
        foreach (string command in OptionalCommands)
            sb.Append($" {command}");

        // Return the string
        return sb.ToString();
    }
}