using System;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

public class StringGameDescriptorConverter : JsonConverter<GameDescriptor>
{
    public GamesManager GamesManager => Services.Games; // TODO: Pass in through ctor instead

    public override void WriteJson(JsonWriter writer, GameDescriptor? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else
            writer.WriteValue(value.GameId);
    }

    public override GameDescriptor ReadJson(JsonReader reader, Type objectType, GameDescriptor? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is not string id)
            throw new Exception("The game descriptor ID can not be null");

        return GamesManager.GetGameDescriptor(id);
    }
}