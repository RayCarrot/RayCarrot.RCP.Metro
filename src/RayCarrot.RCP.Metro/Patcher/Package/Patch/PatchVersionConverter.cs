using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchVersionConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else if (value is PatchVersion)
            writer.WriteValue(value.ToString());
        else
            throw new JsonSerializationException("Expected PatchVersion object value");
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        else if (reader.TokenType == JsonToken.String)
        {
            try
            {
                return PatchVersion.Parse((string)reader.Value!);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error parsing patch version string {reader.Value}", ex);
            }
        }
        else
        {
            throw new JsonSerializationException($"Unexpected token or value when parsing patch version. Token: {reader.TokenType}, Value: {reader.Value}");
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PatchVersion);
    }
}