using System;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Games.Emulators;

public class StringEmulatorDescriptorConverter : JsonConverter<EmulatorDescriptor>
{
    public GamesManager GamesManager => Services.Games; // TODO: Pass in through ctor instead

    public override void WriteJson(JsonWriter writer, EmulatorDescriptor? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else
            writer.WriteValue(value.EmulatorId);
    }

    public override EmulatorDescriptor ReadJson(JsonReader reader, Type objectType, EmulatorDescriptor? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is not string id)
            throw new Exception("The emulator descriptor ID can not be null");

        return GamesManager.GetEmulatorDescriptor(id);
    }
}