namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public abstract class ModExtractor
{
    public abstract FileExtension FileExtension { get; }

    public abstract Task ExtractAsync(FileSystemPath modFilePath, FileSystemPath outputPath, Action<Progress> progressCallback, CancellationToken cancellationToken);
}