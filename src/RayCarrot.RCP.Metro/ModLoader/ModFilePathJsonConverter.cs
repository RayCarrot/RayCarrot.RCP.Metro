using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader;

public class ModFilePathJsonConverter : JsonConverter<ModFilePath>
{
    private const char LocationSeparator = ':';

    public override void WriteJson(JsonWriter writer, ModFilePath value, JsonSerializer serializer)
    {
        string str = value.HasLocation 
            ? $"{value.Location}{LocationSeparator}{value.LocationID}{LocationSeparator}{value.FilePath}" 
            : value.FilePath;
        writer.WriteValue(str);
    }

    public override ModFilePath ReadJson(JsonReader reader, Type objectType, ModFilePath existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is not string str)
            return default;

        string[] values = str.Split(LocationSeparator);

        return values.Length switch
        {
            1 => new ModFilePath(str),
            3 => new ModFilePath(values[2], values[0], values[1]),
            _ => throw new Exception($"Invalid mod file path format '{str}'")
        };
    }
}