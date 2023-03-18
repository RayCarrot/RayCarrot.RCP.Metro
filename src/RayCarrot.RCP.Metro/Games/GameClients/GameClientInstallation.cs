using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Games.Clients;

[JsonObject(MemberSerialization.OptIn)]
public class GameClientInstallation : ProgramInstallation, IComparable<GameClientInstallation>
{
    #region Constructors

    public GameClientInstallation(GameClientDescriptor gameClientDescriptor, InstallLocation installLocation)
        : this(gameClientDescriptor, installLocation, GenerateInstallationID(), new Dictionary<string, object?>())
    { }

    [JsonConstructor]
    private GameClientInstallation(
        GameClientDescriptor gameClientDescriptor,
        InstallLocation installLocation, 
        string installationId,
        Dictionary<string, object?>? data) 
        : base(installLocation, installationId, data)
    {
        GameClientDescriptor = gameClientDescriptor;
    }

    #endregion

    #region Public Properties

    [JsonProperty(PropertyName = "Id")]
    [JsonConverter(typeof(StringGameClientDescriptorConverter))]
    public GameClientDescriptor GameClientDescriptor { get; }

    public string GameClientId => GameClientDescriptor.GameClientId;
    public string FullId => $"{GameClientId}|{InstallationId}";

    #endregion

    #region Public Methods

    public int CompareTo(GameClientInstallation? other)
    {
        if (this == other)
            return 0;
        if (other == null)
            return 1;

        return GameClientDescriptor.CompareTo(other.GameClientDescriptor);
    }

    #endregion
}