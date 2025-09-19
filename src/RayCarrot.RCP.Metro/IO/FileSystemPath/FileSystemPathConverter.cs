#nullable disable
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="FileSystemPath"/> to other values
/// </summary>
public class FileSystemPathConverter : TypeConverter
{
    /// <summary>
    /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="sourceType">A <see cref="Type"/> that represents the type you want to convert from.</param>
    /// <returns>True if this converter can perform the conversion; otherwise, false.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string) || sourceType == typeof(FileSystemInfo) || sourceType == typeof(FileSystemPath))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }

    /// <summary>
    /// Returns whether this converter can convert the object to the specified type, using the specified context.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="destinationType">A <see cref="Type"/> that represents the type you want to convert to.</param>
    /// <returns>True if this converter can perform the conversion; otherwise, false.</returns>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        if (destinationType == typeof(string) || destinationType == typeof(FileSystemInfo) || destinationType == typeof(FileSystemPath))
            return true;
        else
            return base.CanConvertTo(context, destinationType);
    }

    /// <summary>
    /// Converts the given object to the type of this converter, using the specified context and culture information.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
    /// <param name="value">The <see cref="Object"/> to convert.</param>
    /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
    /// <exception cref="NotSupportedException">The conversion cannot be performed</exception>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string stringValue)
            return new FileSystemPath(stringValue);
        else if (value is FileSystemInfo fileSystemInfoValue)
            return new FileSystemPath(fileSystemInfoValue.FullName);
        else if (value is FileSystemPath pathValue)
            return pathValue;
        else if (value == null)
            return FileSystemPath.EmptyPath;
        else
            return base.ConvertFrom(context, culture, value);
    }

    /// <summary>
    /// Converts the given value object to the specified type, using the specified context and culture information.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="culture">A <see cref="CultureInfo"/>. If null is passed, the current culture is assumed.</param>
    /// <param name="value">The <see cref="Object"/> to convert.</param>
    /// <param name="destinationType">The <see cref="Type"/> to convert the value parameter to.</param>
    /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
    /// <exception cref="ArgumentNullException">The destinationType parameter is null.</exception>
    /// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (!(value is FileSystemPath path))
            return base.ConvertTo(context, culture, value, destinationType);

        if (destinationType == typeof(string))
            return path.FullPath;

        if (destinationType == typeof(FileSystemInfo))
            return path.GetFileSystemInfo();

        if (destinationType == typeof(FileSystemPath))
            return value;

        return base.ConvertTo(context, culture, value, destinationType);
    }
}