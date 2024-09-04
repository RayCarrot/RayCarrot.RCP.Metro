using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Extractors;

namespace RayCarrot.RCP.Metro;

public class ModFileLaunchHandler : FileLaunchHandler
{
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.LauncHandler_Mod));
    public override bool DisableFullStartup => true;

    // We don't want extensions such as .zip to be associated with this program
    public override FileAssociationInfo? FileAssociationInfo => null;
    
    public override bool IsValid(FileSystemPath filePath)
    {
        FileExtension fileExtension = filePath.FileExtension;
        return ModExtractor.GetModExtractors().Any(x => x.FileExtension == fileExtension);
    }

    public override async void Invoke(FileSystemPath filePath, State state)
    {
        // Show the mod loader
        await Services.UI.ShowModLoaderAsync(new ModLoaderViewModel.ModToInstall(filePath, null, null));
    }
}