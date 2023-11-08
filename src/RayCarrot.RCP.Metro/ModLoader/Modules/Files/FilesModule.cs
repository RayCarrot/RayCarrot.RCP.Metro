using System.IO;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.Files;

/// <summary>
/// The most basic module. This provides support for adding/replacing and removing files from the game.
/// </summary>
public class FilesModule : ModModule
{
    public const string AddedFilesDirectoryName = "added_files";
    public const string RemovedFilesFileName = "removed_files.txt";

    public override string Id => "files";
    public override LocalizedString Description => new ResourceLocString(nameof(Resources.ModLoader_FilesModule_Description));

    public override void SetupModuleFolder(ModModuleViewModel viewModel, FileSystemPath modulePath)
    {
        Directory.CreateDirectory(modulePath + AddedFilesDirectoryName);
        File.Create(modulePath + RemovedFilesFileName).Dispose();
    }

    public override IReadOnlyCollection<IModFileResource> GetAddedFiles(Mod mod, FileSystemPath modulePath)
    {
        List<IModFileResource> fileResources = new();

        FileSystemPath filesPath = modulePath + AddedFilesDirectoryName;

        if (!filesPath.DirectoryExists)
            return fileResources.AsReadOnly();

        if (mod.Metadata.Archives == null || mod.Metadata.Archives.Length == 0)
        {
            foreach (FileSystemPath file in Directory.EnumerateFiles(filesPath, "*", SearchOption.AllDirectories))
            {
                ModFilePath modFilePath = new(file - filesPath);
                fileResources.Add(new PhysicalModFileResource(modFilePath, file));
            }
        }
        else
        {
            foreach (FileSystemPath file in Directory.EnumerateFiles(filesPath, "*", SearchOption.AllDirectories))
            {
                string relativeFilePath = file - filesPath;
                bool inArchive = false;

                foreach (ModArchiveInfo archive in mod.Metadata.Archives)
                {
                    if (relativeFilePath.StartsWith(archive.FilePath))
                    {
                        ModFilePath modFilePath = new(relativeFilePath.Substring(archive.FilePath.Length + 1), archive.FilePath, archive.Id);
                        fileResources.Add(new PhysicalModFileResource(modFilePath, file));
                        inArchive = true;
                        break;
                    }
                }

                if (!inArchive)
                {
                    ModFilePath modFilePath = new(relativeFilePath);
                    fileResources.Add(new PhysicalModFileResource(modFilePath, file));
                }
            }
        }

        return fileResources.AsReadOnly();
    }

    public override IReadOnlyCollection<ModFilePath> GetRemovedFiles(Mod mod, FileSystemPath modulePath)
    {
        List<ModFilePath> filePaths = new();

        FileSystemPath removedFilesFilePath = modulePath + RemovedFilesFileName;

        if (!removedFilesFilePath.FileExists)
            return filePaths.AsReadOnly();

        string[] removedFiles = File.ReadAllLines(removedFilesFilePath);

        if (mod.Metadata.Archives == null || mod.Metadata.Archives.Length == 0)
        {
            filePaths.AddRange(removedFiles.Select(x => new ModFilePath(x)));
        }
        else
        {
            foreach (string removedFile in removedFiles)
            {
                bool inArchive = false;

                foreach (ModArchiveInfo archive in mod.Metadata.Archives)
                {
                    if (removedFile.StartsWith(archive.FilePath))
                    {
                        filePaths.Add(new ModFilePath(removedFile.Substring(archive.FilePath.Length + 1), archive.FilePath, archive.Id));
                        inArchive = true;
                        break;
                    }
                }

                if (!inArchive)
                    filePaths.Add(new ModFilePath(removedFile));
            }
        }

        return filePaths.AsReadOnly();
    }
}