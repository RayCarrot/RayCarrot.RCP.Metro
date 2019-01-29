using Microsoft.Extensions.Logging;
using RayCarrot.WPF;
using System;
using System.Globalization;
using System.Windows.Media;
// ReSharper disable PossibleNullReferenceException

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="LogLevel"/> to a <see cref="Brush"/>
    /// </summary>
    public class LogTypeToBrushConverter : BaseValueConverter<LogTypeToBrushConverter, LogLevel, object>
    {
        public override object ConvertValue(LogLevel value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case LogLevel.Trace:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#689F38"));

                case LogLevel.Debug:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AFB42B"));

                case LogLevel.Information:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));

                case LogLevel.Warning:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA000"));

                case LogLevel.Error:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E64A19"));

                case LogLevel.Critical:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d32f2f"));

                default:
                    return null;
            }
        }
    }
}