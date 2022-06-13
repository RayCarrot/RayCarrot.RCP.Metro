namespace RayCarrot.RCP.Metro;

/// <summary>
/// The decoders utility
/// </summary>
public class Utility_Decoders : Utility<Utility_Decoders_UI, Utility_Decoders_ViewModel>
{
    public Utility_Decoders()
    {
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsLoading))
                OnIsLoadingChanged();
        };
    }

    public override string DisplayHeader => Resources.Utilities_Decoder_Header;
    public override GenericIconKind Icon => GenericIconKind.Utilities_Decoders;

    public override bool IsLoading => ViewModel.IsLoading;
}