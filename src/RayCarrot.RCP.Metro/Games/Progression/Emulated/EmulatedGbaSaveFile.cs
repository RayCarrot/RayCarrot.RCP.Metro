using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public class EmulatedGbaSaveFile : EmulatedSaveFile
{
    public EmulatedGbaSaveFile(FileSystemPath filePath) : base(filePath) { }

    public override async Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation)
    {
        using RCPContext context = new(FilePath.Parent);
        await gameInstallation.GetComponents<InitializeContextComponent>().InvokeAllAsync(context);

        return new EmulatedSave[]
        {
            new EmulatedGbaSave(this, context, FilePath.Name)
        };
    }
}