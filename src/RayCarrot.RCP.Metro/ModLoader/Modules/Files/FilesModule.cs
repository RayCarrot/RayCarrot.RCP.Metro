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

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override string Id => "files";
    public override LocalizedString Description => new ResourceLocString(nameof(Resources.ModLoader_FilesModule_Description));

    public override ModModuleViewModel GetViewModel(GameInstallation gameInstallation) => new FilesModuleViewModel(this, gameInstallation);

    private static string[] ParsePathsToCreate(string pathsText)
    {
        return pathsText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
    }

    public override void SetupModuleFolder(ModModuleViewModel viewModel, FileSystemPath modulePath)
    {
        // Create directory for added files
        Directory.CreateDirectory(modulePath + AddedFilesDirectoryName);
    
        // Create user specified directories
        foreach (string path in ParsePathsToCreate(((FilesModuleViewModel)viewModel).PathsText))
        {
            try
            {
                Directory.CreateDirectory(modulePath + AddedFilesDirectoryName + path);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Creating user specified files directory");
            }
        }

        // Create user specified directories for archives
        foreach (FilesModuleViewModel.ArchiveViewModel archive in ((FilesModuleViewModel)viewModel).Archives)
        {
            if (!archive.IsEnabled)
                continue;

            // Create archive directory
            Directory.CreateDirectory(modulePath + AddedFilesDirectoryName + archive.FilePath);

            // Create user specified directories
            foreach (string path in ParsePathsToCreate(archive.PathsText))
            {
                try
                {
                    Directory.CreateDirectory(modulePath + AddedFilesDirectoryName + archive.FilePath + path);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Creating user specified files directory");
                }
            }
        }

        // Create file for removed files
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