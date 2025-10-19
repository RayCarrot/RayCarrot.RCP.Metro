using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ModViewModel(
        ModLoaderViewModel modLoaderViewModel, 
        LoaderViewModel loaderViewModel, 
        DownloadableModsSource? downloadableModsSource)
    {
        ModLoaderViewModel = modLoaderViewModel;
        LoaderViewModel = loaderViewModel;
        DownloadableModsSource = downloadableModsSource;

        HasChangesToApply = false;

        CancelCommand = new RelayCommand(CancelDownload);
        ReportNewChangeCommand = new RelayCommand(ReportNewChange);
        OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
        ExtractContentsCommand = new AsyncRelayCommand(ExtractModContentsAsync);
        UninstallCommand = new RelayCommand(UninstallMod);
        UpdateModCommand = new AsyncRelayCommand(UpdateModAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool? _wasEnabled;

    #endregion

    #region Commands

    public ICommand CancelCommand { get; }
    public ICommand ReportNewChangeCommand { get; }
    public ICommand OpenLocationCommand { get; }
    public ICommand ExtractContentsCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand UpdateModCommand { get; }

    #endregion

    #region Public Properties

    public string? Name { get; set; }
    public object? InstallData { get; set; }

    public bool IsEnabled { get; set; }

    public ModLoaderViewModel ModLoaderViewModel { get; }
    public LoaderViewModel LoaderViewModel { get; }
    public DownloadableModsSource? DownloadableModsSource { get; }
    
    public TempDirectory? PendingInstallTempDir { get; init; }

    public ModInstallState InstallState { get; set; }
    public LocalizedString? InstallStateMessage { get; set; }
    public bool CanModify => InstallState is not (ModInstallState.Downloading or ModInstallState.Extracting or ModInstallState.PendingUninstall);

    public CancellationTokenSource? DownloadCancellationTokenSource { get; set; }
    public bool ShowProgress => InstallState is ModInstallState.Downloading or ModInstallState.Extracting;
    public bool HasProgress { get; set; }
    public double CurrentProgress { get; set; }
    public double MinProgress { get; set; }
    public double MaxProgress { get; set; }

    public ModUpdateState UpdateState { get; set; }
    public LocalizedString? UpdateStateMessage { get; set; }
    public object? UpdateData { get; set; }

    [MemberNotNullWhen(true, nameof(DownloadedMod))]
    public bool IsDownloaded => DownloadedMod != null;
    public DownloadedModViewModel? DownloadedMod { get; set; }

    public bool HasChangesToApply { get; private set; }

    #endregion

    #region Private Methods

    private void UpdateHasChangesToApply()
    {
        HasChangesToApply = InstallState switch
        {
            ModInstallState.Installed => IsEnabled != _wasEnabled,
            ModInstallState.Downloading => false,
            ModInstallState.Extracting => false,
            ModInstallState.PendingInstall => true,
            ModInstallState.PendingUninstall => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void ReportNewChange()
    {
        UpdateHasChangesToApply();
        ModLoaderViewModel.ReportNewChanges();
    }

    #endregion

    #region Public Methods

    public void InitDownloading(string? name, object? installData)
    {
        IsEnabled = false;
        _wasEnabled = null;

        DownloadCancellationTokenSource = new();
        HasProgress = false;

        DownloadedMod = null;

        Name = name;
        InstallData = installData;

        SetUpdateState(ModUpdateState.None, String.Empty);
        SetInstallState(ModInstallState.Downloading);
        UpdateHasChangesToApply();
    }

    [MemberNotNull(nameof(DownloadedMod))]
    public void InitDownloaded(ModInstallState installState, Mod mod, ModManifestEntry modEntry)
    {
        if (installState is ModInstallState.Downloading or ModInstallState.Extracting)
            throw new Exception("Can't initialize a mod as downloaded with the state being set to downloading");

        IsEnabled = modEntry.IsEnabled;
        _wasEnabled = IsEnabled;

        DownloadCancellationTokenSource = null;
        HasProgress = false;

        DownloadedMod = new DownloadedModViewModel(ModLoaderViewModel.GameInstallation, DownloadableModsSource, mod, modEntry);
        DownloadedMod.LoadThumbnail();

        Name = DownloadedMod.Name;
        InstallData = DownloadableModsSource?.ParseInstallData(DownloadedMod.InstallInfo.Data);

        SetUpdateState(ModUpdateState.None, String.Empty);
        SetInstallState(installState);
        UpdateHasChangesToApply();
    }

    public void SetProgress(Progress progress)
    {
        HasProgress = true;
        CurrentProgress = progress.Current;
        MinProgress = progress.Min;
        MaxProgress = progress.Max;
    }

    public void SetInstallState(ModInstallState state)
    {
        Logger.Trace("Set install state to {0} for mod with ID {1}", state, DownloadedMod?.Metadata.Id);

        InstallState = state;
        InstallStateMessage = state switch
        {
            ModInstallState.Installed => null,
            ModInstallState.Downloading => new ResourceLocString(nameof(Resources.ModLoader_InstallState_Downloading)),
            ModInstallState.Extracting => new ResourceLocString(nameof(Resources.ModLoader_InstallState_Extracting)),
            ModInstallState.PendingInstall => new ResourceLocString(nameof(Resources.ModLoader_InstallState_PendingInstall)),
            ModInstallState.PendingUninstall => new ResourceLocString(nameof(Resources.ModLoader_InstallState_PendingUninstall)),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        // Disable if pending uninstall
        if (state == ModInstallState.PendingUninstall && IsDownloaded)
            IsEnabled = false;
    }

    public void SetUpdateState(ModUpdateState state, LocalizedString message, object? updateData = null)
    {
        Logger.Trace("Set update state to {0} for mod with ID {1}", state, DownloadedMod?.Metadata.Id);

        UpdateState = state;
        UpdateStateMessage = message;
        UpdateData = updateData;
    }

    public void CancelDownload()
    {
        DownloadCancellationTokenSource?.Cancel();
    }

    public async Task OpenLocationAsync()
    {
        if (!IsDownloaded)
            return;
        
        await Services.File.OpenExplorerLocationAsync(DownloadedMod.Mod.ModDirectoryPath);
    }

    public async Task ExtractModContentsAsync()
    {
        if (!IsDownloaded)
            return;

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

                await Task.Run(() => Services.File.CopyDirectory(DownloadedMod.Mod.ModDirectoryPath, result.SelectedDirectory, false, true));

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
        if (!IsDownloaded)
            return;

        SetInstallState(ModInstallState.PendingUninstall);
        ReportNewChange();

        Logger.Info("Set mod '{0}' with version {1} and ID {2} to pending uninstall", DownloadedMod.Name, DownloadedMod.Metadata.Version, DownloadedMod.Metadata.Id);
    }

    public async Task UpdateModAsync()
    {
        if (DownloadableModsSource == null || !IsDownloaded)
            return;

        try
        {
            Logger.Info("Updating mod with ID {0}", DownloadedMod.Metadata.Id);

            ModDownload? download = await DownloadableModsSource.GetModUpdateDownloadAsync(UpdateData);

            if (download == null)
                return;

            await ModLoaderViewModel.InstallModFromDownloadableFileAsync(
                source: DownloadableModsSource,
                existingMod: this,
                fileName: download.FileName,
                downloadUrl: download.DownloadUrl,
                fileSize: download.FileSize,
                installData: download.InstallData,
                modName: Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Updating mod");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ModLoader_UpdateError);
        }
    }

    public async Task CheckForUpdateAsync(HttpClient httpClient)
    {
        if (InstallState != ModInstallState.Installed || !IsDownloaded)
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
            ModUpdateCheckResult result = await DownloadableModsSource.CheckForUpdateAsync(httpClient, DownloadedMod.InstallInfo);
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
        Downloading,
        Extracting,
        PendingInstall,
        PendingUninstall,
    }

    #endregion
}