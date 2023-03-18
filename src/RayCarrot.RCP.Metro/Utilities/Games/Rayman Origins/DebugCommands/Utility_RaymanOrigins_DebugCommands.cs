namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins debug commands utility
/// </summary>
public class Utility_RaymanOrigins_DebugCommands : Utility<Utility_RaymanOrigins_DebugCommands_Control, Utility_RaymanOrigins_DebugCommands_ViewModel>
{
    public Utility_RaymanOrigins_DebugCommands(GameInstallation gameInstallation) 
        : base(new Utility_RaymanOrigins_DebugCommands_ViewModel(gameInstallation))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override string DisplayHeader => Resources.ROU_DebugCommandsHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanOrigins_DebugCommands;
    public override string InfoText => Resources.ROU_DebugCommandsInfo;
    public override string WarningText => Resources.ROU_DebugCommandsWarning;
    public override bool RequiresAdmin => ViewModel.DebugCommandFilePath.FileExists && !Services.File.CheckFileWriteAccess(ViewModel.DebugCommandFilePath);
    public override bool IsAvailable => GameInstallation.InstallLocation.Directory.DirectoryExists;
}