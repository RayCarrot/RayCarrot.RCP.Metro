﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro;

public class BooleanOrToVisibilityMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.Any(x => x is true) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}