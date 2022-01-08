#nullable disable
using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="FileSystemPath"/> to a <see cref="String"/>
/// </summary>
public class FileSystemPathToStringConverter : BaseValueConverter<FileSystemPathToStringConverter, FileSystemPath, string>
{
    public override string ConvertValue(FileSystemPath value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public override FileSystemPath ConvertValueBack(string value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}