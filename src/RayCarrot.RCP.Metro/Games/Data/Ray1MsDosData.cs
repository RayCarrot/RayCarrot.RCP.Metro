using BinarySerializer;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Data;

/// <summary>
/// Defines the common Ray1 MS-DOS data for a game.
/// </summary>
public class Ray1MsDosData
{
    public Ray1MsDosData(Version[] availableVersions, string selectedVersion)
    {
        AvailableVersions = availableVersions;
        SelectedVersion = selectedVersion;
    }

    /// <summary>
    /// The available version for the game. This gets set once when the game is added.
    /// </summary>
    public Version[] AvailableVersions { get; }

    /// <summary>
    /// The currently selected version. This determines the default launch behavior for the game.
    /// </summary>
    public string SelectedVersion { get; set; }

    /// <summary>
    /// Creates a new <see cref="Ray1MsDosData"/> instance from a newly added game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to create the data for</param>
    /// <returns>The data instance</returns>
    public static Ray1MsDosData Create(GameInstallation gameInstallation)
    {
        // Read the VERSION file to get the versions supported by this release. Then keep
        // the ones which are actually available.
        using RCPContext context = new(gameInstallation.InstallLocation);
        gameInstallation.GetComponents<BinarySettingsComponent>().AddSettings(context);
        LinearFile commonFile = context.AddFile(new LinearFile(context, "PCMAP/COMMON.DAT"));

        PC_FileArchive commonArchive = FileFactory.Read<PC_FileArchive>(context, commonFile.FilePath);
        PC_VersionFile versionFile = commonArchive.ReadFile<PC_VersionFile>(context, "VERSION");

        List<Version> availableVersions = new();

        for (int i = 0; i < versionFile.VersionsCount; i++)
        {
            FileSystemPath versionDir = gameInstallation.InstallLocation + "PCMAP" + versionFile.VersionCodes[i];

            // Not all releases contain all supported versions
            if (versionDir.DirectoryExists)
                availableVersions.Add(new Version(versionFile.VersionCodes[i], versionFile.VersionModes[i]));
        }

        // Make sure at least one version was found. The game requires at least one
        // in order to work and have one be selected.
        if (availableVersions.Count == 0)
            throw new Exception("No versions were found");

        // Default to English if available, otherwise use the first version
        string selectedVersion = availableVersions.Find(x => x.Id.Equals("USA", StringComparison.OrdinalIgnoreCase))?.Id ?? 
                                 availableVersions.First().Id;

        return new Ray1MsDosData(availableVersions.ToArray(), selectedVersion);
    }

    public record Version(string Id, string DisplayName);
}