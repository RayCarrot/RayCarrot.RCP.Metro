﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro.Games.Data;

/// <summary>
/// Data for a game which was installed through the Rayman Control Panel
/// </summary>
public class RCPGameInstallData
{
    public RCPGameInstallData(FileSystemPath installDir, RCPInstallMode installMode, DateTime installDate)
    {
        InstallDir = installDir;
        InstallMode = installMode;
        InstallDate = installDate;
    }

    /// <summary>
    /// The directory the game was installed to. In most cases this matches the install location.
    /// But if the game is a single-file game the install location might point to the file instead.
    /// </summary>
    public FileSystemPath InstallDir { get; }

    /// <summary>
    /// The was in which the game was installed
    /// </summary>
    public RCPInstallMode InstallMode { get; }

    /// <summary>
    /// The time and date the game was installed
    /// </summary>
    public DateTime InstallDate { get; }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RCPInstallMode
    {
        DiscInstall,
        Download,
    }
}