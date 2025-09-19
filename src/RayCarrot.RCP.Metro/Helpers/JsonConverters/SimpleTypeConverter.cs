using System.Reflection;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Serializes a Type to a simple string
/// </summary>
public class SimpleTypeConverter : JsonConverter<Type>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.</value>
    public override bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.</value>
    public override bool CanWrite => true;

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
    /// <param name="hasExistingValue">The existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override Type ReadJson(JsonReader reader, Type objectType, Type existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        // Read the value as a string
        var value = reader.Value.ToString();

        // Get the type from the entry assembly
        return Assembly.GetEntryAssembly()?.GetType($"RayCarrot.RCP.Metro.{value}");
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, Type value, JsonSerializer serializer)
    {
        writer.WriteValue(value.FullName);
    }
}