namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends debug commands utility
/// </summary>
public class Utility_RaymanLegends_DebugCommands : Utility<Utility_RaymanLegends_DebugCommands_Control, Utility_RaymanLegends_DebugCommands_ViewModel>
{
    public Utility_RaymanLegends_DebugCommands(GameInstallation gameInstallation) 
        : base(new Utility_RaymanLegends_DebugCommands_ViewModel(gameInstallation))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.ROU_DebugCommandsHeader));
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanLegends_DebugCommands;
    public override LocalizedString InfoText => new ResourceLocString(nameof(Resources.ROU_DebugCommandsInfo));
    public override LocalizedString WarningText => new ResourceLocString(nameof(Resources.ROU_DebugCommandsWarning));

    public override bool IsAvailable => !(ViewModel.GameFilePath.Parent + "steam_api.dll").FileExists;
    public override LocalizedString NotAvailableInfo => new ResourceLocString(nameof(Resources.ROU_DebugCommandsNotAvailable));
}