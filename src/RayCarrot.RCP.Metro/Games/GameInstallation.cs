using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

// TODO-14: Add properties like install size, add date etc.

[JsonObject(MemberSerialization.OptIn)]
public class GameInstallation
{
    #region Constructors

    public GameInstallation(GameDescriptor gameDescriptor, FileSystemPath installLocation) 
        : this(gameDescriptor, installLocation, new Dictionary<string, object>()) 
    { }

    [JsonConstructor]
    private GameInstallation(GameDescriptor gameDescriptor, FileSystemPath installLocation, Dictionary<string, object>? additionalData)
    {
        GameDescriptor = gameDescriptor;
        InstallLocation = installLocation;
        _additionalData = additionalData ?? new Dictionary<string, object>();
    }

    #endregion

    #region Private Fields

    [JsonProperty(PropertyName = "AdditionalData")]
    private readonly Dictionary<string, object> _additionalData;
    private readonly Dictionary<string, object> _dataCache = new();

    #endregion

    #region Public Properties

    [JsonProperty(PropertyName = "Id")]
    [JsonConverter(typeof(StringGameDescriptorConverter))]
    public GameDescriptor GameDescriptor { get; }

    [JsonProperty(PropertyName = "InstallLocation")]
    public FileSystemPath InstallLocation { get; } // TODO-14: Rename to GamePath?

    public string Id => GameDescriptor.Id;

    public Games? LegacyGame => GameDescriptor.LegacyGame; // TODO-14: Remove once no longer needed

    #endregion

    #region Public Methods

    // TODO-14: These should probably be made thread-safe
    // TODO-14: Logging
    // TODO-14: We should probably handle exceptions here and return null and log error?

    public T? GetObject<T>(string key)
        where T : class
    {
        if (_dataCache.TryGetValue(key, out object obj))
            return (T)obj;

        if (!_additionalData.TryGetValue(key, out obj))
            return null;

        if (obj is not JObject jObj)
            return null;

        T? parsedObj = jObj.ToObject<T>();

        if (parsedObj == null)
            return null;

        _dataCache[key] = parsedObj;

        return parsedObj;
    }

    public T GetValue<T>(string key) where T : struct => GetValue<T>(key, default);

    public T GetValue<T>(string key, T defaultValue)
        where T : struct
    {
        if (!_additionalData.TryGetValue(key, out object obj))
            return defaultValue;

        if (obj is T value)
            return value;

        if (typeof(T).IsEnum)
            return (T)Convert.ChangeType(obj, Enum.GetUnderlyingType(typeof(T)));
        else if (typeof(T) == typeof(FileSystemPath)) // Ugly hack - perhaps we should make FileSystemPath convertible?
            return (T)(object)new FileSystemPath((string)obj);
        else
            return (T)Convert.ChangeType(obj, typeof(T));
    }

    public void SetObject<T>(string key, T obj)
        where T : class
    {
        _additionalData[key] = JObject.FromObject(obj);
        _dataCache[key] = obj;
    }

    public void SetValue<T>(string key, T obj)
        where T : struct
    {
        _additionalData[key] = obj;
    }

    #endregion
}