using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

public class InvertedObjectNullToVisibilityHiddenConverter : BaseValueConverter<InvertedObjectNullToVisibilityHiddenConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null ? Visibility.Hidden : Visibility.Visible;
}