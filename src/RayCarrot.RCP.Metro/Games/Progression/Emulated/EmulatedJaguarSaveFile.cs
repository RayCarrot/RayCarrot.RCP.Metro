namespace RayCarrot.RCP.Metro;

public class EmulatedJaguarSaveFile : EmulatedSaveFile
{
    public EmulatedJaguarSaveFile(FileSystemPath filePath) : base(filePath) { }

    public override Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation)
    {
        using RCPContext context = new(FilePath.Parent);
        context.Initialize(gameInstallation);

        return Task.FromResult(new EmulatedSave[]
        {
            new EmulatedJaguarSave(this, context, FilePath.Name)
        });
    }
}