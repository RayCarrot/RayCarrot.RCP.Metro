using System.Collections.Generic;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

// TODO-14: Add properties like install size, add date etc.
// TODO-14: It's getting confusing with multiple ids. Rename Id -> GameId, Id -> EmulatorId, InstallationId -> same

[JsonObject(MemberSerialization.OptIn)]
public class GameInstallation : ProgramInstallation
{
    #region Constructors

    public GameInstallation(GameDescriptor gameDescriptor, FileSystemPath installLocation) 
        : this(gameDescriptor, installLocation, GenerateInstallationID(), new Dictionary<string, object>()) 
    { }

    [JsonConstructor]
    private GameInstallation(
        GameDescriptor gameDescriptor, 
        FileSystemPath installLocation, 
        string installationId, 
        Dictionary<string, object>? additionalData) 
        : base(installLocation, installationId, additionalData)
    {
        GameDescriptor = gameDescriptor;
    }

    #endregion

    #region Public Properties

    [JsonProperty(PropertyName = "Id")]
    [JsonConverter(typeof(StringGameDescriptorConverter))]
    public GameDescriptor GameDescriptor { get; }

    public string Id => GameDescriptor.Id;
    public string FullId => $"{Id}|{InstallationId}"; // TODO-14: Use this for some logging

    public Games? LegacyGame => GameDescriptor.LegacyGame; // TODO-14: Remove once no longer needed

    #endregion
}