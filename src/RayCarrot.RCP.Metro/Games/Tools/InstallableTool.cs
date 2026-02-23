namespace RayCarrot.RCP.Metro.Games.Tools;

public abstract class InstallableTool
{
    public abstract string ToolId { get; }
    public abstract Version LatestVersion { get; }
    
    public FileSystemPath InstallDirectory => AppFilePaths.ToolsBaseDir + ToolId;

    public virtual void OnUninstalled() { }
}