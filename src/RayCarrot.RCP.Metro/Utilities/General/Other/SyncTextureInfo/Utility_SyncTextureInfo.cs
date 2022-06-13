namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility
/// </summary>
public class Utility_SyncTextureInfo : Utility<Utility_SyncTextureInfo_UI, Utility_SyncTextureInfo_ViewModel>
{
    public Utility_SyncTextureInfo()
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