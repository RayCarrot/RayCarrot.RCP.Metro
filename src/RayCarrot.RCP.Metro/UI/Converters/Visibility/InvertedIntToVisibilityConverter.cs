using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Int32"/> to a <see cref="Visibility"/>
/// </summary>
public class InvertedIntToVisibilityConverter : BaseValueConverter<InvertedIntToVisibilityConverter, int, Visibility>
{
    public override Visibility ConvertValue(int value, Type targetType, object parameter, CultureInfo culture) => 
        value == 0 ? Visibility.Visible : Visibility.Collapsed;

    public override int ConvertValueBack(Visibility value, Type targetType, object parameter, CultureInfo culture) =>
        value != Visibility.Visible ? 1 : 0;
}