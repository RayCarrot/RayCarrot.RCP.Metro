using SharpCompress.Archives;
using SharpCompress.Archives.Zip;

namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public class ZipModExtractor : ArchiveModExtractor
{
    public override FileExtension FileExtension => new(".zip");
    protected override IArchive OpenArchive(FileSystemPath filePath) => ZipArchive.Open(filePath);
}