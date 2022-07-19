#nullable disable
using System;
using BinarySerializer;

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
    public int Revision { get; set; }

    public long TotalSize { get; set; }
    public long ModifiedDateValue { get; set; }
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
        Revision = s.Serialize<int>(Revision, name: nameof(Revision));

        TotalSize = s.Serialize<long>(TotalSize, name: nameof(TotalSize));

        ModifiedDateValue = s.Serialize<long>(ModifiedDateValue, name: nameof(ModifiedDateValue));
        s.Log($"ModifiedDate: {ModifiedDate}");
    }

    #endregion
}