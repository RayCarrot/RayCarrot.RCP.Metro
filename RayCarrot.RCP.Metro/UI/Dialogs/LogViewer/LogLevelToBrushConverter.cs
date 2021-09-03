using Microsoft.Extensions.Logging;
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
                return logLevel switch
                {
                    LogLevel.Trace => "#689F38",
                    LogLevel.Debug => "#AFB42B",
                    LogLevel.Information => "#1976D2",
                    LogLevel.Warning => "#FFA000",
                    LogLevel.Error => "#E64A19",
                    LogLevel.Critical => "#d32f2f",
                    _ => null
                };
            }
        }
    }
}