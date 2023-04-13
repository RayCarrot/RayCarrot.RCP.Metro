namespace RayCarrot.RCP.Metro;

public abstract class EmulatedSaveFile
{
    protected EmulatedSaveFile(FileSystemPath filePath)
    {
        FilePath = filePath;
    }

    public FileSystemPath FilePath { get; }

    public abstract Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation);
}