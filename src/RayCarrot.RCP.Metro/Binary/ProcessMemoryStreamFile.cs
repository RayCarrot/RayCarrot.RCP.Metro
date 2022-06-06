#nullable disable
using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class ProcessMemoryStreamFile : StreamFile
{
    public ProcessMemoryStreamFile(Context context, string name, Stream stream, Endian? endianness = null, bool allowLocalPointers = false, Pointer parentPointer = null, bool leaveOpen = false) : base(context, name, stream, endianness, allowLocalPointers, parentPointer, leaveOpen) { }

    public override BinaryFile GetPointerFile(long serializedValue, Pointer anchor = null)
    {
        if ((anchor == null || anchor.AbsoluteOffset == 0) && (serializedValue == 0 || serializedValue == 0xFFFFFFFF))
            return null;

        return this;
    }

    public override bool SavePointersToMemoryMap => false;
    public override bool IgnoreCacheOnRead => true;
}