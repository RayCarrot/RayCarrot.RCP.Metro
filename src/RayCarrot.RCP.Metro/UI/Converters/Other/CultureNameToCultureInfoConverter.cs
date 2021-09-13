using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="String"/> representing the culture name to a <see cref="CultureInfo"/>
    /// </summary>
    public class CultureNameToCultureInfoConverter : BaseValueConverter<CultureNameToCultureInfoConverter, string, CultureInfo>
    {
        public override CultureInfo ConvertValue(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return new CultureInfo(value);
        }

        public override string ConvertValueBack(CultureInfo value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Name;
        }
    }
}