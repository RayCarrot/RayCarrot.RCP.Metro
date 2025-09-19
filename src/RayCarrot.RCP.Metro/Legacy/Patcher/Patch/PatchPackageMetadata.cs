using BinarySerializer;

namespace RayCarrot.RCP.Metro.Legacy.Patcher;

public class PatchPackageMetadata : BinarySerializable
{
    #region Public Properties

    public int Pre_FormatVersion { get; set; }

    public string ID { get; set; }

    public string[] GameIds { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Website { get; set; }
    public int Version_Major { get; set; }
    public int Version_Minor { get; set; }
    public int Version_Revision { get; set; }
    public PatchPackageChangelogEntry[] ChangelogEntries { get; set; }

    public long TotalSize { get; set; }
    private long ModifiedDateValue { get; set; }
    public DateTime ModifiedDate
    {
        get => DateTime.FromBinary(ModifiedDateValue);
        set => ModifiedDateValue = value.ToBinary();
    }

    #endregion

    #region Public Methods

    public override void SerializeImpl(SerializerObject s)
    {
        ID = s.SerializeString(ID, name: nameof(ID));

        // Starting with version 14.0 of RCP games are now identified by the descriptor id
        // and a patch can be made for multiple games
        if (Pre_FormatVersion >= 2)
        {
            GameIds = s.SerializeArraySize<string, int>(GameIds, name: nameof(GameIds));
            GameIds = s.SerializeStringArray(GameIds, GameIds.Length, name: nameof(GameIds));
        }
        else
        {
            string legacyGameId = s.SerializeString(null, name: "LegacyGameId");

            // Convert to game ids
            GameIds = Services.Games.GetGameDescriptorsFromLegacyId(legacyGameId).Select(x => x.GameId).ToArray();
        }

        Name = s.SerializeString(Name, name: nameof(Name));
        Description = s.SerializeString(Description, name: nameof(Description));
        Author = s.SerializeString(Author, name: nameof(Author));

        if (Pre_FormatVersion >= 1)
            Website = s.SerializeString(Website, name: nameof(Website));
        else
            Website = String.Empty;

        Version_Major = s.Serialize<int>(Version_Major, name: nameof(Version_Major));
        Version_Minor = s.Serialize<int>(Version_Minor, name: nameof(Version_Minor));
        Version_Revision = s.Serialize<int>(Version_Revision, name: nameof(Version_Revision));

        if (Pre_FormatVersion >= 3)
        {
            ChangelogEntries = s.SerializeArraySize<PatchPackageChangelogEntry, int>(ChangelogEntries, name: nameof(ChangelogEntries));
            ChangelogEntries = s.SerializeObjectArray<PatchPackageChangelogEntry>(ChangelogEntries, ChangelogEntries.Length, name: nameof(ChangelogEntries));
        }
        else
        {
            ChangelogEntries = Array.Empty<PatchPackageChangelogEntry>();
        }

        TotalSize = s.Serialize<long>(TotalSize, name: nameof(TotalSize));

        ModifiedDateValue = s.Serialize<long>(ModifiedDateValue, name: nameof(ModifiedDateValue));
        s.Log($"ModifiedDate: {ModifiedDate}");
    }

    #endregion
}