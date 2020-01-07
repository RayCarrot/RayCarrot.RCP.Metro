using RayCarrot.WPF;
using System;
using System.Globalization;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Converts an <see cref="OpenSpaceGameMode"/> to a readable <see cref="String"/>
    /// </summary>
    public class OpenSpaceGameModeToDisplayNameConverter : BaseValueConverter<OpenSpaceGameModeToDisplayNameConverter, OpenSpaceGameMode, string>
    {
        public override string ConvertValue(OpenSpaceGameMode value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.GetDisplayName();
        }
    }
}