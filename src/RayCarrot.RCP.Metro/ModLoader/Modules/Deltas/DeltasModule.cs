using System.IO;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;

public class DeltasModule : ModModule
{
    public DeltasModule(FileSystemPath? singleFilePath)
    {
        SingleFilePath = singleFilePath;
    }

    public const string FileExtension = ".delta";
    public const string SingleFileDeltaPatchName = "game.delta";

    public override string Id => "deltas";
    // TODO-LOC
    public override LocalizedString Description => IsSingleFile 
        ? "This is used to patch parts of the game file. The file name for single-file games should be 'game.delta'. Deltas from multiple mods can be stacked."
        : "This is used to patch parts of a file. This should mainly be used for binary files which are known not to change much, such as an exe or game rom. Deltas from multiple mods can be stacked.";

    public FileSystemPath? SingleFilePath { get; }
    public bool IsSingleFile => SingleFilePath != null;

    public override ModModuleViewModel GetViewModel() => new DeltasModuleViewModel(this, IsSingleFile);

    public override IReadOnlyCollection<IFilePatch> GetPatchedFiles(Mod mod, FileSystemPath modulePath)
    {
        List<IFilePatch> filePatches = new();

        if (IsSingleFile)
        {
            FileSystemPath file = modulePath + SingleFileDeltaPatchName;

            if (file.FileExists)
            {
                ModFilePath modFilePath = new(mod.GameInstallation.InstallLocation.GetRequiredFileName());
                filePatches.Add(new DeltaFilePatch(modFilePath, file));
            }
        }
        else
        {
            FileExtension deltaExt = new(FileExtension);

            if (mod.Metadata.Archives == null || mod.Metadata.Archives.Length == 0)
            {
                foreach (FileSystemPath file in Directory.EnumerateFiles(modulePath, $"*{deltaExt}", SearchOption.AllDirectories))
                {
                    ModFilePath modFilePath = new(file.RemoveFileExtension() - modulePath);
                    filePatches.Add(new DeltaFilePatch(modFilePath, file));
                }
            }
            else
            {
                foreach (FileSystemPath file in Directory.EnumerateFiles(modulePath, $"*{deltaExt}", SearchOption.AllDirectories))
                {
                    string relativeFilePath = file.RemoveFileExtension() - modulePath;
                    bool inArchive = false;

                    foreach (ModArchiveInfo archive in mod.Metadata.Archives)
                    {
                        if (relativeFilePath.StartsWith(archive.FilePath))
                        {
                            ModFilePath modFilePath = new(relativeFilePath.Substring(archive.FilePath.Length + 1), archive.FilePath, archive.Id);
                            filePatches.Add(new DeltaFilePatch(modFilePath, file));
                            inArchive = true;
                            break;
                        }
                    }

                    if (!inArchive)
                    {
                        ModFilePath modFilePath = new(relativeFilePath);
                        filePatches.Add(new DeltaFilePatch(modFilePath, file));
                    }
                }
            }
        }

        return filePatches.AsReadOnly();
    }
}