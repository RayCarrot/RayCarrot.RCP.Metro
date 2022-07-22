namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins update utility
/// </summary>
public class Utility_RaymanOrigins_Update : Utility<Utility_RaymanOrigins_Update_Control, Utility_RaymanOrigins_Update_ViewModel>
{
    public override string DisplayHeader => Resources.ROU_UpdateHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanOrigins_Update;
    public override string InfoText => Resources.ROU_UpdateInfo;
    public override bool RequiresAdditionalFiles => true;
}