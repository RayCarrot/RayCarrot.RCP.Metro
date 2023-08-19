using System.Windows.Input;
using RayCarrot.RCP.Metro.Patcher.Library;

namespace RayCarrot.RCP.Metro.Patcher.Dialogs.Patcher;

public class PatcherViewModel : BaseViewModel
{
    #region Constructor

    public PatcherViewModel(GameInstallation gameInstallation)
    {
        if (!gameInstallation.GameDescriptor.AllowPatching)
            throw new InvalidOperationException("The game installation doesn't allow patching");

        GameInstallation = gameInstallation;

        LoaderViewModel = new LoaderViewModel();
        InstalledPatches = new ObservableCollection<InstalledPatchViewModel>();

        PatchProcessor = new PatchProcessor(GameInstallation);
        Library = new PatchLibrary(GameInstallation);

        InstallPatchFromFileCommand = new AsyncRelayCommand(InstallPatchFromFileAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand InstallPatchFromFileCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public PatchProcessor PatchProcessor { get; }
    public PatchLibrary Library { get; }
    public LoaderViewModel LoaderViewModel { get; }
    
    public ObservableCollection<InstalledPatchViewModel> InstalledPatches { get; }
    public InstalledPatchViewModel? SelectedInstalledPatch { get; set; }

    public bool HasChanges { get; set; }

    #endregion

    #region Private Methods

    private async Task<bool> LoadInstalledPatchesAsync()
    {
        Logger.Info("Loading installed patches");

        try
        {
            // Clear any previously loaded patches
            InstalledPatches.Clear();

            PatchManifest patchManifest = Library.ReadPatchManifest();

            foreach (PatchManifestEntry patchEntry in patchManifest.Patches.Values)
            {
                InstalledPatch patch;

                try
                {
                    // Read the patch
                    patch = Library.ReadInstalledPatch(patchEntry.Id);
                }
                catch (UnsupportedPatchFormatException ex)
                {
                    Logger.Warn(ex, "Adding patch");

                    await Services.MessageUI.DisplayMessageAsync(Resources.Patcher_ReadPatchNewerVersionError, MessageType.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Adding patch");

                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ReadPatchGenericError);
                    return false;
                }

                // Create and add view model
                InstalledPatchViewModel vm = new(this, patch, patchEntry);
                InstalledPatches.Add(vm);

                // Load thumbnail
                vm.LoadThumbnail();

                Logger.Info("Added patch '{0}' from library with version {1} and ID {2}", vm.Metadata.Name, vm.Metadata.Version, vm.Metadata.Id);
            }

            Logger.Info("Loaded {0} patches", patchManifest.Patches.Count);

            RefreshPatchedFiles();

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading installed patches");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_LoadError);

            return false;
        }
    }

    #endregion

    #region Public Methods

    public async Task<bool> InitializeAsync()
    {
        // Reset properties
        HasChanges = false;
        SelectedInstalledPatch = null;

        try
        {
            Library.VerifyLibrary();
        }
        catch (UnsupportedPatchLibraryFormatException ex)
        {
            Logger.Warn(ex, "Verifying library");
            await Services.MessageUI.DisplayMessageAsync(Resources.Patcher_ReadLibraryNewerVersionError, MessageType.Error);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Verifying library");
            // TODO-UPDATE: Error message
            return false;
        }

        // Load patches
        return await LoadInstalledPatchesAsync();
    }

    public void RefreshPatchedFiles()
    {
        // TODO-UPDATE: Implement
    }

    public void OnReorderedPatches()
    {
        RefreshPatchedFiles();
        HasChanges = true;
    }

    public async Task InstallPatchFromFileAsync()
    {
        throw new NotImplementedException();
    }

    public async Task ExtractPatchContentsAsync(InstalledPatchViewModel patchViewModel)
    {
        using (await LoaderViewModel.RunAsync(Resources.Patcher_ExtractContents_Status))
        {
            DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (result.CanceledByUser)
                return;

            try
            {
                Logger.Info("Extracting patch contents");

                // TODO-UPDATE: Implement. Copy folder?

                Logger.Info("Extracted patch contents");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Extracting patch contents");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ExtractContentsError);
                return;
            }
        }

        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Patcher_ExtractContentsSuccess);
    }

    public async Task UninstallPatchAsync(InstalledPatchViewModel patchViewModel)
    {
        throw new NotImplementedException();
        Logger.Info("Uninstalled patch '{0}' with version {1} and ID {2}", patchViewModel.Name, patchViewModel.Metadata.Version, patchViewModel.Metadata.Id);
    }

    public async Task<bool> ApplyAsync()
    {
        using (LoadState state = await LoaderViewModel.RunAsync(Resources.Patcher_Apply_Status))
        {
            Logger.Info("Applying patches");

            try
            {
                await Task.Run(async () =>
                {
                    // TODO-UPDATE: Update library
                    
                    bool success = await PatchProcessor.ApplyAsync(Library, state.SetProgress);

                    Services.Messenger.Send(new ModifiedGamePatchesMessage(GameInstallation));

                    Logger.Info("Applied patches");

                    if (success)
                        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Patcher_ApplySuccess);
                    else
                        await Services.MessageUI.DisplayMessageAsync(Resources.Patcher_ApplySuccessWithErrors, MessageType.Warning);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying patches");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ApplyError);
            }

            // No matter if it succeeds or fails we want to reset the state
            return await InitializeAsync();
        }
    }

    #endregion
}