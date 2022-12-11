#nullable disable
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="String"/> to a <see cref="FileSystemPath"/>
/// </summary>
public class StringToFileSystemPathConverter : BaseValueConverter<StringToFileSystemPathConverter, string, FileSystemPath>
{
    public override FileSystemPath ConvertValue(string value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public override string ConvertValueBack(FileSystemPath value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}