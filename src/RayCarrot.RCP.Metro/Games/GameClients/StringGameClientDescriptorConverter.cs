using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Games.Clients;

public class StringGameClientDescriptorConverter : JsonConverter<GameClientDescriptor>
{
    public GameClientsManager GameClientsManager => Services.GameClients; // TODO: Pass in through ctor instead

    public override void WriteJson(JsonWriter writer, GameClientDescriptor? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else
            writer.WriteValue(value.GameClientId);
    }

    public override GameClientDescriptor ReadJson(JsonReader reader, Type objectType, GameClientDescriptor? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is not string id)
            throw new Exception("The game client descriptor ID can not be null");

        return GameClientsManager.GetGameClientDescriptor(id);
    }
}