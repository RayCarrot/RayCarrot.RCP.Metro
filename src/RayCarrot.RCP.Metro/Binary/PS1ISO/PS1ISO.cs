#nullable disable
using BinarySerializer;
using BinarySerializer.Disk.ISO9660;

namespace RayCarrot.RCP.Metro;

public class PS1ISO : DiscImage
{
    public Sector<PS1ISOLicense> License { get; set; }

    protected override void SerializeSystemSectors(SerializerObject s)
    {
        // 0-3 are unused
        License = SerializeSector(s, License, 4, name: nameof(License));
        // 5-11 have the PlayStation logo
        // 12-15 are unused
    }
}