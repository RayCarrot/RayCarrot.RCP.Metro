using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public class EmulatedGbaSaveFile : EmulatedSaveFile
{
    public EmulatedGbaSaveFile(FileSystemPath filePath) : base(filePath) { }

    public override Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation)
    {
        RCPContext context = new(FilePath.Parent);
        gameInstallation.GetComponents<InitializeContextComponent>().InvokeAll(context);

        return Task.FromResult(new EmulatedSave[]
        {
            new EmulatedGbaSave(this, context, FilePath.Name)
        });
    }
}