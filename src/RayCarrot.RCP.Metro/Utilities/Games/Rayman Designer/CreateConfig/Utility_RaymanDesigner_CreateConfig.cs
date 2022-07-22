namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Designer create config utility
/// </summary>
public class Utility_RaymanDesigner_CreateConfig : Utility<Utility_RaymanDesigner_CreateConfig_Control, Utility_RaymanDesigner_CreateConfig_ViewModel>
{
    public override string DisplayHeader => Resources.RDU_CreateConfigHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanDesigner_CreateConfig;
    public override string InfoText => Resources.RDU_CreateConfigInfo;
    public override bool RequiresAdmin => ViewModel.ConfigPath.FileExists && !Services.File.CheckFileWriteAccess(ViewModel.ConfigPath);
    public override bool IsAvailable => Games.RaymanDesigner.GetInstallDir(false).DirectoryExists;
}