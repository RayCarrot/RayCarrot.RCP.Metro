using MahApps.Metro.Controls;
using System;
using System.Globalization;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Key"/> to a <see cref="String"/>
/// </summary>
public class KeyToStringConverter : BaseValueConverter<KeyToStringConverter, Key, string>
{
    public override string ConvertValue(Key value, Type targetType, object parameter, CultureInfo culture)
    {
        return new HotKey(value).ToString();
    }
}