namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility
/// </summary>
public class Utility_SyncTextureInfo : Utility<Utility_SyncTextureInfo_UI, Utility_SyncTextureInfo_ViewModel>
{
    public override string DisplayHeader => Resources.Utilities_SyncTextureInfo_Header;
    public override GenericIconKind Icon => GenericIconKind.Utilities_SyncTextureInfo;
    public override string InfoText => Resources.Utilities_SyncTextureInfo_Info;
}