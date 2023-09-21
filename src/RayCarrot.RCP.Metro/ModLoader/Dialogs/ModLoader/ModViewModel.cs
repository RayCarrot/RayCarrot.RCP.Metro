using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ByteSizeLib;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ModViewModel(ModLoaderViewModel modLoaderViewModel, LoaderViewModel loaderViewModel, DownloadableModsSource? downloadableModsSource, Mod mod, ModManifestEntry modEntry, TempDirectory? pendingInstallTempDir = null)
    {
        ModLoaderViewModel = modLoaderViewModel;
        LoaderViewModel = loaderViewModel;
        PendingInstallTempDir = pendingInstallTempDir;
        DownloadableModsSource = downloadableModsSource;
        Mod = mod;
        _isEnabled = modEntry.IsEnabled;
        _wasEnabled = _isEnabled;
        InstallInfo = modEntry.InstallInfo;

        ModInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Author)), Metadata.Author),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Size)), ByteSize.FromBytes(modEntry.InstallInfo.Size).ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Version)), Metadata.Version?.ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_ID)), Metadata.Id, UserLevel.Debug),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_FormatVersion)), Metadata.Format.ToString(), UserLevel.Debug),

            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_AddedFiles)), mod.GetAddedFiles().Count.ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_RemovedFiles)), mod.GetRemovedFiles().Count.ToString()),
        };

        ChangelogEntries = new ObservableCollection<ModChangelogEntry>(Metadata.Changelog ?? Array.Empty<ModChangelogEntry>());

        PanelFooterViewModel = DownloadableModsSource?.GetPanelFooterViewModel(InstallInfo);

        if (PendingInstallTempDir != null)
            SetInstallState(ModInstallState.PendingInstall);

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

    private bool? _wasEnabled;

    private bool _isEnabled;

    #endregion

    #region Commands

    public ICommand OpenLocationCommand { get; }
    public ICommand ExtractContentsCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand UpdateModCommand { get; }
    public ICommand OpenWebsiteCommand { get; }

    #endregion

    #region Public Properties

    public ModLoaderViewModel ModLoaderViewModel { get; }
    public LoaderViewModel LoaderViewModel { get; }
    public TempDirectory? PendingInstallTempDir { get; }
    public DownloadableModsSource? DownloadableModsSource { get; }
    public Mod Mod { get; }
    public ModMetadata Metadata => Mod.Metadata;
    public ObservableCollection<DuoGridItemViewModel> ModInfo { get; }
    public ObservableCollection<ModChangelogEntry> ChangelogEntries { get; }

    public string? Name => Metadata.Name;
    public string? Description => Metadata.Description;
    public string? Website => Metadata.Website;

    public bool HasWebsite => Uri.TryCreate(Metadata.Website, UriKind.Absolute, out _);
    public bool HasDescripton => !Metadata.Description.IsNullOrWhiteSpace();
    public ImageSource? Thumbnail { get; set; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            ReportNewChange();
        }
    }
    public ModInstallInfo InstallInfo { get; }

    public ModInstallState InstallState { get; set; }
    public LocalizedString? InstallStateMessage { get; set; }
    public bool CanModify => InstallState != ModInstallState.PendingUninstall;

    public ModUpdateState UpdateState { get; set; }
    public LocalizedString? UpdateStateMessage { get; set; }
    public object? UpdateData { get; set; }

    public bool HasChanges { get; private set; }

    public ModPanelFooterViewModel? PanelFooterViewModel { get; }

    #endregion

    #region Private Methods

    private void SetInstallState(ModInstallState state)
    {
        Logger.Trace("Set install state to {0} for mod with ID {1}", state, Metadata.Id);

        InstallState = state;
        // TODO-LOC
        InstallStateMessage = state switch
        {
            ModInstallState.Installed => null,
            ModInstallState.PendingInstall => "Pending install",
            ModInstallState.PendingUninstall => "Pending uninstall",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        ReportNewChange();
    }

    private void SetUpdateState(ModUpdateState state, LocalizedString message, object? updateData = null)
    {
        Logger.Trace("Set update state to {0} for mod with ID {1}", state, Metadata.Id);

        UpdateState = state;
        UpdateStateMessage = message;
        UpdateData = updateData;
    }

    private void ReportNewChange()
    {
        if (InstallState != ModInstallState.Installed)
        {
            _wasEnabled = null;
            HasChanges = true;
            ModLoaderViewModel.ReportNewChanges();
        }
        else
        {
            HasChanges = IsEnabled != _wasEnabled;
            ModLoaderViewModel.ReportNewChanges();
        }
    }

    #endregion

    #region Public Methods

    public async Task OpenLocationAsync()
    {
        await Services.File.OpenExplorerLocationAsync(Mod.ModDirectoryPath);
    }

    public async Task ExtractModContentsAsync()
    {
        // TODO-LOC
        using (await LoaderViewModel.RunAsync("Extracting mod contents"))
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

                Logger.Info("Extracted mod contents");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Extracting mod contents");

                // TODO-LOC
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when extracting the mod contents");
                return;
            }
        }

        // TODO-LOC
        await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The mod contents were successfully extracted");
    }

    public void UninstallMod()
    {
        SetInstallState(ModInstallState.PendingUninstall);

        Logger.Info("Set mod '{0}' with version {1} and ID {2} to pending uninstall", Name, Metadata.Version, Metadata.Id);
    }

    public async Task UpdateModAsync()
    {
        if (DownloadableModsSource == null)
            return;

        try
        {
            Logger.Info("Updating mod with ID {0}", Metadata.Id);

            ModDownload download = await DownloadableModsSource.GetModUpdateDownloadAsync(UpdateData);

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

            // TODO-LOC
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when updating the mod");

            return;
        }

        // Mark this mod as being uninstalled. Ideally the new downloaded mod should replace this,
        // but if it happens to have a separate id we want to remove this old mod.
        SetInstallState(ModInstallState.PendingUninstall);
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
            // TODO-LOC
            SetUpdateState(ModUpdateState.UnableToCheckForUpdates, "Unable to check for updates for locally installed mods");
            return;
        }

        // TODO-LOC
        SetUpdateState(ModUpdateState.CheckingForUpdates, "Checking for updates");

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