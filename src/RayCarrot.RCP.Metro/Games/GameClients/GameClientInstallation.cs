using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Games.Clients;

[JsonObject(MemberSerialization.OptIn)]
public class GameClientInstallation : ProgramInstallation, IComparable<GameClientInstallation>
{
    #region Constructors

    public GameClientInstallation(GameClientDescriptor gameClientDescriptor, FileSystemPath installLocation)
        : this(gameClientDescriptor, installLocation, GenerateInstallationID(), new Dictionary<string, object?>())
    { }

    [JsonConstructor]
    private GameClientInstallation(
        GameClientDescriptor gameClientDescriptor,
        FileSystemPath installLocation, 
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

    #endregion

    #region Public Methods

    public int CompareTo(GameClientInstallation? other)
    {
        if (this == other)
            return 0;
        if (other == null)
            return 1;

        // TODO-14: Also allow custom sorting? Same as with GameInstallation.

        return GameClientDescriptor.CompareTo(other.GameClientDescriptor);
    }

    #endregion
}