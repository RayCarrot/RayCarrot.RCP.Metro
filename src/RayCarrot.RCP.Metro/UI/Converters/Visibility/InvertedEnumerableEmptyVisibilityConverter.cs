using System;
using System.Collections;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="IEnumerable"/> to a <see cref="Visibility"/> which is set to <see cref="Visibility.Visible"/> when the collection is empty
    /// </summary>
    public class InvertedEnumerableEmptyVisibilityConverter : BaseValueConverter<InvertedEnumerableEmptyVisibilityConverter, IEnumerable, Visibility>
    {
        public override Visibility ConvertValue(IEnumerable value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Any() ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}