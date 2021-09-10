using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="Boolean"/> to a <see cref="Visibility"/>
    /// </summary>
    public class BooleanToVisibilityConverter : BaseValueConverter<BooleanToVisibilityConverter, bool, Visibility>
    {
        public override Visibility ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture) => 
            value ? Visibility.Visible : Visibility.Collapsed;

        public override bool ConvertValueBack(Visibility value, Type targetType, object parameter, CultureInfo culture) =>
            value == Visibility.Visible;
    }
}