namespace RayCarrot.RCP.Metro;

/// <summary>
/// The decoders utility
/// </summary>
public class Utility_Serializers : Utility<Utility_Serializers_Control, Utility_Serializers_ViewModel>
{
    public Utility_Serializers()
    {
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsLoading))
                OnIsLoadingChanged();
        };
    }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.Utilities_Serializers_Header));
    public override GenericIconKind Icon => GenericIconKind.Utilities_Serializers;

    public override bool IsLoading => ViewModel.IsLoading;
}