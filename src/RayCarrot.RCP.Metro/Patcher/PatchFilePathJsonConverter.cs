using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchFilePathJsonConverter : JsonConverter<PatchFilePath>
{
    private const char LocationSeparator = ':';

    public override void WriteJson(JsonWriter writer, PatchFilePath value, JsonSerializer serializer)
    {
        string str = value.HasLocation 
            ? $"{value.Location}{LocationSeparator}{value.LocationID}{LocationSeparator}{value.FilePath}" 
            : value.FilePath;
        writer.WriteValue(str);
    }

    public override PatchFilePath ReadJson(JsonReader reader, Type objectType, PatchFilePath existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is not string str)
            return default;

        string[] values = str.Split(LocationSeparator);

        return values.Length switch
        {
            1 => new PatchFilePath(str),
            3 => new PatchFilePath(values[2], values[0], values[1]),
            _ => throw new Exception($"Invalid patch file path format '{str}'")
        };
    }
}