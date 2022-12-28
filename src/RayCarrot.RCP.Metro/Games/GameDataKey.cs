namespace RayCarrot.RCP.Metro;

public static class GameDataKey
{
    // TODO-14: Maybe move to relevant GameDescriptor files instead? Same with GameClientDataKey.

    // General
    public const string RCP_GameInstallData = "RCP_GameInstallData";
    public const string RCP_AddedFiles = "RCP_AddedFiles";
    public const string RCP_CustomName = "RCP_CustomName";

    // Win32
    public const string Win32_RunAsAdmin = "Win32_RunAsAdmin";
    
    // Game clients
    public const string Client_SelectedClient = "Client_SelectedClient";
    public const string Client_DosBox_MountPath = "Client_DosBox_MountPath";
    
    // Games
    public const string Ray1_MsDosData = "Ray1_MsDosData"; // TODO-14: Use this for KIT, FAN and 60N as well
    public const string RRR2_LaunchMode = "RRR2_LaunchMode";
    public const string RRRAC_ShownLaunchMessage = "RRRAC_ShownLaunchMessage";
    public const string RGH_LaunchData = "RGH_LaunchData";
}