namespace RayCarrot.RCP.Metro.ModLoader.Extractors;

public abstract class ModExtractor
{
    private static readonly ModExtractor[] _modExtractors = 
    {
        new ZipModExtractor(), // .zip
        new SevenZipModExtractor(), // .7z
        new RarModExtractor(), // .rar
        new LegacyGamePatchModExtractor(), // .gp (legacy)
    };

    public static ModExtractor[] GetModExtractors() => _modExtractors;

    public abstract FileExtension FileExtension { get; }

    public abstract string[] GetGameTargets(FileSystemPath modFilePath);

    public abstract Task ExtractAsync(FileSystemPath modFilePath, FileSystemPath outputPath, Action<Progress> progressCallback, CancellationToken cancellationToken);
}