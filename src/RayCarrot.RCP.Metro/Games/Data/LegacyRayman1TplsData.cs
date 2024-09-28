namespace RayCarrot.RCP.Metro.Games.Data;

/// <summary>
/// Legacy TPLS data for the Rayman 1 utility
/// </summary>
public class LegacyRayman1TplsData
{
    public LegacyRayman1TplsData(FileSystemPath installDir)
    {
        InstallDir = installDir;
    }

    /// <summary>
    /// The directory it is installed under
    /// </summary>
    public FileSystemPath InstallDir { get; }

    /// <summary>
    /// The id for the game client installation added for the TPLS DOSBox version
    /// </summary>
    public string? GameClientInstallationId { get; set; }
}