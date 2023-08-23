using SharpCompress.Archives;
using SharpCompress.Archives.Rar;

namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public class RarModExtractor : ArchiveModExtractor
{
    public override FileExtension FileExtension => new(".rar");
    protected override IArchive OpenArchive(FileSystemPath filePath) => RarArchive.Open(filePath);
}