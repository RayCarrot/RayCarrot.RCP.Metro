using MahApps.Metro.Controls;
using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="Boolean"/> to a <see cref="TransitionType"/>
    /// </summary>
    public class BooleanToTransitionTypeConverter : BaseValueConverter<BooleanToTransitionTypeConverter, bool, TransitionType>
    {
        public override TransitionType ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? TransitionType.Left : TransitionType.Normal;
        }
    }
}