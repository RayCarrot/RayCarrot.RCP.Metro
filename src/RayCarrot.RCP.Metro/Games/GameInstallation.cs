using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

[JsonObject(MemberSerialization.OptIn)]
public class GameInstallation
{
    #region Constructors

    public GameInstallation(Games game, GameType gameType, FileSystemPath installLocation, bool isRCPInstalled) 
        : this(game, gameType, installLocation, isRCPInstalled, new Dictionary<string, object>()) 
    { }

    [JsonConstructor]
    private GameInstallation(Games game, GameType gameType, FileSystemPath installLocation, bool isRCPInstalled, Dictionary<string, object>? additionalData)
    {
        Game = game;
        GameType = gameType;
        InstallLocation = installLocation;
        IsRCPInstalled = isRCPInstalled;
        _additionalData = additionalData ?? new Dictionary<string, object>();

        GameInfo = Game.GetGameInfo();
    }

    #endregion

    #region Private Fields

    [JsonProperty(PropertyName = "AdditionalData")]
    private readonly Dictionary<string, object> _additionalData;
    private readonly Dictionary<string, object> _dataCache = new();

    #endregion

    #region Public Properties

    [JsonProperty(PropertyName = "Game")]
    public Games Game { get; }

    [JsonProperty(PropertyName = "Type")]
    public GameType GameType { get; } // TODO-14: Remove/replace this with new type system

    [JsonProperty(PropertyName = "InstallLocation")]
    public FileSystemPath InstallLocation { get; } // TODO-14: Rename to GamePath?

    /// <summary>
    /// Indicates if the game was installed through the Rayman Control Panel
    /// </summary>
    [JsonProperty(PropertyName = "IsRCPInstalled")]
    public bool IsRCPInstalled { get; } // TODO-14: Have this be additional data? Have an object for info on how it was installed? Not all games can be installed through RCP, so not all games need this by default.

    // TODO-14: Eventually change this
    public GameInfo GameInfo { get; }

    // TODO-14: Eventually we might want to get rid of the Games enum, so use this ID as much as possible for now
    public string ID => Game.ToString();

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