﻿using RayCarrot.RCP.Metro.Legacy.Patcher;

namespace RayCarrot.RCP.Metro;

public class LegacyGamePatchFileLaunchHandler : FileLaunchHandler
{
    // TODO-LOC
    public override LocalizedString DisplayName => "Game patch (.gp)";
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
        await Services.UI.ShowModLoaderAsync(new[] { filePath });
    }
}