using System.IO;
using BinarySerializer;
using RayCarrot.RCP.Metro.Legacy.Patcher;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public class LegacyGamePatchModExtractor : ModExtractor
{
    public override FileExtension FileExtension => new(PatchPackage.FileExtension);

    public override async Task ExtractAsync(FileSystemPath modFilePath, FileSystemPath outputPath, Action<Progress> progressCallback, CancellationToken cancellationToken)
    {
        using Context context = new RCPContext(modFilePath.Parent);
        PatchPackage patch = context.ReadRequiredFileData<PatchPackage>(modFilePath.Name);

        // Get every used archive
        ModArchiveInfo[] archives = patch.AddedFiles.
            Concat(patch.RemovedFiles).
            GroupBy(x => x.Location).
            Where(x => x.Key != String.Empty).
            Select(x => new ModArchiveInfo(x.First().LocationID, x.Key)).
            ToArray();

        // Write metadata
        ModMetadata modMetadata = new(
            Id: patch.Metadata.ID,
            Games: patch.Metadata.GameIds,
            Format: Mod.LatestFormatVersion,
            Name: patch.Metadata.Name,
            Description: patch.Metadata.Description,
            Author: patch.Metadata.Author,
            Website: patch.Metadata.Website,
            Version: new ModVersion(patch.Metadata.Version_Major, patch.Metadata.Version_Minor, patch.Metadata.Version_Revision),
            Changelog: patch.Metadata.ChangelogEntries.ToArray(x => 
                new ModChangelogEntry(new ModVersion(x.Version_Major, x.Version_Minor, x.Version_Revision), x.Date, x.Description)), 
            Archives: archives);
        JsonHelpers.SerializeToFile(modMetadata, outputPath + Mod.MetadataFileName);

        // Write removed files, if any
        if (patch.RemovedFiles.Any())
        {
            Directory.CreateDirectory(outputPath + Mod.DefaultVersion);
            File.WriteAllLines(outputPath + Mod.DefaultVersion + Mod.RemovedFilesFileName, patch.RemovedFiles.ToArray(x => x.FullFilePath));
        }

        // Extract thumbnail
        if (patch.HasThumbnail)
        {
            using Stream thumbOutputStream = File.Create(outputPath + Mod.ThumbnailFileName);
            await patch.ThumbnailResource.ReadData(context, true).CopyToAsync(thumbOutputStream);
        }

        // Extract added files
        for (int i = 0; i < patch.AddedFileResources.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PatchFilePath filePath = patch.AddedFiles[i];
            PackagedResourceEntry resource = patch.AddedFileResources[i];

            progressCallback(new Progress(i, patch.AddedFiles.Length));

            FileSystemPath fileDest = outputPath + Mod.DefaultVersion + Mod.FilesDirectoryName + filePath.FullFilePath;
            Directory.CreateDirectory(fileDest.Parent);

            using FileStream dstStream = File.Create(fileDest);
            using Stream srcStream = resource.ReadData(context, true);

            await srcStream.CopyToAsync(dstStream);
        }

        progressCallback(Progress.Complete);
    }
}