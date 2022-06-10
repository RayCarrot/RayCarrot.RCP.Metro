#nullable disable
using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class ProcessMemoryStreamFile : MemoryMappedStreamFile
{
    public ProcessMemoryStreamFile(
        Context context, 
        string name, 
        long baseAddress, 
        long? memoryRegionLength, 
        Stream stream, 
        Endian? endianness = null, 
        long memoryMappedPriority = -1, 
        Pointer parentPointer = null, 
        bool leaveOpen = false) 
        : base(context, name, baseAddress, stream, endianness, memoryMappedPriority, parentPointer, leaveOpen)
    {
        MemoryRegionLength = memoryRegionLength;
    }

    public override BinaryFile GetPointerFile(long serializedValue, Pointer anchor = null)
    {
        // If haven't specified a length for this memory region then we can only assume the pointer should be in this file
        if (MemoryRegionLength == null)
            return this;
        // If we have a length we check the other regions with a length too and see if this pointer is in the range of one of them
        else
            return GetMemoryMappedPointerFile(serializedValue, anchor) ?? this;
    }

    protected override BinaryFile GetLocalPointerFile(long serializedValue, Pointer anchor = null)
    {
        // If we do not have a length we can't check this, so return null
        if (MemoryRegionLength == null)
            return null;

        long anchorOffset = anchor?.AbsoluteOffset ?? 0;

        if (serializedValue + anchorOffset >= BaseAddress && serializedValue + anchorOffset <= BaseAddress + MemoryRegionLength)
            return this;

        return null;
    }

    public long? MemoryRegionLength { get; }
    public override bool SavePointersToMemoryMap => false;
    public override bool IgnoreCacheOnRead => true;

    public void DisposeStream() => Stream.Dispose();
}