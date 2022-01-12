namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Direct Play utility
/// </summary>
public class Utility_Rayman3_DirectPlay : Utility<Utility_Rayman3_DirectPlay_UI, Utility_Rayman3_DirectPlay_ViewModel>
{
    public override string DisplayHeader => Resources.R3U_DirectPlayHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_Rayman3_DirectPlay;
    public override string InfoText => Resources.R3U_DirectPlayInfo;
    public override bool RequiresAdmin => true;
    public override bool IsAvailable => AppViewModel.WindowsVersion is >= WindowsVersion.Win8 or WindowsVersion.Unknown;
}