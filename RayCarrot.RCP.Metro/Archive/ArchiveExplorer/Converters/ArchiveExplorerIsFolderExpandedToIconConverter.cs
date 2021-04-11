using System;
using System.Globalization;
using MahApps.Metro.IconPacks;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    public class ArchiveExplorerIsFolderExpandedToIconConverter : BaseValueConverter<ArchiveExplorerIsFolderExpandedToIconConverter, bool, PackIconMaterialKind>
    {
        public override PackIconMaterialKind ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? PackIconMaterialKind.FolderOpenOutline : PackIconMaterialKind.FolderOutline;
        }
    }
}