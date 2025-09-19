#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Legacy.Patcher;

public class PatchLibraryPackageHistory : BinarySerializable
{
    private long ModifiedDateValue { get; set; }
    public DateTime ModifiedDate
    {
        get => DateTime.FromBinary(ModifiedDateValue);
        set => ModifiedDateValue = value.ToBinary();
    }

    public PatchFilePath[] AddedFiles { get; set; }
    public PackagedResourceChecksum[] AddedFileChecksums { get; set; }

    public PatchFilePath[] ReplacedFiles { get; set; }
    public PackagedResourceChecksum[] ReplacedFileChecksums { get; set; }
    public PackagedResourceEntry[] ReplacedFileResources { get; set; }

    public PatchFilePath[] RemovedFiles { get; set; }
    public PackagedResourceEntry[] RemovedFileResources { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        ModifiedDateValue = s.Serialize<long>(ModifiedDateValue, name: nameof(ModifiedDateValue));
        s.Log($"ModifiedDate: {ModifiedDate}");

        AddedFiles = s.SerializeArraySize<PatchFilePath, int>(AddedFiles, name: nameof(AddedFiles));
        AddedFiles = s.SerializeObjectArray<PatchFilePath>(AddedFiles, AddedFiles.Length, name: nameof(AddedFiles));

        AddedFileChecksums = s.SerializeArraySize<PackagedResourceChecksum, int>(AddedFileChecksums, name: nameof(AddedFileChecksums));
        AddedFileChecksums = s.SerializeObjectArray<PackagedResourceChecksum>(AddedFileChecksums, AddedFileChecksums.Length, name: nameof(AddedFileChecksums));

        ReplacedFiles = s.SerializeArraySize<PatchFilePath, int>(ReplacedFiles, name: nameof(ReplacedFiles));
        ReplacedFiles = s.SerializeObjectArray<PatchFilePath>(ReplacedFiles, ReplacedFiles.Length, name: nameof(ReplacedFiles));

        ReplacedFileChecksums = s.SerializeArraySize<PackagedResourceChecksum, int>(ReplacedFileChecksums, name: nameof(ReplacedFileChecksums));
        ReplacedFileChecksums = s.SerializeObjectArray<PackagedResourceChecksum>(ReplacedFileChecksums, ReplacedFileChecksums.Length, name: nameof(ReplacedFileChecksums));

        ReplacedFileResources = s.SerializeArraySize<PackagedResourceEntry, int>(ReplacedFileResources, name: nameof(ReplacedFileResources));
        ReplacedFileResources = s.SerializeObjectArray<PackagedResourceEntry>(ReplacedFileResources, ReplacedFileResources.Length, name: nameof(ReplacedFileResources));

        if (ReplacedFiles.Length != ReplacedFileChecksums.Length || ReplacedFiles.Length != ReplacedFileResources.Length)
            throw new BinarySerializableException(this, $"The replaced file array lengths don't match ({ReplacedFiles.Length}, {ReplacedFileChecksums.Length}, {ReplacedFileResources.Length})");

        RemovedFiles = s.SerializeArraySize<PatchFilePath, int>(RemovedFiles, name: nameof(RemovedFiles));
        RemovedFiles = s.SerializeObjectArray<PatchFilePath>(RemovedFiles, RemovedFiles.Length, name: nameof(RemovedFiles));

        RemovedFileResources = s.SerializeArraySize<PackagedResourceEntry, int>(RemovedFileResources, name: nameof(RemovedFileResources));
        RemovedFileResources = s.SerializeObjectArray<PackagedResourceEntry>(RemovedFileResources, RemovedFileResources.Length, name: nameof(RemovedFileResources));

        if (RemovedFiles.Length != RemovedFileResources.Length)
            throw new BinarySerializableException(this, $"The removed file array lengths don't match ({RemovedFiles.Length}, {RemovedFileResources.Length})");
    }
}