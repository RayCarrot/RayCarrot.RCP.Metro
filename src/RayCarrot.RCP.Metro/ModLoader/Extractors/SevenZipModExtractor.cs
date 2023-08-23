using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;

namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public class SevenZipModExtractor : ArchiveModExtractor
{
    public override FileExtension FileExtension => new(".7z");
    protected override IArchive OpenArchive(FileSystemPath filePath) => SevenZipArchive.Open(filePath);
}