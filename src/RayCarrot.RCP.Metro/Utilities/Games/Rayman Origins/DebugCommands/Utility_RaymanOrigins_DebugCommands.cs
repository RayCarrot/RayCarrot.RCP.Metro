using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins debug commands utility
/// </summary>
public class Utility_RaymanOrigins_DebugCommands : Utility<Utility_RaymanOrigins_DebugCommands_UI, Utility_RaymanOrigins_DebugCommands_ViewModel>
{
    public override string DisplayHeader => Resources.ROU_DebugCommandsHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanOrigins_DebugCommands;
    public override string InfoText => Resources.ROU_DebugCommandsInfo;
    public override string WarningText => Resources.ROU_DebugCommandsWarning;
    public override bool RequiresAdmin => ViewModel.DebugCommandFilePath.FileExists && !Services.File.CheckFileWriteAccess(ViewModel.DebugCommandFilePath);
    public override bool IsAvailable => Games.RaymanOrigins.GetInstallDir(false).DirectoryExists;
    public override IEnumerable<string> GetAppliedUtilities()
    {
        if (ViewModel.DebugCommandFilePath.FileExists)
            yield return Resources.ROU_DebugCommandsHeader;
    }
}