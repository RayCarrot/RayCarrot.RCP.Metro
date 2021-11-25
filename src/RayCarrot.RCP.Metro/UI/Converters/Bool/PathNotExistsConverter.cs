#nullable disable
using System;
using System.Globalization;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="String"/> to a <see cref="Boolean"/> which is true if the value is a non-existing file system path
/// </summary>
public class PathNotExistsConverter : BaseValueConverter<PathNotExistsConverter, string, bool>
{
    public override bool ConvertValue(string value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(new FileSystemPath(value).Exists);
    }
}