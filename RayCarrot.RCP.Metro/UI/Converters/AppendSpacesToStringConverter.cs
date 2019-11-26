using RayCarrot.WPF;
using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="String"/> to a <see cref="String"/> with spaces appended
    /// </summary>
    public class AppendSpacesToStringConverter : BaseValueConverter<AppendSpacesToStringConverter, string, string>
    {
        public override string ConvertValue(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return value + "  ";
        }
    }
}