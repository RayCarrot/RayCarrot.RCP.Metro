﻿using System.IO;
using System.Net.Http;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Extractors;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Modules;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModLoaderViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ModLoaderViewModel(GameInstallation gameInstallation, FileSystemPath[]? pendingModFiles = null)
    {
        GameInstallation = gameInstallation;
        _pendingModFiles = pendingModFiles;

        // Get the available mod modules for this game
        _modModules = gameInstallation.GetComponents<ModModuleComponent>().CreateObjects().ToArray();

        if (_modModules.Length == 0)
            throw new InvalidOperationException("The game installation doesn't support mods");

        _httpClient = new HttpClient(); // TODO-UPDATE: Share a single client throughout the app?
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

    private FileSystemPath[]? _pendingModFiles;
    private readonly ModModule[] _modModules;
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
    public LocalizedString? ChangedModsText { get; set; }

    public bool HasChanges { get; set; }

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
            // TODO-UPDATE: Localize
            using (LoadState state = await LoaderViewModel.RunAsync("Migrating patches to mods"))
            {
                await Task.Run(async () => await manager.MigrateAsync(state.SetProgress));
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Migrating legacy patches");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when migrating patches to mods");
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
                    mod = Library.ReadInstalledMod(modEntry.Id, _modModules);
                }
                catch (UnsupportedModFormatException ex)
                {
                    Logger.Warn(ex, "Adding mod");

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync("The selected mod was made with a newer version of the Rayman Control Panel and can not be read", MessageType.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Adding mod");

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, "The selected mod could not be read");
                    return false;
                }

                // Create and add view model
                ModViewModel vm = new(this, LoaderViewModel, DownloadableModsSource.GetSource(modEntry.InstallInfo), mod, modEntry);
                Mods.Add(vm);

                // Load thumbnail
                vm.LoadThumbnail();

                Logger.Info("Added mod '{0}' from library with version {1} and ID {2}", vm.Metadata.Name, vm.Metadata.Version, vm.Metadata.Id);
            }

            Logger.Info("Loaded {0} mods", modManifest.Mods.Count);

            RefreshModifiedFiles();

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading installed mods");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when loading the mods");

            return false;
        }
    }

    private async Task<bool> VerifyPatchSecurityAsync(Mod mod)
    {
        // Check if the patch adds or replaces exe or dll files. Expand to check other file types too?
        bool hasCodeFiles = mod.GetAddedFiles().Any(file =>
            file.Path.FilePath.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) ||
            file.Path.FilePath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase));

        // Have the user verify
        if (hasCodeFiles)
        {
            Logger.Info("Mod with id {0} contains one or more potentially harmful files", mod.Metadata.Id);

            // TODO-UPDATE: Localize
            return await Services.MessageUI.DisplayMessageAsync(String.Format("The mod {0} adds or replaces sensitive files, such as executable files, in the game. Only install this mod if you trust the author. Continue?", mod.Metadata.Name),
                MessageType.Question, true);
        }
        else
        {
            return true;
        }
    }

    private async Task AddLocalModsToInstall(FileSystemPath[] filePaths)
    {
        using (LoadState state = await LoaderViewModel.RunAsync())
        {
            state.SetCanCancel(true);

            foreach (FileSystemPath modFilePath in filePaths)
            {
                // TODO-UPDATE: Localize
                state.SetStatus($"Extracting mod {modFilePath.Name}");

                try
                {
                    await AddModToInstallAsync(modFilePath, state, null, null);
                }
                catch (OperationCanceledException ex)
                {
                    // TODO-UPDATE: Log
                    break;
                }
                catch (Exception ex)
                {
                    // TODO-UPDATE: Log and show error message
                }
            }

            ReportNewChanges();
        }
    }

    private async Task AddModToInstallAsync(FileSystemPath filePath, LoadState loadState, string? sourceId, object? installData)
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
            Mod extractedMod = new(extractTempDir.TempPath, _modModules);

            // Verify game
            if (!extractedMod.Metadata.IsGameValid(GameInstallation.GameDescriptor))
            {
                Logger.Warn("Failed to add mod due to the current game {0} not being supported", GameInstallation.FullId);

                IEnumerable<LocalizedString> gameTargets = extractedMod.Metadata.Games.Select(x => Services.Games.GetGameDescriptor(x).DisplayName);
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync(String.Format("The mod {0} can't be installed to this game due to it not being one of the game targets:\r\n\r\n{1}",
                    extractedMod.Metadata.Name, String.Join(Environment.NewLine, gameTargets)), MessageType.Error);

                extractTempDir.Dispose();

                return;
            }

            // Verify the security
            if (!await VerifyPatchSecurityAsync(extractedMod))
            {
                extractTempDir.Dispose();
                return;
            }

            string id = extractedMod.Metadata.Id;
            long size = (long)extractTempDir.TempPath.GetSize().Bytes;
            ModInstallInfo installInfo = new(
                Source: sourceId, 
                Version: extractedMod.Metadata.Version, 
                Size: size, 
                Date: DateTime.Now, 
                Data: installData == null ? null : JObject.FromObject(installData));

            int existingModIndex = Mods.FindItemIndex(x => x.Metadata.Id == id);

            // The mod is being added as a new mod
            if (existingModIndex == -1)
            {
                ModManifestEntry modEntry = new(id, installInfo, true);
                ModViewModel viewModel = new(this, LoaderViewModel, DownloadableModsSource.GetSource(modEntry.InstallInfo), extractedMod, modEntry, extractTempDir);

                Mods.Add(viewModel);
                viewModel.LoadThumbnail();
            }
            // The mod is being added as an update
            else
            {
                ModViewModel existingMod = Mods[existingModIndex];

                ModManifestEntry modEntry = new(id, installInfo, existingMod.IsEnabled);
                ModViewModel viewModel = new(this, LoaderViewModel, DownloadableModsSource.GetSource(modEntry.InstallInfo), extractedMod, modEntry, extractTempDir);

                existingMod.Dispose();
                Mods[existingModIndex] = viewModel;
                viewModel.LoadThumbnail();
            }
        }
        catch
        {
            extractTempDir.Dispose();
            throw;
        }
    }

    #endregion

    #region Public Static Methods

    public static async Task<ModLoaderViewModel?> FromFilesAsync(FileSystemPath[] modFilePaths)
    {
        if (modFilePaths.Length == 0)
            throw new ArgumentException("There has to be a least one mod file provided", nameof(modFilePaths));

        // Use the first file to determine which games are being targeted
        FileSystemPath firstFile = modFilePaths[0];
        FileExtension firstFileExtension = firstFile.FileExtension;
        ModExtractor? modExtractor = ModExtractor.GetModExtractors().FirstOrDefault(x => x.FileExtension == firstFileExtension);

        if (modExtractor == null)
            throw new Exception("One or more mod files are not valid");

        string[] gameTargets = modExtractor.GetGameTargets(firstFile);

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
                // TODO-UPDATE: Localize
                Title = "Select game to install the mod to"
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
        SelectedMod = null;

        try
        {
            Library.VerifyLibrary();
        }
        catch (UnsupportedModLibraryFormatException ex)
        {
            // TODO-UPDATE: Localize
            Logger.Warn(ex, "Verifying library");
            await Services.MessageUI.DisplayMessageAsync("The game mod library was made with a newer version of the Rayman Control Panel and can not be read", MessageType.Error);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Verifying library");
            // TODO-UPDATE: Error message
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

        return true;
    }

    public void RefreshModifiedFiles()
    {
        // TODO-UPDATE: Log
        ModifiedFiles.Refresh(Mods.Where(x => x.IsEnabled));

        // TODO-UPDATE: Localize
        AddedFilesText = new ConstLocString($"{ModifiedFiles.AddedFilesCount} added files");
        RemovedFilesText = new ConstLocString($"{ModifiedFiles.RemovedFilesCount} removed files");
    }

    public void ReportNewChanges()
    {
        RefreshModifiedFiles();

        int changedMods = Mods.Count(x => x.HasChanges);

        HasChanges = changedMods > 0;
        // TODO-UPDATE: Localize
        if (HasChanges)
            ChangedModsText = $"{changedMods} unsaved mods";
        else
            ChangedModsText = null;
    }

    public async Task InstallModFromFileAsync()
    {
        FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-UPDATE: Localize
            Title = "Select mods to install",
            // TODO-UPDATE: Localize
            ExtensionFilter = new FileFilterItem(_modExtractors.Select(x => x.FileExtension), "Mod archives").StringRepresentation,
            MultiSelection = true
        });

        if (result.CanceledByUser)
            return;

        Logger.Info("Adding {0} mods to be installed", result.SelectedFiles.Length);
        await AddLocalModsToInstall(result.SelectedFiles);

        Logger.Info("Added mods");
    }

    public async Task InstallModFromDownloadableFileAsync(
        DownloadableModsSource source, 
        string fileName, 
        string downloadUrl, 
        long fileSize, 
        object? installData)
    {
        // TODO-UPDATE: Localize
        // TODO-UPDATE: Try/catch
        using (LoadState state = await LoaderViewModel.RunAsync($"Downloading mod {fileName}", true))
        {
            // Create a temp file to download to
            using TempFile tempFile = new(false, new FileExtension(fileName));

            // Open a stream to the downloadable file
            using (Stream httpStream = await _httpClient.GetStreamAsync(downloadUrl))
            {
                // Download to the temp file
                using FileStream tempFileStream = File.Create(tempFile.TempPath);
                await httpStream.CopyToExAsync(tempFileStream, progressCallback: state.SetProgress, cancellationToken: state.CancellationToken, length: fileSize);
            }

            // TODO-UPDATE: Localize
            state.SetStatus($"Extracting mod {fileName}");

            try
            {
                await AddModToInstallAsync(tempFile.TempPath, state, source.Id, installData);
                ReportNewChanges();

                // TODO-UPDATE: Have some way to indicate it was downloaded. Switch tabs? Show icon on library tab?
            }
            catch (OperationCanceledException ex)
            {
                // TODO-UPDATE: Log
            }
            catch (Exception ex)
            {
                // TODO-UPDATE: Log and show error message
            }
        }
    }

    public async Task CheckForUpdatesAsync()
    {
        await Task.WhenAll(Mods.Select(x => x.CheckForUpdateAsync(_httpClient)));
    }

    public async Task<bool> ApplyAsync()
    {
        // TODO-UPDATE: Localize
        using (LoadState state = await LoaderViewModel.RunAsync("Applying mods"))
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
                    bool success = await ModProcessor.ApplyAsync(Library, _modModules, state.SetProgress);

                    Services.Messenger.Send(new ModifiedGameModsMessage(GameInstallation));

                    Logger.Info("Applied mods");

                    if (success)
                        // TODO-UPDATE: Localize
                        await Services.MessageUI.DisplaySuccessfulActionMessageAsync("Successfully applied all mods");
                    else
                        // TODO-UPDATE: Localize
                        await Services.MessageUI.DisplayMessageAsync("Finished applying mods. Some files could not be modified.", MessageType.Warning);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying mods");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when applying the mods. Not all changes were applied and some data might have been lost. Make sure to not have any files from the game open while applying mods.");
            }
        }

        // No matter if it succeeds or fails we want to reset the state
        return await InitializeAsync();
    }

    public void Dispose()
    {
        Mods.DisposeAll();
        _httpClient.Dispose();
    }

    #endregion
}