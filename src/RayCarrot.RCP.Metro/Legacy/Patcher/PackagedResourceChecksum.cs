#nullable disable
using System.IO;
using System.Security.Cryptography;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Legacy.Patcher;

// NOTE: Currently the checksum values are unused. The checksum gets calculated when creating a patch and then copied into
//       here, but not used for anything. The original intention was to use them for verifying that a file had not been
//       changed outside of the patch, but doing all of those calculations each time you opened the patcher would probably
//       be quite slow. So for now we keep them here, unused, until they might be needed in the future.

public class PackagedResourceChecksum : BinarySerializable
{
    public byte[] Checksum_SHA256 { get; set; }

    /// <summary>
    /// Creates a new checksum for a stream
    /// </summary>
    /// <param name="stream">The stream</param>
    /// <returns>The calculated checksum</returns>
    public static PackagedResourceChecksum FromStream(Stream stream)
    {
        PackagedResourceChecksum c = new();

        using SHA256Managed sha = new();
        c.Checksum_SHA256 = sha.ComputeHash(stream);
        
        return c;
    }

    public override void SerializeImpl(SerializerObject s)
    {
        Checksum_SHA256 = s.SerializeArraySize<byte, byte>(Checksum_SHA256, name: nameof(Checksum_SHA256));
        Checksum_SHA256 = s.SerializeArray<byte>(Checksum_SHA256, Checksum_SHA256.Length, name: nameof(Checksum_SHA256));
    }
}