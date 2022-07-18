#nullable disable
using System.IO;
using BinarySerializer;
using Ionic.Zlib;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A resource entry for data packed in a patch file
/// </summary>
public class PatchFileResourceEntry : BinarySerializable
{
    public PatchFilePath FilePath { get; set; }
    public byte[] Checksum { get; set; }
    public long DataOffset { get; set; }
    public long CompressedDataLength { get; set; }
    public long DecompressedDataLength { get; set; }
    public bool IsCompressed => CompressedDataLength != 0;

    public Stream ReadData(BinaryDeserializer s, Pointer baseOffset)
    {
        long length = CompressedDataLength == 0 ? DecompressedDataLength : CompressedDataLength;

        s.Goto(baseOffset + DataOffset);
        byte[] buffer = s.SerializeArray<byte>(default, length, name: "Data");

        MemoryStream memStream = new(buffer);

        if (IsCompressed)
            return new FixedLengthWrapperStream(new DeflateStream(memStream, CompressionMode.Decompress), DecompressedDataLength);
        else
            return memStream;
    }

    public override void SerializeImpl(SerializerObject s)
    {
        FilePath = s.SerializeObject<PatchFilePath>(FilePath, name: nameof(FilePath));
        Checksum = s.SerializeArraySize<byte, byte>(Checksum, name: nameof(Checksum));
        Checksum = s.SerializeArray<byte>(Checksum, Checksum.Length, name: nameof(Checksum));
        DataOffset = s.Serialize<long>(DataOffset, name: nameof(DataOffset));
        CompressedDataLength = s.Serialize<long>(CompressedDataLength, name: nameof(CompressedDataLength));
        DecompressedDataLength = s.Serialize<long>(DecompressedDataLength, name: nameof(DecompressedDataLength));
    }
}