using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts an <see cref="Object"/> to a <see cref="Boolean"/> which is true when the value is not null
    /// </summary>
    public class IsNotNullConverter : BaseValueConverter<IsNotNullConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(value is null);
    }
}