namespace RayCarrot.RCP.Metro;

/// <summary>
/// The converters utility
/// </summary>
public class Utility_Converters : Utility<Utility_Converters_Control, Utility_Converters_ViewModel>
{
    public Utility_Converters()
    {
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsLoading))
                OnIsLoadingChanged();
        };
    }

    public override string DisplayHeader => Resources.Utilities_Converter_Header;
    public override GenericIconKind Icon => GenericIconKind.Utilities_Converters;

    public override bool IsLoading => ViewModel.IsLoading;
}