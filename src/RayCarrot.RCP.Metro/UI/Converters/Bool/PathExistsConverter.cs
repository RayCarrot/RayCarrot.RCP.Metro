using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="String"/> to a <see cref="Boolean"/> which is true if the value is an existing file system path
/// </summary>
public class PathExistsConverter : BaseValueConverter<PathExistsConverter, string, bool>
{
    public override bool ConvertValue(string value, Type targetType, object parameter, CultureInfo culture)
    {
        return new FileSystemPath(value).Exists;
    }
}