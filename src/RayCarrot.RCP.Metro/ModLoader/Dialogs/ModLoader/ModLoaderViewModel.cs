using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Extractors;
using RayCarrot.RCP.Metro.ModLoader.Library;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModLoaderViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ModLoaderViewModel(GameInstallation gameInstallation)
    {
        if (!gameInstallation.GameDescriptor.SupportsMods)
            throw new InvalidOperationException("The game installation doesn't support mods");

        GameInstallation = gameInstallation;

        LoaderViewModel = new LoaderViewModel();
        Mods = new ObservableCollection<ModViewModel>();

        ModProcessor = new ModProcessor(GameInstallation);
        Library = new ModLibrary(GameInstallation);

        ModifiedFiles = new ModifiedFilesViewModel(GameInstallation);

        InstallModFromFileCommand = new AsyncRelayCommand(InstallModFromFileAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand InstallModFromFileCommand { get; }

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

    public bool HasChanges { get; set; }

    #endregion

    #region Private Methods

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
                    mod = Library.ReadInstalledMod(modEntry.Id);
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
                ModViewModel vm = new(this, LoaderViewModel, mod, modEntry);
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
        bool hasCodeFiles = mod.Versions.Any(version => mod.GetAddedFiles(version).Any(file =>
            file.Path.FilePath.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) ||
            file.Path.FilePath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)));

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

        // Load mods
        return await LoadInstalledModsAsync();
    }

    public void RefreshModifiedFiles()
    {
        // TODO-UPDATE: Log
        ModifiedFiles.Refresh(Mods.Where(x => x.IsEnabled));
    }

    public void OnReorderedMods()
    {
        RefreshModifiedFiles();
        HasChanges = true;
    }

    public async Task InstallModFromFileAsync()
    {
        ModExtractor[] modExtractors = 
        {
            new ZipModExtractor(), // .zip
            new SevenZipModExtractor(), // .7z
            new RarModExtractor(), // .rar
            new LegacyGamePatchModExtractor(), // .gp (legacy)
        };

        FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-UPDATE: Localize
            Title = "Select mods to install",
            // TODO-UPDATE: Localize
            ExtensionFilter = new FileFilterItem(modExtractors.Select(x => x.FileExtension), "Mod archives").StringRepresentation,
            MultiSelection = true
        });

        if (result.CanceledByUser)
            return;

        Logger.Info("Adding {0} patches to be installed", result.SelectedFiles.Length);
        using (LoadState state = await LoaderViewModel.RunAsync())
        {
            state.SetCanCancel(true);

            foreach (FileSystemPath selectedFile in result.SelectedFiles)
            {
                // TODO-UPDATE: Localize
                state.SetStatus($"Extracting mod {selectedFile.Name}");

                FileExtension fileExtension = selectedFile.FileExtension;
                ModExtractor modExtractor = modExtractors.First(x => x.FileExtension == fileExtension);

                TempDirectory extractTempDir = new(true);

                try
                {
                    // Extract the mod
                    await Task.Run(async () =>
                        await modExtractor.ExtractAsync(selectedFile, extractTempDir.TempPath, state.SetProgress, state.CancellationToken));

                    // Read the mod
                    Mod extractedMod = new(extractTempDir.TempPath);

                    // Verify game
                    if (!extractedMod.Metadata.IsGameValid(GameInstallation.GameDescriptor))
                    {
                        Logger.Warn("Failed to add mod due to the current game {0} not being supported", GameInstallation.FullId);

                        IEnumerable<LocalizedString> gameTargets = extractedMod.Metadata.Games.Select(x => Services.Games.GetGameDescriptor(x).DisplayName);
                        // TODO-UPDATE: Localize
                        await Services.MessageUI.DisplayMessageAsync(String.Format("The mod {0} can't be installed to this game due to it not being one of the game targets:\r\n\r\n{1}",
                            extractedMod.Metadata.Name, String.Join(Environment.NewLine, gameTargets)), MessageType.Error);

                        extractTempDir.Dispose();

                        continue;
                    }

                    // Verify the security
                    if (!await VerifyPatchSecurityAsync(extractedMod))
                    {
                        extractTempDir.Dispose();
                        continue;
                    }

                    string id = extractedMod.Metadata.Id;
                    long size = (long)extractTempDir.TempPath.GetSize().Bytes;

                    int existingModIndex = Mods.FindItemIndex(x => x.Metadata.Id == id);

                    // The mod is being added as a new mod
                    if (existingModIndex == -1)
                    {
                        ModManifestEntry modEntry = new(id, size, true, null);
                        ModViewModel viewModel = new(this, LoaderViewModel, extractedMod, modEntry, extractTempDir);

                        Mods.Add(viewModel);
                    }
                    // The mod is being added as an update
                    else
                    {
                        ModViewModel existingMod = Mods[existingModIndex];

                        ModManifestEntry modEntry = new(id, size, existingMod.IsEnabled, existingMod.Version);
                        ModViewModel viewModel = new(this, LoaderViewModel, extractedMod, modEntry, extractTempDir);

                        existingMod.Dispose();
                        Mods[existingModIndex] = viewModel;
                    }

                    HasChanges = true;
                }
                catch (OperationCanceledException ex)
                {
                    // TODO-UPDATE: Log
                    extractTempDir.Dispose();

                    break;
                }
                catch (Exception ex)
                {
                    // TODO-UPDATE: Log and show error message
                    extractTempDir.Dispose();
                }
            }

            RefreshModifiedFiles();
        }

        Logger.Info("Added mods");
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

                        switch (modViewModel.State)
                        {
                            // Already installed mod
                            case ModViewModel.InstallState.Installed:
                                installedMods.Add(id, new ModManifestEntry(id, modViewModel.Size, modViewModel.IsEnabled, modViewModel.Version));
                                break;
                            
                            // Install new mod
                            case ModViewModel.InstallState.PendingInstall:
                                Library.InstallMod(modViewModel.Mod.ModDirectoryPath, id, false);
                                installedMods.Add(id, new ModManifestEntry(id, modViewModel.Size, modViewModel.IsEnabled, modViewModel.Version));
                                break;
                            
                            // Uninstall
                            case ModViewModel.InstallState.PendingUninstall:
                                Library.UninstallMod(modViewModel.Metadata.Id);
                                break;

                            default:
                                throw new InvalidOperationException($"Mod state {modViewModel.State} is invalid");
                        }
                    }

                    // Update the mod manifest
                    Library.WriteModManifest(new ModManifest(installedMods));

                    // Apply the mods
                    bool success = await ModProcessor.ApplyAsync(Library, state.SetProgress);

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

            // No matter if it succeeds or fails we want to reset the state
            return await InitializeAsync();
        }
    }

    public void Dispose()
    {
        Mods.DisposeAll();
    }

    #endregion
}