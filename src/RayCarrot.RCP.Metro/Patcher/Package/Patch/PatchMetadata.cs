#nullable disable
using System;
using BinarySerializer;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchMetadata : BinarySerializable
{
    #region Public Properties

    public string ID { get; set; }
    public string GameName { get; set; }
    public Games Game
    {
        get => (Games)Enum.Parse(typeof(Games), GameName);
        set => GameName = value.ToString();
    }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    [JsonProperty]
    private int Version_Major { get; set; }
    [JsonProperty]
    private int Version_Minor { get; set; }
    [JsonProperty]
    private int Version_Revision { get; set; }
    [JsonIgnore]
    public Version Version
    {
        get => new(Version_Major, Version_Minor, Version_Revision);
        set
        {
            Version_Major = value.Major;
            Version_Minor = value.Minor;
            Version_Revision = value.Build;
        }
    }

    public long TotalSize { get; set; }
    private long ModifiedDateValue { get; set; }
    public DateTime ModifiedDate
    {
        get => DateTime.FromBinary(ModifiedDateValue);
        set => ModifiedDateValue = value.ToBinary();
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Generates a new unique ID for a patch
    /// </summary>
    /// <returns>The generated ID</returns>
    public static string GenerateID() => Guid.NewGuid().ToString();

    #endregion

    #region Public Methods

    public override void SerializeImpl(SerializerObject s)
    {
        ID = s.SerializeString(ID, name: nameof(ID));

        GameName = s.SerializeString(GameName, name: nameof(GameName));

        Name = s.SerializeString(Name, name: nameof(Name));
        Description = s.SerializeString(Description, name: nameof(Description));
        Author = s.SerializeString(Author, name: nameof(Author));
        Version_Major = s.Serialize<int>(Version_Major, name: nameof(Version_Major));
        Version_Minor = s.Serialize<int>(Version_Minor, name: nameof(Version_Minor));
        Version_Revision = s.Serialize<int>(Version_Revision, name: nameof(Version_Revision));

        TotalSize = s.Serialize<long>(TotalSize, name: nameof(TotalSize));

        ModifiedDateValue = s.Serialize<long>(ModifiedDateValue, name: nameof(ModifiedDateValue));
        s.Log($"ModifiedDate: {ModifiedDate}");
    }

    #endregion
}