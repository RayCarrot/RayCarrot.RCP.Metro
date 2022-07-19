using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro.Patcher;

public class AvailableFileLocationsWrapper : DependencyObject
{
    public IEnumerable<PatchCreatorViewModel.AvailableFileLocation>? Value
    {
        get => (IEnumerable<PatchCreatorViewModel.AvailableFileLocation>?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(IEnumerable<PatchCreatorViewModel.AvailableFileLocation>),
        typeof(AvailableFileLocationsWrapper),
        new PropertyMetadata());
}