using System.Globalization;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

public class JValueToStringConverter : BaseValueConverter<JValueToStringConverter, JValue, string>
{
    public override string ConvertValue(JValue value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value.Type switch
        {
            // Add quotes
            JTokenType.String => $"\"{value.Value}\"",
            
            // Lower-case
            JTokenType.Boolean => value.Value!.ToString().ToLower(),
            
            // Null
            JTokenType.Null => "null",
            
            // Default
            _ => value.Value?.ToString() ?? "null"
        };
    }
}