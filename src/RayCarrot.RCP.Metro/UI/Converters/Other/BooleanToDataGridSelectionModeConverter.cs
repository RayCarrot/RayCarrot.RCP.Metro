#nullable disable
using System;
using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Boolean"/> to a <see cref="DataGridSelectionMode"/>
/// </summary>
public class BooleanToDataGridSelectionModeConverter : BaseValueConverter<BooleanToDataGridSelectionModeConverter, bool, DataGridSelectionMode>
{
    public override DataGridSelectionMode ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture)
    {
        return value ? DataGridSelectionMode.Extended : DataGridSelectionMode.Single;
    }

    public override bool ConvertValueBack(DataGridSelectionMode value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == DataGridSelectionMode.Extended;
    }
}