namespace RayCarrot.RCP.Metro;

public class EmulatedGbaSaveFile : EmulatedSaveFile
{
    public EmulatedGbaSaveFile(FileSystemPath filePath) : base(filePath) { }

    public override Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation)
    {
        using RCPContext context = new(FilePath.Parent);
        context.Initialize(gameInstallation);

        return Task.FromResult(new EmulatedSave[]
        {
            new EmulatedGbaSave(this, context, FilePath.Name)
        });
    }
}