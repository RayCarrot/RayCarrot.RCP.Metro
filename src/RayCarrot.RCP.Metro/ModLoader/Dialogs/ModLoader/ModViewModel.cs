using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ModViewModel(
        ModLoaderViewModel modLoaderViewModel, 
        LoaderViewModel loaderViewModel, 
        DownloadableModsSource? downloadableModsSource, 
        Mod mod, 
        ModManifestEntry modEntry,
        ModInstallState installState)
    {
        ModLoaderViewModel = modLoaderViewModel;
        LoaderViewModel = loaderViewModel;
        DownloadableModsSource = downloadableModsSource;
        Mod = mod;
        IsEnabled = modEntry.IsEnabled;
        _wasEnabled = IsEnabled;
        HasChangesToApply = false;
        InstallInfo = modEntry.InstallInfo;

        ModInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_Author)), Metadata.Author),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_Version)), Metadata.Version?.ToString()),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_FormatVersion)), Metadata.Format.ToString(), UserLevel.Technical),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_ID)), Metadata.Id, UserLevel.Technical),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_Size)), BinaryHelpers.BytesToString(modEntry.InstallInfo.Size)),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_InstallSource)), DownloadableModsSource?.DisplayName ?? new ResourceLocString(nameof(Resources.ModLoader_LocalInstallSource))),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_Modules)), Mod.GetSupportedModules().JoinItems(", ", x => x.Id)),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_AddedFiles)), mod.GetAddedFiles().Count.ToString()),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_RemovedFiles)), mod.GetRemovedFiles().Count.ToString()),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_PatchedFiles)), mod.GetPatchedFiles().Count.ToString()),
        };

        ReadOnlyCollection<string> unsupportedModules = Mod.GetUnsupportedModules();
        if (unsupportedModules.Any())
            UnsupportedModulesErrorMessage = new ResourceLocString(nameof(Resources.ModLoader_UnsupportedModulesInfo), unsupportedModules.JoinItems(", "));

        ChangelogEntries = new ObservableCollection<ModChangelogEntry>(Metadata.Changelog ?? Array.Empty<ModChangelogEntry>());

        PanelFooterViewModel = DownloadableModsSource?.GetPanelFooterViewModel(InstallInfo);

        SetInstallState(installState);
        UpdateHasChangedToApply();

        ReportNewChangeCommand = new RelayCommand(ReportNewChange);
        OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
        ExtractContentsCommand = new AsyncRelayCommand(ExtractModContentsAsync);
        UninstallCommand = new RelayCommand(UninstallMod);
        UpdateModCommand = new AsyncRelayCommand(UpdateModAsync);
        OpenWebsiteCommand = new RelayCommand(OpenWebsite);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly bool? _wasEnabled;

    #endregion

    #region Commands

    public ICommand ReportNewChangeCommand { get; }
    public ICommand OpenLocationCommand { get; }
    public ICommand ExtractContentsCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand UpdateModCommand { get; }
    public ICommand OpenWebsiteCommand { get; }

    #endregion

    #region Public Properties

    public ModLoaderViewModel ModLoaderViewModel { get; }
    public LoaderViewModel LoaderViewModel { get; }
    public TempDirectory? PendingInstallTempDir { get; init; }
    public DownloadableModsSource? DownloadableModsSource { get; }
    public Mod Mod { get; }
    public ModMetadata Metadata => Mod.Metadata;
    public ObservableCollection<DuoGridItemViewModel> ModInfo { get; }
    public ObservableCollection<ModChangelogEntry> ChangelogEntries { get; }

    public LocalizedString? UnsupportedModulesErrorMessage { get; }

    public string? Name => Metadata.Name;
    public string? Description => Metadata.Description;
    public string? Website => Metadata.Website;

    public bool HasWebsite => Uri.TryCreate(Metadata.Website, UriKind.Absolute, out _);
    public bool HasDescripton => !Metadata.Description.IsNullOrWhiteSpace();
    public ImageSource? Thumbnail { get; set; }

    public bool IsEnabled { get; set; }
    public ModInstallInfo InstallInfo { get; }

    public ModInstallState InstallState { get; set; }
    public LocalizedString? InstallStateMessage { get; set; }
    public bool CanModify => InstallState != ModInstallState.PendingUninstall;

    public ModUpdateState UpdateState { get; set; }
    public LocalizedString? UpdateStateMessage { get; set; }
    public object? UpdateData { get; set; }

    public bool HasChangesToApply { get; private set; }

    public ModPanelFooterViewModel? PanelFooterViewModel { get; }

    #endregion

    #region Private Methods

    private void SetInstallState(ModInstallState state)
    {
        Logger.Trace("Set install state to {0} for mod with ID {1}", state, Metadata.Id);

        InstallState = state;
        InstallStateMessage = state switch
        {
            ModInstallState.Installed => null,
            ModInstallState.PendingInstall => new ResourceLocString(nameof(Resources.ModLoader_InstallState_PendingInstall)),
            ModInstallState.PendingUninstall => new ResourceLocString(nameof(Resources.ModLoader_InstallState_PendingUninstall)),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        // Disable if pending uninstall
        if (state == ModInstallState.PendingUninstall)
            IsEnabled = false;
    }

    private void SetUpdateState(ModUpdateState state, LocalizedString message, object? updateData = null)
    {
        Logger.Trace("Set update state to {0} for mod with ID {1}", state, Metadata.Id);

        UpdateState = state;
        UpdateStateMessage = message;
        UpdateData = updateData;
    }

    private void UpdateHasChangedToApply()
    {
        HasChangesToApply = InstallState switch
        {
            ModInstallState.Installed => IsEnabled != _wasEnabled,
            ModInstallState.PendingInstall => true,
            ModInstallState.PendingUninstall => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void ReportNewChange()
    {
        UpdateHasChangedToApply();
        ModLoaderViewModel.ReportNewChanges();
    }

    #endregion

    #region Public Methods

    public async Task OpenLocationAsync()
    {
        await Services.File.OpenExplorerLocationAsync(Mod.ModDirectoryPath);
    }

    public async Task ExtractModContentsAsync()
    {
        using (LoaderLoadState state = await LoaderViewModel.RunAsync(Resources.ModLoader_ExtractingModStatus))
        {
            DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (result.CanceledByUser)
                return;

            try
            {
                Logger.Info("Extracting mod contents");

                await Task.Run(() => Services.File.CopyDirectory(Mod.ModDirectoryPath, result.SelectedDirectory, false, true));

                state.Complete();

                Logger.Info("Extracted mod contents");

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.ModLoader_ExtractingModSuccess);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Extracting mod contents");

                state.Error();

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_ExtractingModError);
            }
        }
    }

    public void UninstallMod()
    {
        SetInstallState(ModInstallState.PendingUninstall);
        ReportNewChange();

        Logger.Info("Set mod '{0}' with version {1} and ID {2} to pending uninstall", Name, Metadata.Version, Metadata.Id);
    }

    public async Task UpdateModAsync()
    {
        if (DownloadableModsSource == null)
            return;

        try
        {
            Logger.Info("Updating mod with ID {0}", Metadata.Id);

            ModDownload? download = await DownloadableModsSource.GetModUpdateDownloadAsync(UpdateData);

            if (download == null)
                return;

            await ModLoaderViewModel.InstallModFromDownloadableFileAsync(
                source: DownloadableModsSource,
                fileName: download.FileName,
                downloadUrl: download.DownloadUrl,
                fileSize: download.FileSize,
                installData: download.InstallData);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Updating mod");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_UpdateError);

            return;
        }

        // Mark this mod as being uninstalled. Ideally the new downloaded mod should replace this,
        // but if it happens to have a separate id we want to remove this old mod.
        SetInstallState(ModInstallState.PendingUninstall);
        ReportNewChange();
    }

    public void LoadThumbnail()
    {
        Logger.Trace("Loading thumbnail for mod with ID {0}", Metadata.Id);

        try
        {
            FileSystemPath? thumbFilePath = Mod.GetThumbnailFilePath();

            if (thumbFilePath == null)
            {
                Thumbnail = null;
                return;
            }

            BitmapImage thumb = new();
            thumb.BeginInit();
            thumb.CreateOptions |= BitmapCreateOptions.IgnoreImageCache;
            thumb.CacheOption = BitmapCacheOption.OnLoad; // Required to allow the file to be deleted, such as if a temp file
            thumb.UriSource = new Uri(thumbFilePath);
            thumb.EndInit();

            if (thumb.CanFreeze)
                thumb.Freeze();

            Thumbnail = thumb;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading mod thumbnail");
        }
    }

    public void OpenWebsite()
    {
        if (Metadata.Website != null)
            Services.App.OpenUrl(Metadata.Website);
    }

    public async Task CheckForUpdateAsync(HttpClient httpClient)
    {
        if (InstallState != ModInstallState.Installed)
        {
            SetUpdateState(ModUpdateState.None, String.Empty);
            return;
        }

        if (DownloadableModsSource == null)
        {
            SetUpdateState(ModUpdateState.UnableToCheckForUpdates, new ResourceLocString(nameof(Resources.ModLoader_UpdateState_UnableToCheckLocal)));
            return;
        }

        SetUpdateState(ModUpdateState.CheckingForUpdates, new ResourceLocString(nameof(Resources.ModLoader_UpdateState_Checking)));

        try
        {
            ModUpdateCheckResult result = await DownloadableModsSource.CheckForUpdateAsync(httpClient, InstallInfo);
            SetUpdateState(result.State, result.Message ?? String.Empty, result.UpdateData);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking for mod update");

            SetUpdateState(ModUpdateState.ErrorCheckingForUpdates, ex.Message);
        }
    }

    public void Dispose()
    {
        PendingInstallTempDir?.Dispose();
    }

    #endregion

    #region Enums

    public enum ModInstallState
    {
        Installed,
        PendingInstall,
        PendingUninstall,
    }

    #endregion
}