#nullable disable
using RayCarrot.Rayman;
using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an enum with a <see cref="GameModeBaseAttribute"/> to a readable <see cref="String"/>
/// </summary>
public class GameModeToDisplayNameConverter : BaseValueConverter<GameModeToDisplayNameConverter, Enum, string>
{
    public override string ConvertValue(Enum value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.GetAttribute<GameModeBaseAttribute>()?.DisplayName ?? value.ToString();
    }
}