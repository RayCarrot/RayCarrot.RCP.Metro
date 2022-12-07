using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Data for a game which was installed through the Rayman Control Panel
/// </summary>
public class UserData_RCPGameInstallData
{
    public UserData_RCPGameInstallData(FileSystemPath installDir, RCPInstallMode installMode)
    {
        InstallDir = installDir;
        InstallMode = installMode;
    }

    // TODO-14: Add more properties like Registry key (if we add that eventually), install date, size etc.? Show this as debug info somewhere

    public FileSystemPath InstallDir { get; }
    public RCPInstallMode InstallMode { get; }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RCPInstallMode
    {
        DiscInstall,
        Download,
    }
}