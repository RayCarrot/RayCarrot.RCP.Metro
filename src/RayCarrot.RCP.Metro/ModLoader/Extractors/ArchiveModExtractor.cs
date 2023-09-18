using System.IO;
using RayCarrot.RCP.Metro.Legacy.Patcher;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public abstract class ArchiveModExtractor : ModExtractor
{
    private static IArchiveEntry? FindModMetadata(IArchive archive)
    {
        return archive.Entries.FirstOrDefault(x => x.Key == Mod.MetadataFileName);
    }

    private static IArchiveEntry? FindLegacyPatchPackageEntry(IArchive archive)
    {
        return archive.Entries.FirstOrDefault(x => 
            !x.IsDirectory && 
            x.Key.EndsWith(PatchPackage.FileExtension, StringComparison.InvariantCultureIgnoreCase));
    }

    private static async Task ExtractArchiveAsync(IArchive archive, FileSystemPath outputPath, Action<Progress> progressCallback, CancellationToken cancellationToken)
    {
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

    protected abstract IArchive OpenArchive(FileSystemPath filePath);

    public override string[] GetGameTargets(FileSystemPath modFilePath)
    {
        using IArchive archive = OpenArchive(modFilePath);

        IArchiveEntry? modMetadataEntry = FindModMetadata(archive);

        if (modMetadataEntry != null)
        {
            using Stream entryStream = modMetadataEntry.OpenEntryStream();
            try
            {
                ModMetadata metadata = JsonHelpers.DeserializeFromStream<ModMetadata>(entryStream);
                return metadata.Games;
            }
            catch (Exception ex)
            {
                throw new InvalidModException($"The mod metadata file is invalid. {ex.Message}", ex);
            }
        }
        else
        {
            // Check if the archive has a legacy patch package. We need to support this
            // as these were previously uploaded in .zip files to GameBanana. If there
            // is one found we can safely assume this is a legacy mod.
            IArchiveEntry? legacyPatch = FindLegacyPatchPackageEntry(archive);

            if (legacyPatch == null)
                throw new InvalidModException("The archive does not contain a mod. A metadata file must be present.");

            using TempFile tempFile = new(false);
            legacyPatch.WriteToFile(tempFile.TempPath);
            LegacyGamePatchModExtractor legacyExtractor = new();
            return legacyExtractor.GetGameTargets(tempFile.TempPath);
        }
    }

    public override async Task ExtractAsync(FileSystemPath modFilePath, FileSystemPath outputPath, Action<Progress> progressCallback, CancellationToken cancellationToken)
    {
        using IArchive archive = OpenArchive(modFilePath);

        if (FindModMetadata(archive) != null)
        {
            await ExtractArchiveAsync(archive, outputPath, progressCallback, cancellationToken);
        }
        else
        {
            // Check if the archive has a legacy patch package. We need to support this
            // as these were previously uploaded in .zip files to GameBanana. If there
            // is one found we can safely assume this is a legacy mod.
            IArchiveEntry? legacyPatch = FindLegacyPatchPackageEntry(archive);

            if (legacyPatch == null)
                throw new InvalidModException("The archive does not contain a mod. A metadata file must be present.");

            using TempFile tempFile = new(false);
            legacyPatch.WriteToFile(tempFile.TempPath);
            LegacyGamePatchModExtractor legacyExtractor = new();
            await legacyExtractor.ExtractAsync(tempFile.TempPath, outputPath, progressCallback, cancellationToken);
        }
    }
}