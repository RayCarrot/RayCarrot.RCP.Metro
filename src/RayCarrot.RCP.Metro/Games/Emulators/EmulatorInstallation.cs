using System.Collections.Generic;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

[JsonObject(MemberSerialization.OptIn)]
public class EmulatorInstallation : ProgramInstallation
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
}