#nullable disable
using System;
using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Visual"/> to ´the first descendant control by the <see cref="Type"/> specified
/// </summary>
public class GetDescendantByTypeConverter : BaseValueConverter<GetDescendantByTypeConverter, Visual, object, Type>
{
    public override object ConvertValue(Visual value, Type targetType, Type parameter, CultureInfo culture)
    {
        return value.GetDescendantByType(parameter);
    }
}