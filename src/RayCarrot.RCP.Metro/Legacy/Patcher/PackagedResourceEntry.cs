#nullable disable
using System.IO;
using BinarySerializer;
using Ionic.Zlib;

namespace RayCarrot.RCP.Metro.Legacy.Patcher;

/// <summary>
/// A resource entry for data packed in a package file
/// </summary>
public class PackagedResourceEntry : BinarySerializable
{
    private Func<Stream> _getPendingImportFunc;
    private bool _isPendingImportCompressed;

    public long DataOffset { get; set; }
    public long CompressedDataLength { get; set; }
    public long DecompressedDataLength { get; set; }

    public Stream ReadData(Context context, bool decompress)
    {
        if (Offset == null)
            throw new InvalidOperationException("Can't read data from an uninitialized resource entry");

        // Now you may wonder, why do we pass in the context to this method if we already have it here? Well for clarify. This
        // method will open the context again and without it being passed in that is not very clear! We don't want to end up
        // opening file streams multiple times by mistake.
        if (context != Context)
            throw new InvalidOperationException("Can't read data using a different context than the one the object was initialized with");

        long length = CompressedDataLength == 0 ? DecompressedDataLength : CompressedDataLength;
        
        BinaryDeserializer s = context.Deserializer;
        s.Goto(Offset.File.StartPointer + DataOffset);
        byte[] buffer = s.SerializeArray<byte>(default, length, name: "Data");

        MemoryStream memStream = new(buffer);

        if (decompress)
            return new FixedLengthWrapperStream(new DeflateStream(memStream, CompressionMode.Decompress), DecompressedDataLength);
        else
            return memStream;
    }

    public void SetPendingImport(Func<Stream> getPendingImportFunc, bool isCompressed)
    {
        _getPendingImportFunc = getPendingImportFunc;
        _isPendingImportCompressed = isCompressed;
    }

    public (Stream Stream, bool IsCompressed) GetPendingImport()
    {
        Func<Stream> getPendingImportFunc = _getPendingImportFunc;
        ClearPendingImport();
        return (getPendingImportFunc?.Invoke(), _isPendingImportCompressed);
    }

    public void ClearPendingImport()
    {
        _getPendingImportFunc = null;
        _isPendingImportCompressed = false;
    }

    public override void SerializeImpl(SerializerObject s)
    {
        DataOffset = s.Serialize<long>(DataOffset, name: nameof(DataOffset));
        CompressedDataLength = s.Serialize<long>(CompressedDataLength, name: nameof(CompressedDataLength));
        DecompressedDataLength = s.Serialize<long>(DecompressedDataLength, name: nameof(DecompressedDataLength));
    }
}