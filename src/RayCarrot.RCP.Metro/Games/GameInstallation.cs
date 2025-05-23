﻿using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using RayCarrot.RCP.Metro.Games.Components;
namespace RayCarrot.RCP.Metro;

[JsonObject(MemberSerialization.OptIn)]
public class GameInstallation : ProgramInstallation, IComparable<GameInstallation>
{
    #region Constructors

    public GameInstallation(GameDescriptor gameDescriptor, InstallLocation installLocation) 
        : this(gameDescriptor, installLocation, GenerateInstallationID(), new Dictionary<string, object?>()) 
    { }

    [JsonConstructor]
    private GameInstallation(
        GameDescriptor gameDescriptor,
        InstallLocation installLocation, 
        string installationId, 
        Dictionary<string, object?>? data) 
        : base(installLocation, installationId, data)
    {
        GameDescriptor = gameDescriptor;

        GameId = gameDescriptor.GameId;
        FullId = $"{gameDescriptor.GameId}|{installationId}";
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private GameComponentProvider? ComponentProvider { get; set; }

    #endregion

    #region Public Properties

    [JsonProperty(PropertyName = "Id")]
    [JsonConverter(typeof(StringGameDescriptorConverter))]
    public GameDescriptor GameDescriptor { get; }

    public string GameId { get; }
    public string FullId { get; }

    #endregion

    #region Public Methods

    public bool HasComponent<T>() 
        where T : GameComponent => 
        ComponentProvider?.HasComponent<T>() ?? false;
    public T? GetComponent<T>() 
        where T : GameComponent => 
        ComponentProvider?.GetComponent<T>();
    public T GetRequiredComponent<T>() 
        where T : GameComponent => 
        ComponentProvider?.GetComponent<T>() ?? throw new InvalidOperationException($"Component of type {typeof(T)} was not found");
    public U GetRequiredComponent<T, U>() 
        where T : GameComponent
        where U : T =>
        ComponentProvider?.GetComponents<T>().OfType<U>().FirstOrDefault() ?? throw new InvalidOperationException($"Component of type {typeof(U)} and registered as type {typeof(T)} was not found");
    public IEnumerable<T> GetComponents<T>() 
        where T : GameComponent => 
        ComponentProvider?.GetComponents<T>() ?? Enumerable.Empty<T>();

    [MemberNotNull(nameof(ComponentProvider))]
    public void RebuildComponents()
    {
        ComponentProvider = GameDescriptor.RegisterComponents(this).BuildProvider(this);
        Logger.Trace("Rebuilt components for {0}", FullId);
    }

    public int CompareTo(GameInstallation? other)
    {
        if (this == other) 
            return 0;
        if (other == null) 
            return 1;

        return GameDescriptor.CompareTo(other.GameDescriptor);
    }

    #endregion
}