﻿#nullable disable
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Helper methods for JSON
/// </summary>
public static class JsonHelpers
{
    private static IEnumerable<JsonConverter> GetDefaultConverters()
    {
        yield return new ByteArrayHexConverter();
    }

    /// <summary>
    /// Serializes an object to a file
    /// </summary>
    /// <typeparam name="T">The type of file to serialize</typeparam>
    /// <param name="obj">The object to serialize</param>
    /// <param name="formatting">The formatting to use</param>
    /// <param name="filePath">The file to serialize to</param>
    /// <param name="converters">Optional converters to use</param>
    public static void SerializeToFile<T>(T obj, string filePath, Formatting formatting = Formatting.Indented, params JsonConverter[] converters)
    {
        // Serialize to JSON
        var json = JsonConvert.SerializeObject(obj, formatting, converters.Concat(GetDefaultConverters()).ToArray());

        // Write to output
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Serializes an object to a stream
    /// </summary>
    /// <typeparam name="T">The type of file to serialize</typeparam>
    /// <param name="obj">The object to serialize</param>
    /// <param name="stream">The stream to serialize to</param>
    /// <param name="converters">Optional converters to use</param>
    public static void SerializeToStream<T>(T obj, Stream stream, params JsonConverter[] converters)
    {
        JsonSerializer serializer = new();

        serializer.Formatting = Formatting.Indented;
        
        foreach (JsonConverter c in converters.Concat(GetDefaultConverters()))
            serializer.Converters.Add(c);

        using StreamWriter sr = new(stream);
        using JsonTextWriter jsonTextWriter = new(sr);

        serializer.Serialize(jsonTextWriter, obj);
    }

    /// <summary>
    /// Deserializes an object from a file
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize</typeparam>
    /// <param name="filePath">The file to deserialize</param>
    /// <param name="converters">Optional converters to use</param>
    /// <returns>The deserialized object</returns>
    public static T DeserializeFromFile<T>(string filePath, params JsonConverter[] converters)
    {
        // Read the JSON
        var json = File.ReadAllText(filePath);

        // Return the deserialized object
        return JsonConvert.DeserializeObject<T>(json, converters.Concat(GetDefaultConverters()).ToArray());
    }

    /// <summary>
    /// Deserializes an object from a URL
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize</typeparam>
    /// <param name="url">The URL of the JSON string to deserialize</param>
    /// <param name="converters">Optional converters to use</param>
    /// <returns>The deserialized object</returns>
    public static async Task<T> DeserializeFromURLAsync<T>(string url, params JsonConverter[] converters)
    {
        // Create the web client
        using WebClient wc = new();

        wc.Encoding = Encoding.UTF8;

        // Download the string
        string str = await wc.DownloadStringTaskAsync(url);

        // Deserialize
        return JsonConvert.DeserializeObject<T>(str, converters.Concat(GetDefaultConverters()).ToArray());
    }

    /// <summary>
    /// Deserializes an object from a file
    /// </summary>
    /// <param name="filePath">The file to deserialize</param>
    /// <param name="type">The type of object to deserialize</param>
    /// <param name="converters">Optional converters to use</param>
    /// <returns>The deserialized object</returns>
    public static object DeserializeFromFile(string filePath, Type type, params JsonConverter[] converters)
    {
        // Read the JSON
        var json = File.ReadAllText(filePath);

        // Return the deserialized object
        return JsonConvert.DeserializeObject(json, type, converters.Concat(GetDefaultConverters()).ToArray());
    }

    /// <summary>
    /// Deserializes an object from a stream
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize</typeparam>
    /// <param name="stream">The stream to deserialize</param>
    /// <param name="converters">Optional converters to use</param>
    /// <returns>The deserialized object</returns>
    public static T DeserializeFromStream<T>(Stream stream, params JsonConverter[] converters)
    {
        JsonSerializer serializer = new();

        foreach (JsonConverter c in converters.Concat(GetDefaultConverters()))
            serializer.Converters.Add(c);

        using StreamReader sr = new(stream);
        using JsonTextReader jsonTextReader = new(sr);
        
        return serializer.Deserialize<T>(jsonTextReader);
    }
}