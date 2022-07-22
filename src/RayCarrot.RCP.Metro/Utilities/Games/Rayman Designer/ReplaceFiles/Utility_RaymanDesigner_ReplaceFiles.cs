namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Designer replace files utility
/// </summary>
public class Utility_RaymanDesigner_ReplaceFiles : Utility<Utility_RaymanDesigner_ReplaceFiles_Control, Utility_RaymanDesigner_ReplaceFiles_ViewModel>
{
    public override string DisplayHeader => Resources.RDU_ReplaceFilesHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanDesigner_ReplaceFiles;
    public override string InfoText => Resources.RDU_ReplaceFilesInfo;
    public override bool RequiresAdditionalFiles => true;
    public override bool RequiresAdmin => !Services.File.CheckDirectoryWriteAccess(Games.RaymanDesigner.GetInstallDir());
    public override bool IsAvailable => Games.RaymanDesigner.GetInstallDir(false).DirectoryExists;
}