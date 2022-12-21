using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ProgramInstallation
{
    #region Constructors

    protected ProgramInstallation(FileSystemPath installLocation, string installationId, Dictionary<string, object>? additionalData)
    {
        InstallLocation = installLocation;
        InstallationId = installationId ?? throw new ArgumentNullException(nameof(installationId));
        _additionalData = additionalData ?? new Dictionary<string, object>();
    }

    #endregion

    #region Private Fields

    [JsonProperty(PropertyName = "AdditionalData")]
    private readonly Dictionary<string, object> _additionalData;
    private readonly Dictionary<string, object> _dataCache = new();

    #endregion

    #region Public Properties

    /// <summary>
    /// The install location of this program. This can either be a directory or a file path depending on
    /// the program. For example most installed games expect a directory, but an emulator or emulated
    /// ROM-based game would expect a file.
    /// </summary>
    [JsonProperty(PropertyName = "InstallLocation")]
    public FileSystemPath InstallLocation { get; }

    /// <summary>
    /// The unique ID of this specific installation. This is needed to ensure we can reference an installation
    /// even after serializing to JSON. Removing and adding back this installation will give it a new ID.
    /// </summary>
    [JsonProperty(PropertyName = "InstallationId")]
    public string InstallationId { get; }

    // TODO-14: Add user-defined name

    #endregion

    #region Protected Methods

    protected static string GenerateInstallationID() => Guid.NewGuid().ToString();

    #endregion

    #region Public Methods

    // TODO-14: These should probably be made thread-safe
    // TODO-14: Logging
    // TODO-14: We should probably handle exceptions here and return null and log error?

    public T GetRequiredObject<T>(string key) where T : class =>
        GetObject<T>(key) ?? throw new Exception($"The object with key {key} and type {typeof(T)} could not be found");

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

    public T GetOrCreateObject<T>(string key)
        where T : class, new()
    {
        if (_dataCache.TryGetValue(key, out object obj))
            return (T)obj;

        if (!_additionalData.TryGetValue(key, out obj) ||
            obj is not JObject jObj)
        {
            T newObj = new();
            SetObject(key, newObj);
            return newObj;
        }

        T? parsedObj = jObj.ToObject<T>();

        if (parsedObj == null)
        {
            T newObj = new();
            SetObject(key, newObj);
            return newObj;
        }

        _dataCache[key] = parsedObj;

        return parsedObj;
    }

    public T? GetValue<T>(string key) => GetValue<T>(key, default);

    public T? GetValue<T>(string key, T? defaultValue)
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

    public void SetObject<T>(string key, T? obj)
        where T : class
    {
        if (obj is null)
        {
            _additionalData.Remove(key);
            _dataCache.Remove(key);
        }
        else
        {
            _additionalData[key] = JObject.FromObject(obj);
            _dataCache[key] = obj;
        }
    }

    public void SetValue<T>(string key, T obj)
    {
        if (obj is null)
            _additionalData.Remove(key);
        else
            _additionalData[key] = obj;
    }

    #endregion
}