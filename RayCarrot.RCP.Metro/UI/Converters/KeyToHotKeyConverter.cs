using MahApps.Metro.Controls;
using RayCarrot.WPF;
using System;
using System.Globalization;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="Key"/> to a <see cref="HotKey"/>
    /// </summary>
    public class KeyToHotKeyConverter : BaseValueConverter<KeyToHotKeyConverter, Key, HotKey>
    {
        public override HotKey ConvertValue(Key value, Type targetType, object parameter, CultureInfo culture)
        {
            return new HotKey(value);
        }

        public override Key ConvertValueBack(HotKey value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Key;
        }
    }
}