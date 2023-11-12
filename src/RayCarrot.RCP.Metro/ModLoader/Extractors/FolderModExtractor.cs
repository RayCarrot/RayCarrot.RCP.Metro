using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public class FolderModExtractor : ModExtractor
{
    public override FileExtension FileExtension => new(".jsonc");

    public override string[] GetGameTargets(FileSystemPath modFilePath)
    {
        try
        {
            ModMetadata metadata = JsonHelpers.DeserializeFromFile<ModMetadata>(modFilePath);
            return metadata.Games;
        }
        catch (Exception ex)
        {
            throw new InvalidModException($"The mod metadata file is invalid. {ex.Message}", ex);
        }
    }

    public override Task ExtractAsync(FileSystemPath modFilePath, FileSystemPath outputPath, Action<Progress> progressCallback, CancellationToken cancellationToken)
    {
        progressCallback(Progress.Empty);

        Services.File.CopyDirectory(modFilePath.Parent, outputPath, true, true);
        
        progressCallback(Progress.Complete);
        
        return Task.CompletedTask;
    }
}