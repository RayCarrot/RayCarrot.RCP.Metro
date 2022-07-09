using System;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchFilePathJsonConverter : JsonConverter<PatchFilePath>
{
    private const char LocationSeparator = ':';

    public override void WriteJson(JsonWriter writer, PatchFilePath value, JsonSerializer serializer)
    {
        string str = value.HasLocation ? $"{value.Location}{LocationSeparator}{value.FilePath}" : value.FilePath;
        writer.WriteValue(str);
    }

    public override PatchFilePath ReadJson(JsonReader reader, Type objectType, PatchFilePath existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is not string str)
            return default;

        int separatorIndex = str.IndexOf(LocationSeparator);

        if (separatorIndex == -1)
            return new PatchFilePath(String.Empty, str);
        else
            return new PatchFilePath(str.Substring(0, separatorIndex), str.Substring(separatorIndex + 1));
    }
}