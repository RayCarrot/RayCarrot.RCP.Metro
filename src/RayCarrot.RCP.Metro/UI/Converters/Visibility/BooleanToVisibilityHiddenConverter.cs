using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Boolean"/> to a <see cref="Visibility"/> with the false state being <see cref="Visibility.Hidden"/>
/// </summary>
public class BooleanToVisibilityHiddenConverter : BaseValueConverter<BooleanToVisibilityHiddenConverter, bool, Visibility>
{
    public override Visibility ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture) => 
        value ? Visibility.Visible : Visibility.Hidden;

    public override bool ConvertValueBack(Visibility value, Type targetType, object parameter, CultureInfo culture) =>
        value == Visibility.Visible;
}