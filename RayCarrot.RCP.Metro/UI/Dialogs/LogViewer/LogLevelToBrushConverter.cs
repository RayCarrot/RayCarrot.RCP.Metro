using NLog;
using System;
using System.Globalization;
using System.Windows.Media;

// ReSharper disable PossibleNullReferenceException

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts a <see cref="LogLevel"/> to a <see cref="Brush"/>
    /// </summary>
    internal class LogLevelToBrushConverter : BaseValueConverter<LogLevelToBrushConverter, LogLevel, object>
    {
        public override object ConvertValue(LogLevel value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetDefaultColor(value)));

            static string GetDefaultColor(LogLevel logLevel)
            {
                if (logLevel == LogLevel.Trace)
                    return "#689F38";
                else if (logLevel == LogLevel.Debug)
                    return "#AFB42B";
                else if (logLevel == LogLevel.Info)
                    return "#1976D2";
                else if (logLevel == LogLevel.Warn)
                    return "#FFA000";
                else if (logLevel == LogLevel.Error)
                    return "#E64A19";
                else if (logLevel == LogLevel.Fatal)
                    return "#d32f2f";
                else
                    return null;
            }
        }
    }
}