namespace RayCarrot.RCP.Metro;

public class EmulatedGbcSaveFile : EmulatedSaveFile
{
    public EmulatedGbcSaveFile(FileSystemPath filePath) : base(filePath) { }

    public override Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation)
    {
        using RCPContext context = new(FilePath.Parent);
        context.Initialize(gameInstallation);

        return Task.FromResult(new EmulatedSave[]
        {
            new EmulatedGbcSave(this, context, FilePath.Name)
        });
    }
}