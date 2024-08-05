using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.ModLoader.Extractors;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Resource;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModLoaderViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ModLoaderViewModel(GameInstallation gameInstallation, ModToInstall[]? pendingModFiles = null)
    {
        GameInstallation = gameInstallation;
        _pendingModFiles = pendingModFiles;

        // Verify the game supports mods
        if (!gameInstallation.GetComponents<ModModuleComponent>().Any())
            throw new InvalidOperationException("The game installation doesn't support mods");

        _httpClient = new HttpClient(); // TODO: Share a single client throughout the app?
        _modExtractors = ModExtractor.GetModExtractors();

        LoaderViewModel = new LoaderViewModel();
        Mods = new ObservableCollection<ModViewModel>();

        ModProcessor = new ModProcessor(GameInstallation);
        Library = new ModLibrary(GameInstallation);

        ModifiedFiles = new ModifiedFilesViewModel(GameInstallation);
        DownloadableMods = new DownloadableModsViewModel(this, GameInstallation, _httpClient, DownloadableModsSource.GetSources());

        InstallModFromFileCommand = new AsyncRelayCommand(InstallModFromFileAsync);
        CheckForUpdatesCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private ModToInstall[]? _pendingModFiles;
    private readonly HttpClient _httpClient;
    private readonly ModExtractor[] _modExtractors;

    #endregion

    #region Commands

    public ICommand InstallModFromFileCommand { get; }
    public ICommand CheckForUpdatesCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ModProcessor ModProcessor { get; }
    public ModLibrary Library { get; }
    public LoaderViewModel LoaderViewModel { get; }
    
    public ObservableCollection<ModViewModel> Mods { get; }
    public ModViewModel? SelectedMod { get; set; }

    public ModifiedFilesViewModel ModifiedFiles { get; }
    public bool ShowModifiedFilesAsTree
    {
        get => ModifiedFiles.ShowModifiedFilesAsTree;
        set
        {
            ModifiedFiles.ShowModifiedFilesAsTree = value;
            RefreshModifiedFiles();
        }
    }

    public LocalizedString? AddedFilesText { get; set; }
    public LocalizedString? RemovedFilesText { get; set; }
    public LocalizedString? PatchedFilesText { get; set; }
    public LocalizedString? ChangedModsText { get; set; }

    public bool HasChanges { get; set; }
    public bool HasReorderedMods { get; set; }

    public DownloadableModsViewModel DownloadableMods { get; }

    #endregion

    #region Private Methods

    private async Task MigrateLegacyPatchesAsync()
    {
        LegacyPatchMigrationManager manager = new(GameInstallation, Library);

        if (!manager.CanMigrate())
            return;

        try
        {
            using (LoaderLoadState state = await LoaderViewModel.RunAsync(Resources.ModLoader_MigratingPatchesStatus))
            {
                await Task.Run(async () => await manager.MigrateAsync(state.SetProgress));
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Migrating legacy patches");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_MigratingPatchesError);
        }
    }

    private async Task<bool> LoadInstalledModsAsync()
    {
        Logger.Info("Loading installed mods");

        try
        {
            // Clear any previously loaded mods
            Mods.DisposeAll();
            Mods.Clear();

            ModManifest modManifest = Library.ReadModManifest();

            foreach (ModManifestEntry modEntry in modManifest.Mods.Values)
            {
                Mod mod;

                try
                {
                    // Read the mod
                    mod = Library.ReadInstalledMod(modEntry.Id, GameInstallation);
                }
                catch (UnsupportedModFormatException ex)
                {
                    Logger.Warn(ex, "Reading installed mod");

                    await Services.MessageUI.DisplayMessageAsync(Resources.ModLoader_ReadingModVersionError, MessageType.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Reading installed mod");

                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_ReadingModError);
                    return false;
                }

                // Create and add view model
                ModViewModel vm = new(this, LoaderViewModel, DownloadableModsSource.GetSource(modEntry.InstallInfo), mod, modEntry);
                Mods.Add(vm);

                // Load thumbnail
                vm.LoadThumbnail();

                Logger.Info("Added installed mod '{0}' from library with version {1} and ID {2}", vm.Metadata.Name, vm.Metadata.Version, vm.Metadata.Id);
            }

            Logger.Info("Loaded {0} installed mods", modManifest.Mods.Count);

            RefreshModifiedFiles();

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading installed mods");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_ReadingModsError);

            return false;
        }
    }

    private async Task<bool> VerifyModSecurityAsync(Mod mod)
    {
        // Check if the patch adds or replaces exe or dll files. Expand to check other file types too?
        bool hasCodeFiles = mod.GetAddedFiles().
            Select(x => x.Path.FilePath).
            Concat(mod.GetPatchedFiles().Select(x => x.Path.FilePath)).
            Any(x => x.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) ||
                     x.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase));

        // Have the user verify
        if (hasCodeFiles)
        {
            Logger.Info("Mod with id {0} contains one or more potentially harmful files", mod.Metadata.Id);

            return await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.ModLoader_SecurityWarning, mod.Metadata.Name), MessageType.Question, true);
        }
        else
        {
            return true;
        }
    }

    private async Task AddLocalModsToInstall(IEnumerable<ModToInstall> modFiles)
    {
        Logger.Info("Adding new mods to install");

        using (LoaderLoadState state = await LoaderViewModel.RunAsync())
        {
            state.SetCanCancel(true);

            foreach (ModToInstall modFile in modFiles)
            {
                state.SetStatus(String.Format(Resources.ModLoader_InstallStatus, modFile.FilePath.Name));

                try
                {
                    await AddModToInstallAsync(modFile.FilePath, state, modFile.SourceId, modFile.InstallData);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Info(ex, "Canceled adding mod to install");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Adding mod to install");

                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.ModLoader_InstallError, modFile.FilePath.Name));
                }
            }

            ReportNewChanges();
        }
    }

    private async Task AddModToInstallAsync(FileSystemPath filePath, LoaderLoadState loadState, string? sourceId, object? installData)
    {
        FileExtension fileExtension = filePath.FileExtension;
        ModExtractor modExtractor = _modExtractors.First(x => x.FileExtension == fileExtension);

        TempDirectory extractTempDir = new(true);

        try
        {
            // Extract the mod
            await Task.Run(async () =>
                await modExtractor.ExtractAsync(filePath, extractTempDir.TempPath, loadState.SetProgress, loadState.CancellationToken));

            // Read the mod
            Mod extractedMod = new(extractTempDir.TempPath, GameInstallation);

            // Verify game
            if (!extractedMod.Metadata.IsGameValid(GameInstallation.GameDescriptor))
            {
                Logger.Warn("Failed to add mod due to the current game {0} not being supported", GameInstallation.FullId);

                IEnumerable<LocalizedString> gameTargets = extractedMod.Metadata.Games.Select(x => Services.Games.GetGameDescriptor(x).DisplayName);

                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.ModLoader_InstallInvalidTargetError,
                    extractedMod.Metadata.Name, String.Join(Environment.NewLine, gameTargets)), MessageType.Error);

                extractTempDir.Dispose();

                return;
            }

            // Verify the security
            if (!await VerifyModSecurityAsync(extractedMod))
            {
                extractTempDir.Dispose();
                return;
            }

            string id = extractedMod.Metadata.Id;
            long size = extractTempDir.TempPath.GetSize();
            ModInstallInfo installInfo = new(
                Source: sourceId, 
                Version: extractedMod.Metadata.Version, 
                Size: size, 
                Date: DateTime.Now, 
                Data: installData == null ? null : JObject.FromObject(installData));

            int existingModIndex = Mods.FindItemIndex(x => x.Metadata.Id == id);

            ModViewModel viewModel;

            // The mod is being added as a new mod
            if (existingModIndex == -1)
            {
                ModManifestEntry modEntry = new(id, installInfo, true);
                viewModel = new ModViewModel(this, LoaderViewModel, DownloadableModsSource.GetSource(modEntry.InstallInfo), extractedMod, modEntry, extractTempDir);

                Mods.Add(viewModel);
                viewModel.LoadThumbnail();
            }
            // The mod is being added as an update
            else
            {
                ModViewModel existingMod = Mods[existingModIndex];

                ModManifestEntry modEntry = new(id, installInfo, existingMod.IsEnabled);
                viewModel = new ModViewModel(this, LoaderViewModel, DownloadableModsSource.GetSource(modEntry.InstallInfo), extractedMod, modEntry, extractTempDir);

                existingMod.Dispose();
                Mods[existingModIndex] = viewModel;
                viewModel.LoadThumbnail();
            }

            Services.Messenger.Send(new ModInstalledMessage(viewModel), 
                // TODO: For now we use the installation id as the token to keep this scoped to the current
                //       dialog instance. In the future we might want to set up a system where each open
                //       dialog instance has an id which we can then use as a token.
                GameInstallation.InstallationId);

            Logger.Info("Added new mod to install with ID {0}", extractedMod.Metadata.Id);
        }
        catch
        {
            extractTempDir.Dispose();
            throw;
        }
    }

    private long GetLargestArchiveSize()
    {
        HashSet<string> checkedArchives = new();
        long largestSize = 0;

        // Check history first since these will be reverted
        LibraryFileHistory? history = Library.ReadHistory();
        if (history != null)
        {
            foreach (ModFilePath addedFile in history.AddedFiles)
                checkArchiveSize(addedFile);

            foreach (ModFilePath removedFile in history.RemovedFiles)
                checkArchiveSize(removedFile);

            foreach (ModFilePath replacedFile in history.ReplacedFiles)
                checkArchiveSize(replacedFile);
        }

        // Then check enabled mods
        foreach (ModViewModel modViewModel in Mods.Where(x => x.IsEnabled))
        {
            Mod mod = modViewModel.Mod;

            foreach (IModFileResource addedFile in mod.GetAddedFiles())
                checkArchiveSize(addedFile.Path);

            foreach (ModFilePath removedFile in mod.GetRemovedFiles())
                checkArchiveSize(removedFile);

            foreach (IFilePatch patchedFile in mod.GetPatchedFiles())
                checkArchiveSize(patchedFile.Path);
        }

        return largestSize;

        void checkArchiveSize(ModFilePath filePath)
        {
            if (!filePath.HasLocation) 
                return;
            
            if (!checkedArchives.Add(filePath.Location))
                return;

            FileSystemPath archiveFilePath = GameInstallation.InstallLocation.Directory + filePath.Location;

            if (archiveFilePath.FileExists)
            {
                long fileSize = archiveFilePath.GetSize();

                if (fileSize > largestSize)
                    largestSize = fileSize;
            }
        }
    }

    #endregion

    #region Public Static Methods

    public static async Task<ModLoaderViewModel?> FromFilesAsync(ModToInstall[] modFilePaths)
    {
        if (modFilePaths.Length == 0)
            throw new ArgumentException("There has to be a least one mod file provided", nameof(modFilePaths));

        // Use the first file to determine which games are being targeted
        ModToInstall firstFile = modFilePaths[0];
        FileExtension firstFileExtension = firstFile.FilePath.FileExtension;
        ModExtractor? modExtractor = ModExtractor.GetModExtractors().FirstOrDefault(x => x.FileExtension == firstFileExtension);

        if (modExtractor == null)
            throw new Exception("One or more mod files are not valid");

        string[] gameTargets = modExtractor.GetGameTargets(firstFile.FilePath);

        // Get all the installations which the patch supports
        List<GameInstallation> gameInstallations = Services.Games.GetInstalledGames().
            Where(x => gameTargets.Contains(x.GameDescriptor.GameId)).
            ToList();

        // Make sure there is an installed game which can be patched
        if (!gameInstallations.Any())
        {
            string gameTargetNames = String.Join(Environment.NewLine, gameTargets.Select(x =>
                Services.Games.TryGetGameDescriptor(x, out GameDescriptor? g) ? g.DisplayName.Value : x));
            await Services.MessageUI.DisplayMessageAsync(String.Format("Can't open the mod due to none of the following targeted games having been added:\r\n\r\n{0}", gameTargetNames), MessageType.Error);
            return null;
        }

        GameInstallation gameInstallation;

        // If there is more than 1 matching game we ask the user which one to patch
        if (gameInstallations.Count > 1)
        {
            GamesSelectionResult result = await Services.UI.SelectGamesAsync(new GamesSelectionViewModel(gameInstallations)
            {
                Title = Resources.ModLoader_SelectInstallTargetTitle
            });

            if (result.CanceledByUser)
                return null;

            gameInstallation = result.SelectedGame;
        }
        else
        {
            gameInstallation = gameInstallations.First();
        }

        return new ModLoaderViewModel(gameInstallation, modFilePaths);
    }

    #endregion

    #region Public Methods

    public async Task<bool> InitializeAsync()
    {
        // Reset properties
        HasChanges = false;
        HasReorderedMods = false;
        ChangedModsText = null;
        SelectedMod = null;

        // Make sure we have write access
        if (!Services.File.CheckDirectoryWriteAccess(GameInstallation.InstallLocation.Directory))
        {
            Logger.Info("User does not have write access to game installation");
            await Services.MessageUI.DisplayMessageAsync(Resources.ModLoader_WriteAccessDenied, MessageType.Error);
            return false;
        }

        try
        {
            Library.VerifyLibrary();
        }
        catch (UnsupportedModLibraryFormatException ex)
        {
            Logger.Warn(ex, "Verifying library");
            await Services.MessageUI.DisplayMessageAsync(Resources.ModLoader_ReadLibraryVersionError, MessageType.Error);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Verifying library");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_ReadLibraryError);

            return false;
        }

        // Migrate legacy patches first
        await MigrateLegacyPatchesAsync();

        // Load mods
        bool success = await LoadInstalledModsAsync();

        if (!success)
            return false;

        // Add pending mods
        if (_pendingModFiles != null)
        {
            await AddLocalModsToInstall(_pendingModFiles);
            _pendingModFiles = null;
        }

        // Check for mod updates if set to do so
        if (Services.Data.ModLoader_AutomaticallyCheckForUpdates)
            CheckForUpdatesCommand.Execute(null);

        // Load downloadable mods
        await DownloadableMods.LoadModsAsync();

        Logger.Info("Finished initializing mod loader");

        return true;
    }

    public void RefreshModifiedFiles()
    {
        Logger.Trace("Refreshing modified files");

        ModifiedFiles.Refresh(Mods.Where(x => x.IsEnabled));

        AddedFilesText = ModifiedFiles.AddedFilesCount == 0 
            ? null 
            : new ResourceLocString(nameof(Resources.ModLoader_AddedFilesInfo), ModifiedFiles.AddedFilesCount);
        RemovedFilesText = ModifiedFiles.RemovedFilesCount == 0
            ? null
            : new ResourceLocString(nameof(Resources.ModLoader_RemovedFilesInfo), ModifiedFiles.RemovedFilesCount);
        PatchedFilesText = ModifiedFiles.PatchedFilesCount == 0
            ? null
            : new ResourceLocString(nameof(Resources.ModLoader_PatchedFilesInfo), ModifiedFiles.PatchedFilesCount);
    }

    public void ReportNewChanges()
    {
        Logger.Trace("New mod loader changes have been reported");

        RefreshModifiedFiles();

        int changedMods = Mods.Count(x => x.HasChanges);

        HasChanges = changedMods > 0 || HasReorderedMods;

        if (changedMods > 0)
            ChangedModsText = new ResourceLocString(nameof(Resources.ModLoader_UnsavedChangesInfo), changedMods);
        else if (HasReorderedMods)
            ChangedModsText = new ResourceLocString(nameof(Resources.ModLoader_UnsavedOrderChangesInfo));
        else
            ChangedModsText = null;
    }

    public async Task InstallModFromFileAsync()
    {
        FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = Resources.ModLoader_InstallSelectionTitle,
            ExtensionFilter = new FileFilterItem(_modExtractors.Select(x => x.FileExtension), Resources.ModLoader_ModFileTypeFilterName).StringRepresentation,
            MultiSelection = true
        });

        if (result.CanceledByUser)
            return;

        Logger.Info("Adding {0} mods to be installed", result.SelectedFiles.Length);
        await AddLocalModsToInstall(result.SelectedFiles.Select(x => new ModToInstall(x, null, null)));

        Logger.Info("Added mods");
    }

    public async Task InstallModFromDownloadableFileAsync(
        DownloadableModsSource source, 
        string fileName, 
        string downloadUrl, 
        long fileSize, 
        object? installData)
    {
        try
        {
            using (LoaderLoadState state = await LoaderViewModel.RunAsync(String.Format(Resources.ModLoader_DownloadingModStatus, fileName), true))
            {
                Logger.Info("Downloading mod to install from {0}", downloadUrl);

                // Create a temp file to download to
                using TempFile tempFile = new(false, new FileExtension(fileName));

                // Open a stream to the downloadable file
                using (Stream httpStream = await _httpClient.GetStreamAsync(downloadUrl))
                {
                    // Download to the temp file
                    using FileStream tempFileStream = File.Create(tempFile.TempPath);
                    await httpStream.CopyToExAsync(tempFileStream, progressCallback: state.SetProgress, cancellationToken: state.CancellationToken, length: fileSize);
                }

                state.SetStatus(String.Format(Resources.ModLoader_ExtractingDownloadedModStatus, fileName));

                await AddModToInstallAsync(tempFile.TempPath, state, source.Id, installData);
                ReportNewChanges();
            }
        }
        catch (OperationCanceledException ex)
        {
            Logger.Info(ex, "Canceled downloading mod");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Downloading mod");
            
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_DownloadModError﻿);
        }
    }

    public async Task CheckForUpdatesAsync()
    {
        Logger.Info("Checking all mods for updates");
        await Task.WhenAll(Mods.Select(x => x.CheckForUpdateAsync(_httpClient)));
        Logger.Info("Finished checking for updates");
    }

    public async Task<bool?> ApplyAsync()
    {
        // Warn about file conflicts
        if (ModifiedFiles.HasConflicts && Services.Data.ModLoader_ShowModConflictsWarning)
        {
            bool result = await Services.MessageUI.DisplayMessageAsync(Resources.ModLoader_FileConflictsWarning, Resources.ModLoader_FileConflictsWarningHeader, MessageType.Warning, true);

            if (!result)
                return null;
        }

        // Verify the game is not running if Win32
        if (GameInstallation.GameDescriptor is { Platform: GamePlatform.Win32, Structure: DirectoryProgramInstallationStructure structure })
        {
            FileSystemPath exeFilePath = structure.FileSystem.GetAbsolutePath(GameInstallation, ProgramPathType.PrimaryExe);

            if (exeFilePath.FileExists)
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(exeFilePath.RemoveFileExtension().Name);
                    if (processes.Any(x => x.MainModule?.FileName == exeFilePath))
                    {
                        await Services.MessageUI.DisplayMessageAsync(Resources.ModLoader_GameRunningError, Resources.ModLoader_GameRunningErrorHeader, MessageType.Error);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Checking if game is running");
                }
            }
        }

        // Verify the user has enough space left
        try
        {
            // Get the largest size for an archive to repack
            long minBytes = GetLargestArchiveSize();

            // Add at least 1 GB for safety
            minBytes += 0x40000000;

            // Get the drive which has the user's temp folder
            DriveInfo driveInfo = new(new DirectoryInfo(Path.GetTempPath()).Root.Name);

            Logger.Info("Mod loader requires {0} bytes to apply mods", minBytes);

            // Check the available space
            if (driveInfo.AvailableFreeSpace < minBytes)
            {
                await Services.MessageUI.DisplayMessageAsync(
                    String.Format(Resources.ModLoader_InsufficientSpaceError, BinaryHelpers.BytesToString(minBytes)), 
                    Resources.ModLoader_InsufficientSpaceErrorHeader, 
                    MessageType.Error);
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Checking available space on drive");
        }

        using (LoaderLoadState state = await LoaderViewModel.RunAsync(Resources.ModLoader_ApplyStatus))
        {
            Logger.Info("Applying mods");

            try
            {
                await Task.Run(async () =>
                {
                    Dictionary<string, ModManifestEntry> installedMods = new();
                    
                    // Process every mod
                    foreach (ModViewModel modViewModel in Mods)
                    {
                        string id = modViewModel.Metadata.Id;

                        switch (modViewModel.InstallState)
                        {
                            // Already installed mod
                            case ModViewModel.ModInstallState.Installed:
                                installedMods.Add(id, new ModManifestEntry(id, modViewModel.InstallInfo, modViewModel.IsEnabled));
                                break;
                            
                            // Install new mod
                            case ModViewModel.ModInstallState.PendingInstall:
                                Library.InstallMod(modViewModel.Mod.ModDirectoryPath, id, false);
                                installedMods.Add(id, new ModManifestEntry(id, modViewModel.InstallInfo, modViewModel.IsEnabled));
                                break;
                            
                            // Uninstall
                            case ModViewModel.ModInstallState.PendingUninstall:
                                Library.UninstallMod(modViewModel.Metadata.Id);
                                break;

                            default:
                                throw new InvalidOperationException($"Mod state {modViewModel.InstallState} is invalid");
                        }
                    }

                    // Update the mod manifest
                    Library.WriteModManifest(new ModManifest(installedMods));

                    // Apply the mods
                    ApplyModsResult result = await ModProcessor.ApplyAsync(Library, state.SetProgress);

                    Services.Messenger.Send(new ModifiedGameModsMessage(GameInstallation));

                    Logger.Info("Applied mods");

                    if (result.Success)
                        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.ModLoader_ApplySuccess);
                    else
                        await Services.MessageUI.DisplayMessageAsync(
                            String.Format(Resources.ModLoader_ApplyWithErrors, result.ErrorMessage), 
                            MessageType.Warning);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying mods");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_ApplyError);
            }
        }

        // No matter if it succeeds or fails we want to reset the state
        return await InitializeAsync();
    }

    public void Dispose()
    {
        Mods.DisposeAll();
        _httpClient.Dispose();
        DownloadableMods.Dispose();
    }

    #endregion

    #region Records

    public record ModToInstall(FileSystemPath FilePath, string? SourceId, object? InstallData);

    #endregion
}