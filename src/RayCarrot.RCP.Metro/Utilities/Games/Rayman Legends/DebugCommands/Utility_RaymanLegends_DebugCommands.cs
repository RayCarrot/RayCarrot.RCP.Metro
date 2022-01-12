namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends debug commands utility
/// </summary>
public class Utility_RaymanLegends_DebugCommands : Utility<Utility_RaymanLegends_DebugCommands_UI, Utility_RaymanLegends_DebugCommands_ViewModel>
{
    public override string DisplayHeader => Resources.ROU_DebugCommandsHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanLegends_DebugCommands;
    public override string InfoText => Resources.ROU_DebugCommandsInfo;
    public override string WarningText => Resources.ROU_DebugCommandsWarning;
    public override bool IsAvailable => !(ViewModel.GameFilePath.Parent + "steam_api.dll").FileExists;
}