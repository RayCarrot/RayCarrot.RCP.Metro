using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;

public class DeltaFile : BinarySerializable
{
    public DeltaChunk[] Chunks { get; set; }

    public static DeltaFile Create(FileSystemPath sourceFile, FileSystemPath modifiedFile)
    {
        byte[] sourceData = File.ReadAllBytes(sourceFile);
        byte[] modifiedData = File.ReadAllBytes(modifiedFile);

        List<DeltaChunk> chunks = new();
        List<byte> chunkData = new();
        long chunkOffset = -1;

        for (int i = 0; i < modifiedData.Length; i++)
        {
            if (i >= sourceData.Length || sourceData[i] != modifiedData[i])
            {
                if (chunkOffset == -1)
                    chunkOffset = i;

                chunkData.Add(modifiedData[i]);
            }
            else if (chunkOffset != -1)
            {
                chunks.Add(new DeltaChunk()
                {
                    PatchOffset = chunkOffset,
                    PatchData = chunkData.ToArray(),
                });
                chunkOffset = -1;
                chunkData.Clear();
            }
        }

        if (chunkOffset != -1)
            chunks.Add(new DeltaChunk()
            {
                PatchOffset = chunkOffset,
                PatchData = chunkData.ToArray(),
            });

        return new DeltaFile()
        {
            Chunks = chunks.ToArray()
        };
    }

    public override void SerializeImpl(SerializerObject s)
    {
        s.SerializeMagicString("DELTA", 6);
        Chunks = s.SerializeArraySize<DeltaChunk, long>(Chunks, name: nameof(Chunks));
        Chunks = s.SerializeObjectArray<DeltaChunk>(Chunks, Chunks.Length, name: nameof(Chunks));
    }
}