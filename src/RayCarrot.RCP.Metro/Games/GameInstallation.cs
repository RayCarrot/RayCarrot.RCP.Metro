using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

// TODO-14: Add properties like install size, add date etc.

[JsonObject(MemberSerialization.OptIn)]
public class GameInstallation : ProgramInstallation, IComparable<GameInstallation>
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

    public string GameId => GameDescriptor.GameId;
    public string FullId => $"{GameId}|{InstallationId}"; // TODO-14: Use this for some logging

    public LegacyGame? LegacyGame => GameDescriptor.LegacyGame; // TODO-14: Remove once no longer needed

    #endregion

    #region Public Methods

    public int CompareTo(GameInstallation? other)
    {
        if (this == other) 
            return 0;
        if (other == null) 
            return 1;

        // TODO-14: How do we handle sorting if user has added two of the same game? Display name? Add date? Custom?
        //          Problem with custom sorting then is that if we ever update the sorting we can't really do it
        //          without breaking the custom sort. Which might be fine if it auto-sorts after update?

        return GameDescriptor.CompareTo(other.GameDescriptor);
    }

    #endregion
}