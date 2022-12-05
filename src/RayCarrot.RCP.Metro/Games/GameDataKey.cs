namespace RayCarrot.RCP.Metro;

public static class GameDataKey
{
    // Win32
    public const string Win32_RunAsAdmin = "Win32_RunAsAdmin";
    
    // TODO-14: Rename and organize. Maybe move to relevant GameDescriptor files instead?
    public const string DOSBoxMountPath = "DOSBoxMountPath";
    public const string Ray1MSDOSData = "Ray1MSDOSData"; // TODO-14: Use this for KIT, FAN and 60N as well
    public const string RRR2LaunchMode = "RRR2LaunchMode";
    public const string RCPGameInstallInfo = "RCPGameInstallInfo";
    public const string EmulatorInstallationId = "EmulatorInstallationId";
}