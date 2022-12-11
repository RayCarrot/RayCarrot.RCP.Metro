using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Games.Emulators;

[JsonObject(MemberSerialization.OptIn)]
public class EmulatorInstallation : ProgramInstallation, IComparable<EmulatorInstallation>
{
    #region Constructors

    public EmulatorInstallation(EmulatorDescriptor emulatorDescriptor, FileSystemPath installLocation)
        : this(emulatorDescriptor, installLocation, GenerateInstallationID(), new Dictionary<string, object>())
    { }

    [JsonConstructor]
    private EmulatorInstallation(
        EmulatorDescriptor emulatorDescriptor,
        FileSystemPath installLocation, 
        string installationId,
        Dictionary<string, object>? additionalData) 
        : base(installLocation, installationId, additionalData)
    {
        EmulatorDescriptor = emulatorDescriptor;
    }

    #endregion

    #region Public Properties

    [JsonProperty(PropertyName = "Id")]
    [JsonConverter(typeof(StringEmulatorDescriptorConverter))]
    public EmulatorDescriptor EmulatorDescriptor { get; }

    #endregion

    #region Public Methods

    public int CompareTo(EmulatorInstallation? other)
    {
        if (this == other)
            return 0;
        if (other == null)
            return 1;

        // TODO-14: Also allow custom sorting? Same as with GameInstallation.

        return EmulatorDescriptor.CompareTo(other.EmulatorDescriptor);
    }

    #endregion
}