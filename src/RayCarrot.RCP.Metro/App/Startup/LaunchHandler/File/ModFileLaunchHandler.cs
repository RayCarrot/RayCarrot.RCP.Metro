using RayCarrot.RCP.Metro.Legacy.Patcher;
using RayCarrot.RCP.Metro.ModLoader.Extractors;

namespace RayCarrot.RCP.Metro;

public class ModFileLaunchHandler : FileLaunchHandler
{
    public override bool DisableFullStartup => true;

    // Only use the legacy patch package file extension for associations since
    // we don't want ones like .zip to be associated with this program
    public override FileAssociationInfo FileAssociationInfo => new(
        FileExtension: PatchPackage.FileExtension,
        Id: "RCP_Metro.GamePatch",
        Name: "Rayman Control Panel Game Patch",
        GetIconFunc: () => Files.GamePatch,
        IconFileName: "GamePatch.ico");

    public override bool IsValid(FileSystemPath filePath)
    {
        FileExtension fileExtension = filePath.FileExtension;
        return ModExtractor.GetModExtractors().Any(x => x.FileExtension == fileExtension);
    }

    public override async void Invoke(FileSystemPath filePath, State state)
    {
        // Show the mod loader
        await Services.UI.ShowModLoaderAsync(new[] { filePath });
    }
}