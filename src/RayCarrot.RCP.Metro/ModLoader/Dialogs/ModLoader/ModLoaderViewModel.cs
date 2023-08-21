using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Library;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModLoaderViewModel : BaseViewModel
{
    #region Constructor

    public ModLoaderViewModel(GameInstallation gameInstallation)
    {
        if (!gameInstallation.GameDescriptor.SupportsMods)
            throw new InvalidOperationException("The game installation doesn't support mods");

        GameInstallation = gameInstallation;

        LoaderViewModel = new LoaderViewModel();
        Mods = new ObservableCollection<InstalledModViewModel>();

        ModProcessor = new ModProcessor(GameInstallation);
        Library = new ModLibrary(GameInstallation);

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
    
    public ObservableCollection<InstalledModViewModel> Mods { get; }
    public InstalledModViewModel? SelectedMod { get; set; }

    public bool HasChanges { get; set; }

    #endregion

    #region Private Methods

    private async Task<bool> LoadInstalledModsAsync()
    {
        Logger.Info("Loading installed mods");

        try
        {
            // Clear any previously loaded mods
            Mods.Clear();

            ModManifest modManifest = Library.ReadModManifest();

            foreach (ModManifestEntry modEntry in modManifest.Mods.Values)
            {
                InstalledMod mod;

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
                InstalledModViewModel vm = new(this, LoaderViewModel, mod, modEntry);
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
        // TODO-UPDATE: Implement
    }

    public void OnReorderedMods()
    {
        RefreshModifiedFiles();
        HasChanges = true;
    }

    public async Task InstallModFromFileAsync()
    {
        throw new NotImplementedException();
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
                    foreach (InstalledModViewModel modViewModel in Mods)
                    {
                        string id = modViewModel.Metadata.Id;

                        switch (modViewModel.State)
                        {
                            // Already installed mod
                            case InstalledModViewModel.InstallState.Installed:
                                installedMods.Add(id, new ModManifestEntry(id, modViewModel.Size, modViewModel.IsEnabled, modViewModel.Version));
                                break;
                            
                            // Install new mod
                            case InstalledModViewModel.InstallState.PendingInstall:
                                Library.InstallMod(modViewModel.InstalledMod.ModDirectoryPath, id, false);
                                installedMods.Add(id, new ModManifestEntry(id, modViewModel.Size, modViewModel.IsEnabled, modViewModel.Version));
                                break;
                            
                            // Uninstall
                            case InstalledModViewModel.InstallState.PendingUninstall:
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

    #endregion
}