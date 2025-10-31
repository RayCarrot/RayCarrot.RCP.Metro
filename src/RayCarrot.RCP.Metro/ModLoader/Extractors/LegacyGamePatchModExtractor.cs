﻿using System.IO;
using BinarySerializer;
using RayCarrot.RCP.Metro.Legacy.Patcher;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Modules.Files;

namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public class LegacyGamePatchModExtractor : ModExtractor
{
    public override FileExtension FileExtension => new(PatchPackage.FileExtension);

    public override string[] GetGameTargets(FileSystemPath modFilePath)
    {
        using Context context = new RCPContext(modFilePath.Parent);
        PatchPackage patch = context.ReadRequiredFileData<PatchPackage>(modFilePath.Name);

        return patch.Metadata.GameIds;
    }

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
            Archives: archives,
            Dependencies: null);
        JsonHelpers.SerializeToFile(modMetadata, outputPath + Mod.MetadataFileName);

        // Extract thumbnail
        if (patch.HasThumbnail)
        {
            using Stream thumbOutputStream = File.Create(outputPath + Mod.ThumbnailFileName);
            await patch.ThumbnailResource.ReadData(context, true).CopyToAsync(thumbOutputStream);
        }

        // Create a files module
        FilesModule filesModule = new();

        // Write removed files, if any
        if (patch.RemovedFiles.Any())
        {
            Directory.CreateDirectory(outputPath + filesModule.Id);
            File.WriteAllLines(outputPath + filesModule.Id + FilesModule.RemovedFilesFileName, patch.RemovedFiles.ToArray(x => x.FullFilePath));
        }

        // Extract added files
        for (int i = 0; i < patch.AddedFileResources.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PatchFilePath filePath = patch.AddedFiles[i];
            PackagedResourceEntry resource = patch.AddedFileResources[i];

            progressCallback(new Progress(i, patch.AddedFiles.Length));

            FileSystemPath fileDest = outputPath + filesModule.Id + FilesModule.AddedFilesDirectoryName + filePath.FullFilePath;
            Directory.CreateDirectory(fileDest.Parent);

            using FileStream dstStream = File.Create(fileDest);
            using Stream srcStream = resource.ReadData(context, true);

            await srcStream.CopyToAsync(dstStream);
        }

        progressCallback(Progress.Complete);
    }
}