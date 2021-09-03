using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="Boolean"/> to a <see cref="Duration"/> to be used in a FluidMoveBehavior
    /// </summary>
    public class BooleanToFluidMoveDurationConverter : BaseValueConverter<BooleanToFluidMoveDurationConverter, bool, Duration>
    {
        public override Duration ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? new Duration(new TimeSpan(0, 0, 0, 0, 300)) : new Duration(TimeSpan.Zero);
        }

        public override bool ConvertValueBack(Duration value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}