using System;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

// Note: This is technically not used anymore, but we keep it anyway in case we need it
public class PatchFilePathJsonConverter : JsonConverter<PatchFilePath>
{
    private const char LocationSeparator = ':';

    public override void WriteJson(JsonWriter writer, PatchFilePath? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        string str = value.HasLocation 
            ? $"{value.Location}{LocationSeparator}{value.LocationID}{LocationSeparator}{value.FilePath}" 
            : value.FilePath;
        writer.WriteValue(str);
    }

    public override PatchFilePath? ReadJson(JsonReader reader, Type objectType, PatchFilePath? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is not string str)
            return default;

        string[] values = str.Split(LocationSeparator);

        return values.Length switch
        {
            1 => new PatchFilePath(String.Empty, String.Empty, str),
            3 => new PatchFilePath(values[0], values[1], values[2]),
            _ => throw new Exception($"Invalid patch file path format '{str}'")
        };
    }
}