using System.IO;
using RayCarrot.RCP.Metro.Legacy.Patcher;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public abstract class ArchiveModExtractor : ModExtractor
{
    protected abstract IArchive OpenArchive(FileSystemPath filePath);

    public override async Task ExtractAsync(FileSystemPath modFilePath, FileSystemPath outputPath, Action<Progress> progressCallback, CancellationToken cancellationToken)
    {
        using IArchive archive = OpenArchive(modFilePath);

        // Check if the archive has a legacy patch package. We need to support this
        // as these were previously uploaded in .zip files to GameBanana. If there
        // is one found we can safely assume this is a legacy mod.
        IArchiveEntry? legacyPatch = archive.Entries.
            FirstOrDefault(x => !x.IsDirectory && x.Key.EndsWith(PatchPackage.FileExtension, StringComparison.InvariantCultureIgnoreCase));

        if (legacyPatch != null)
        {
            using TempFile tempFile = new(false);
            legacyPatch.WriteToFile(tempFile.TempPath);
            LegacyGamePatchModExtractor legacyExtractor = new();
            await legacyExtractor.ExtractAsync(tempFile.TempPath,  outputPath, progressCallback, cancellationToken);
            return;
        }

        using IReader reader = archive.ExtractAllEntries();

        Progress progress = new(0, archive.TotalSize);

        while (reader.MoveToNextEntry())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.Entry.IsDirectory)
                continue;
            
            using EntryStream decompStream = reader.OpenEntryStream();

            FileSystemPath outputFilePath = outputPath + reader.Entry.Key;

            Directory.CreateDirectory(outputFilePath.Parent);

            using FileStream outputStream = File.Create(outputFilePath);
            await decompStream.CopyToExAsync(outputStream, x => progressCallback(progress.Add(x, reader.Entry.Size)), cancellationToken);

            progress += reader.Entry.Size;
        }

        progressCallback(progress.Completed());
    }
}