namespace RayCarrot.RCP.Metro;

/// <summary>
/// The CPA texture sync utility
/// </summary>
public class Utility_CPATextureSync : Utility<Utility_CPATextureSync_Control, Utility_CPATextureSync_ViewModel>
{
    public Utility_CPATextureSync(GameInstallation gameInstallation, CPATextureSyncData data) 
        : base(new Utility_CPATextureSync_ViewModel(gameInstallation, data))
    {
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsLoading))
                OnIsLoadingChanged();
        };
    }

    public override string DisplayHeader => Resources.Utilities_SyncTextureInfo_Header;
    public override GenericIconKind Icon => GenericIconKind.Utilities_SyncTextureInfo;
    public override string InfoText => Resources.Utilities_SyncTextureInfo_Info;

    public override bool IsLoading => ViewModel.IsLoading;
}