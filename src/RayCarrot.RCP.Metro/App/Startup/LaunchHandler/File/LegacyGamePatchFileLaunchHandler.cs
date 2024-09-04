using RayCarrot.RCP.Metro.Legacy.Patcher;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

namespace RayCarrot.RCP.Metro;

public class LegacyGamePatchFileLaunchHandler : FileLaunchHandler
{
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.LaunchHandler_LegacyPatch));
    public override bool DisableFullStartup => true;

    public override FileAssociationInfo FileAssociationInfo => new(
        FileExtension: PatchPackage.FileExtension,
        Id: "RCP_Metro.GamePatch",
        Name: "Rayman Control Panel Game Patch",
        GetIconFunc: () => Files.GamePatch,
        IconFileName: "GamePatch.ico");

    public override bool IsValid(FileSystemPath filePath)
    {
        return filePath.FileExtension == new FileExtension(PatchPackage.FileExtension);
    }

    public override async void Invoke(FileSystemPath filePath, State state)
    {
        // Show the mod loader
        await Services.UI.ShowModLoaderAsync(new ModLoaderViewModel.ModToInstall(filePath, null, null));
    }
}