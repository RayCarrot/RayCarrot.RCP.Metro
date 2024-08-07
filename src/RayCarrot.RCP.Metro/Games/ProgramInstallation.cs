﻿using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ProgramInstallation
{
    #region Constructors

    protected ProgramInstallation(InstallLocation installLocation, string installationId, Dictionary<string, object?>? data)
    {
        InstallLocation = installLocation;
        InstallationId = installationId ?? throw new ArgumentNullException(nameof(installationId));
        _data = data ?? new Dictionary<string, object?>();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    [JsonProperty(PropertyName = "Data")]
    private readonly Dictionary<string, object?> _data;

    private readonly List<DataChangedCallback> _dataChangedCallbacks = new();
    private Dictionary<string, object>? _cache;

    #endregion

    #region Public Properties

    /// <summary>
    /// The install location of this program
    /// </summary>
    [JsonProperty(PropertyName = "InstallLocation")]
    public InstallLocation InstallLocation { get; }

    /// <summary>
    /// The unique ID of this specific installation. This is needed to ensure we can reference an installation
    /// even after serializing to JSON. Removing and adding back this installation will give it a new ID.
    /// </summary>
    [JsonProperty(PropertyName = "InstallationId")]
    public string InstallationId { get; }

    #endregion

    #region Private Methods

    private void OnDataChanged(string key)
    {
        foreach (DataChangedCallback dataChangedCallback in _dataChangedCallbacks)
        {
            if (dataChangedCallback.Key == key)
                dataChangedCallback.Callback();
        }
    }

    #endregion

    #region Protected Methods

    protected static string GenerateInstallationID() => Guid.NewGuid().ToString();

    #endregion

    #region Public Methods

    public T GetRequiredObject<T>(string key) where T : class =>
        GetObject<T>(key) ?? throw new Exception($"The object with key {key} and type {typeof(T)} could not be found");

    public T? GetObject<T>(string key)
        where T : class
    {
        lock (_data)
        {
            // Try and get the object. If not found then return null.
            if (!_data.TryGetValue(key, out object? obj))
                return null;

            // Always return null if the object is null
            if (obj is null)
                return null;

            // If the object has already been parsed we can return that
            if (obj is T cachedObj)
                return cachedObj;

            // If the object has not been parsed we expect it to be a JObject,
            // otherwise we're trying to access the object with different types.
            if (obj is not JObject jObj)
            {
                Logger.Warn("The data object {0} type {1} does not match the requested type {2}", key, obj.GetType(), typeof(T));
                return null;
            }

            T? parsedObj;

            try
            {
                // Parse the JObject
                parsedObj = jObj.ToObject<T>();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "The data object {0} could not be parsed as {1}", ex, typeof(T));
                parsedObj = null;
            }

            // Cache the parsed object
            _data[key] = parsedObj;

            return parsedObj;
        }
    }

    public T? GetValue<T>(string key) => GetValue<T>(key, default);

    public T? GetValue<T>(string key, T? defaultValue)
    {
        lock (_data)
        {
            if (!_data.TryGetValue(key, out object? obj))
                return defaultValue;

            if (obj is T value)
                return value;

            T? convertedValue;

            try
            {
                // TODO: Potentially improve this converting code to handle more cases in a better way. For example, what if
                //       the enum is serialized as a string?
                if (typeof(T).IsEnum)
                    convertedValue = (T)Convert.ChangeType(obj, Enum.GetUnderlyingType(typeof(T)));
                else if (typeof(T) == typeof(FileSystemPath)) // Ugly hack - perhaps we should make FileSystemPath convertible?
                    convertedValue = (T)(object)new FileSystemPath((string?)obj);
                else
                    convertedValue = (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "The data value {0} could not be converted to {1}", ex, typeof(T));
                convertedValue = defaultValue;
            }

            _data[key] = convertedValue;

            return convertedValue;
        }
    }

    public void SetObject<T>(string key, T? obj)
        where T : class
    {
        lock (_data)
        {
            if (obj is null)
                _data.Remove(key);
            else
                _data[key] = obj;
        }
        
        OnDataChanged(key);
    }

    public void ModifyObject<T>(string key, Action<T> modifyObjectAction)
        where T : class, new()
    {
        lock (_data)
        {
            T? obj = GetObject<T>(key);

            if (obj is null)
            {
                obj = new T();
                _data[key] = obj;
            }

            modifyObjectAction(obj);
            SetObject(key, obj); // Don't really need to do this step anymore, but let's keep it anyway
        }
    }

    public void SetValue<T>(string key, T obj)
    {
        lock (_data)
        {
            if (obj is null)
                _data.Remove(key);
            else
                _data[key] = obj;
        }

        OnDataChanged(key);
    }

    public void AddDataChangedCallback(string key, Action callback)
    {
        _dataChangedCallbacks.Add(new DataChangedCallback(key, callback));
    }

    public void RemoveDataChangedCallback(string key, Action callback)
    {
        _dataChangedCallbacks.RemoveAll(x => x.Key == key && x.Callback == callback);
    }

    public void CacheObject<T>(string key, T obj)
        where T : class
    {
        _cache ??= new Dictionary<string, object>();
        _cache[key] = obj;
    }

    public bool TryGetCachedObject<T>(string key, [NotNullWhen(true)] out T? obj)
        where T : class
    {
        if (_cache == null)
        {
            obj = null;
            return false;
        }
        else
        {
            bool success = _cache.TryGetValue(key, out object cachedObj);

            if (success && cachedObj is T cachedObjOfT)
            {
                obj = cachedObjOfT;
                return true;
            }
            else
            {
                obj = null;
                return false;
            }
        }
    }

    #endregion

    #region Records

    private readonly record struct DataChangedCallback(string Key, Action Callback);

    #endregion
}