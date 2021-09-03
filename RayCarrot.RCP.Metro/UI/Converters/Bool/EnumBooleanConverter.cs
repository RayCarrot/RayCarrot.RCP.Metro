using System;
using System.Globalization;
using RayCarrot.Common;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts an <see cref="Enum"/> to a <see cref="Boolean"/> which is true if the value equals the parameter value.
    /// This is useful for a <see cref="System.Windows.Controls.RadioButton"/> bound to an <see cref="Enum"/>
    /// </summary>
    public class EnumBooleanConverter : BaseValueConverter<EnumBooleanConverter, Enum, bool, string>
    {
        public override bool ConvertValue(Enum value, Type targetType, string parameter, CultureInfo culture)
        {
            object parameterValue = Enum.Parse(value.GetType(), parameter);

            return parameterValue.Equals(value);
        }

        public override Enum ConvertValueBack(bool value, Type targetType, string parameter, CultureInfo culture)
        {
            return Enum.Parse(targetType, parameter).CastTo<Enum>();
        }
    }
}