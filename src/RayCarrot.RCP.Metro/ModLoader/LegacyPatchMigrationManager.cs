using System.IO;
using BinarySerializer;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Legacy.Patcher;
using RayCarrot.RCP.Metro.ModLoader.Extractors;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Resource;
using RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

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
    
    // Hard-code GB info for previously installable patches so they can receive updates
    private readonly Dictionary<string, GameBananaInstallData> _patchGameBananaInstallData = new()
    {
        ["88080deb-5f26-4d08-b44f-b0b6b36d1e22"] = new GameBananaInstallData(479402, 1088420), // Rayman 2 - Ray2Fix
        ["4bd00c57-a459-4349-a76c-1ec58838c949"] = new GameBananaInstallData(479416, 1088460), // Rayman 2 - Swedish (Svenska) Fan Translation
        ["e75debed-e663-4f4d-92ed-61c20ba06645"] = new GameBananaInstallData(479430, 1088494), // Rayman 2 - Portuguese (Português) Fan Translation
        ["a003c436-4866-4d8c-aaa7-620057df16b7"] = new GameBananaInstallData(480005, 1090214), // Rayman 2 - Irish (Gaeilge) Fan Translation
        ["2712e279-f822-44d7-a601-15eb02186dd3"] = new GameBananaInstallData(479823, 1089657), // Rayman 2 - Higher Quality Official Textures
        ["dd074196-a28b-4354-819d-68caa9fc9527"] = new GameBananaInstallData(480010, 1090221), // Rayman 2 - Remove Pirate Head DRM
        ["a936601c-b563-417b-ad71-24dc1c3dadf0"] = new GameBananaInstallData(480013, 1090226), // Rayman 2 - AI Upscaled Texture Pack
        ["2712c87a-4146-472e-b5c4-ea7209c25379"] = new GameBananaInstallData(398611, 1088357), // Rayman 3 - Increased CNT File Size Limit
        ["b3c021e8-5da6-4596-aa43-84f57813c2d2"] = new GameBananaInstallData(480151, 1090620), // Rayman Arena - Multiplayer Patch
        ["86256503-f4cd-4724-acac-cc8e583cedf5"] = new GameBananaInstallData(480156, 1090633), // Rayman Origins - High Quality Videos
        ["f7efb9d2-e1b9-4930-b855-fcb51d16e781"] = new GameBananaInstallData(479778, 1089533), // Tonic Trouble - Compatibility Patch
        ["07aed475-5432-483a-8506-e8d1d25c4c88"] = new GameBananaInstallData(479789, 1089561), // Tonic Trouble Special Edition - Compatibility Patch
    };

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

            long modSize = extractDir.TempPath.GetSize();

            Library.InstallMod(extractDir.TempPath, patchEntry.ID, false);

            using Context context = new RCPContext(_patchesDir);
            PatchPackage patch = context.ReadRequiredFileData<PatchPackage>(patchFileName);
            ModVersion version = new(patch.Metadata.Version_Major, patch.Metadata.Version_Minor,
                patch.Metadata.Version_Revision);

            string? source = null;
            JObject? data = null;

            if (_patchGameBananaInstallData.TryGetValue(patchEntry.ID, out GameBananaInstallData installData))
            {
                source = new GameBananaModsSource().Id;
                data = JObject.FromObject(installData);
            }

            ModInstallInfo installInfo = new(source, version, modSize, DateTime.Now, data);
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