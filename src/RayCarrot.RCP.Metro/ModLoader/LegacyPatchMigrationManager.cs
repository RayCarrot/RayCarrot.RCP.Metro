using System.IO;
using BinarySerializer;
using RayCarrot.RCP.Metro.Legacy.Patcher;
using RayCarrot.RCP.Metro.ModLoader.Extractors;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader;

public class LegacyPatchMigrationManager
{
    #region Constructor

    public LegacyPatchMigrationManager(GameInstallation gameInstallation, ModLibrary library)
    {
        GameInstallation = gameInstallation;
        Library = library;
        _patchesDir = GameInstallation.InstallLocation.Directory + ".patches";
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly FileSystemPath _patchesDir;

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ModLibrary Library { get; }

    #endregion

    #region Private Methods

    private static ModFilePath ConvertPath(PatchFilePath path) => new(path.FilePath, path.Location, path.LocationID);

    private async Task MigratePatchesAsync(PatchLibraryPackage patchLibrary, Action<Progress> progressCallback)
    {
        LegacyGamePatchModExtractor patchExtractor = new();

        ModManifest modManifest = new(new Dictionary<string, ModManifestEntry>());
        Progress progress = new(0, patchLibrary.Patches.Length);

        foreach (PatchLibraryPackagePatchEntry patchEntry in patchLibrary.Patches)
        {
            Logger.Trace("Migrating patch with id {0}", patchEntry.ID);

            using TempDirectory extractDir = new(true);

            string patchFileName = $"{patchEntry.ID}{PatchPackage.FileExtension}";

            await patchExtractor.ExtractAsync(_patchesDir + patchFileName, extractDir.TempPath, x => progressCallback(progress.Add(x, 1)), CancellationToken.None);

            long modSize = (long)extractDir.TempPath.GetSize().Bytes;

            Library.InstallMod(extractDir.TempPath, patchEntry.ID, false);

            using Context context = new RCPContext(_patchesDir);
            PatchPackage patch = context.ReadRequiredFileData<PatchPackage>(patchFileName);
            ModVersion version = new(patch.Metadata.Version_Major, patch.Metadata.Version_Minor,
                patch.Metadata.Version_Revision);

            // TODO-UPDATE: We want prev. downloadable patches to include the GameBanana install info so they can keep receiving updates.
            //              So we need to hard-code a list of some ids to include the info for once the mods get uploaded to GameBanana.
            ModInstallInfo installInfo = new(null, version, modSize, DateTime.Now, null);
            modManifest.Mods[patchEntry.ID] = new ModManifestEntry(patchEntry.ID, installInfo, patchEntry.IsEnabled);

            progress++;
        }

        Library.WriteModManifest(modManifest);
    }

    private void MigrateHistory(PatchLibraryPackage patchLibrary)
    {
        LibraryFileHistoryBuilder builder = new();

        foreach (PatchFilePath addedFile in patchLibrary.History.AddedFiles)
        {
            builder.AddAddedFile(ConvertPath(addedFile));
        }

        for (int i = 0; i < patchLibrary.History.ReplacedFiles.Length; i++)
        {
            PatchFilePath replacedFile = patchLibrary.History.ReplacedFiles[i];
            PackagedResourceEntry replacedFileResource = patchLibrary.History.ReplacedFileResources[i];

            using Stream resourceStream = replacedFileResource.ReadData(patchLibrary.Context, true);
            ModFilePath path = ConvertPath(replacedFile);

            builder.AddReplacedFile(path, new VirtualModFileResource(path, resourceStream));
        }

        for (int i = 0; i < patchLibrary.History.RemovedFiles.Length; i++)
        {
            PatchFilePath removedFile = patchLibrary.History.RemovedFiles[i];
            PackagedResourceEntry removedFileResource = patchLibrary.History.RemovedFileResources[i];

            using Stream resourceStream = removedFileResource.ReadData(patchLibrary.Context, true);
            ModFilePath path = ConvertPath(removedFile);

            builder.AddRemovedFile(path, new VirtualModFileResource(path, resourceStream));
        }

        builder.BuildFileHistory(Library);
    }

    #endregion

    #region Public Methods

    public bool CanMigrate() => _patchesDir.DirectoryExists && !Library.IsInitialized;

    public async Task MigrateAsync(Action<Progress> progressCallback)
    {
        Logger.Info("Migrating legacy patches to mods");

        using (Context context = new RCPContext(_patchesDir))
        {
            PatchLibraryPackage? patchLibrary = context.ReadFileData<PatchLibraryPackage>($"library{PatchLibraryPackage.FileExtension}");

            if (patchLibrary == null)
            {
                context.Dispose();
                Services.File.DeleteDirectory(_patchesDir);
                return;
            }

            try
            {
                await MigratePatchesAsync(patchLibrary, progressCallback);
                MigrateHistory(patchLibrary);
            }
            catch
            {
                // Delete the library if the migration fails since it's incomplete by this point
                Library.DeleteLibrary();
                throw;
            }
        }

        Services.File.DeleteDirectory(_patchesDir);
        
        Logger.Info("Finished migrating legacy patches");
    }

    #endregion
}