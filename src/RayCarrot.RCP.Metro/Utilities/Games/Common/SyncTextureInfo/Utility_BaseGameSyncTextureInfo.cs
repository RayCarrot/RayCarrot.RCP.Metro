namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base game sync texture info utility
/// </summary>
public abstract class Utility_BaseGameSyncTextureInfo : Utility<Utility_BaseGameSyncTextureInfo_UI, Utility_BaseGameSyncTextureInfo_ViewModel>
{
    protected Utility_BaseGameSyncTextureInfo(Utility_BaseGameSyncTextureInfo_ViewModel viewModel) : base(viewModel) { }

    public override string DisplayHeader => Resources.Utilities_SyncTextureInfo_Header;
    public override GenericIconKind Icon => GenericIconKind.Utilities_SyncTextureInfo;
    public override string InfoText => Resources.Utilities_SyncTextureInfo_Info;
}