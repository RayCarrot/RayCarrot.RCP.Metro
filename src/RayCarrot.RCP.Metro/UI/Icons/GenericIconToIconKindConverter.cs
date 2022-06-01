using System;
using System.Globalization;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

public class GenericIconToIconKindConverter : BaseValueConverter<GenericIconToIconKindConverter, GenericIconKind, PackIconMaterialKind>
{
    public override PackIconMaterialKind ConvertValue(GenericIconKind value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((GenericIcon)App.Current.FindResource($"RCP.GenericIcons.{value}")).IconKind;
    }
}