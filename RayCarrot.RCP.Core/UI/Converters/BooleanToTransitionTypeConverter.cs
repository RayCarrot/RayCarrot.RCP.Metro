using MahApps.Metro.Controls;
using RayCarrot.WPF;
using System;
using System.Globalization;

namespace RayCarrot.RCP.Core
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

        public override bool ConvertValueBack(TransitionType value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}