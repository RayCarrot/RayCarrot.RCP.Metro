using System.Windows.Input;
using System.Windows.Media;
using ByteSizeLib;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ModViewModel(ModLoaderViewModel modLoaderViewModel, LoaderViewModel loaderViewModel, Mod mod, ModManifestEntry modEntry, TempDirectory? pendingInstallTempDir = null)
    {
        ModLoaderViewModel = modLoaderViewModel;
        LoaderViewModel = loaderViewModel;
        PendingInstallTempDir = pendingInstallTempDir;
        Mod = mod;
        _isEnabled = modEntry.IsEnabled;
        // TODO-UPDATE: Perhaps we should check if the selected version is defined, otherwise go back to the default
        Version = modEntry.Version ?? Mod.DefaultVersion;
        Size = modEntry.Size;

        ModInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Author)), Metadata.Author),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Size)), ByteSize.FromBytes(modEntry.Size).ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Version)), Metadata.Version?.ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_ID)), Metadata.Id, UserLevel.Debug),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_FormatVersion)), Metadata.Format.ToString(), UserLevel.Debug),

            // TODO-UPDATE: Update these values when the user changes version
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_AddedFiles)), mod.GetAddedFiles(Version).Count().ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_RemovedFiles)), mod.GetRemovedFiles(Version).Count().ToString()),
        };

        ChangelogEntries = new ObservableCollection<ModChangelogEntry>(Metadata.Changelog ?? Array.Empty<ModChangelogEntry>());

        if (PendingInstallTempDir != null)
            SetState(InstallState.PendingInstall);

        ExtractContentsCommand = new AsyncRelayCommand(ExtractModContentsAsync);
        UninstallCommand = new RelayCommand(UninstallMod);
        OpenWebsiteCommand = new RelayCommand(OpenWebsite);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _isEnabled;

    #endregion

    #region Commands

    public ICommand ExtractContentsCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand OpenWebsiteCommand { get; }

    #endregion

    #region Public Properties

    public ModLoaderViewModel ModLoaderViewModel { get; }
    public LoaderViewModel LoaderViewModel { get; }
    public TempDirectory? PendingInstallTempDir { get; }
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
            ModLoaderViewModel.RefreshModifiedFiles();
            ModLoaderViewModel.HasChanges = true;
        }
    }
    public string Version { get; set; } // TODO-UPDATE: Allow changing from UI
    public long Size { get; }

    public InstallState State { get; set; }
    public LocalizedString? StateMessage { get; set; }
    public bool CanModify => State != InstallState.PendingUninstall;

    #endregion

    #region Private Methods

    private void SetState(InstallState state)
    {
        State = state;
        // TODO-UPDATE: Localize
        StateMessage = state switch
        {
            InstallState.Installed => null,
            InstallState.PendingInstall => "Pending install",
            InstallState.PendingUninstall => "Pending uninstall",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    #endregion

    #region Public Methods

    public async Task ExtractModContentsAsync()
    {
        // TODO-UPDATE: Localize
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

                // TODO-UPDATE: Implement. Copy folder?

                Logger.Info("Extracted mod contents");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Extracting mod contents");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when extracting the mod contents");
                return;
            }
        }

        // TODO-UPDATE: Localize
        await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The mod contents were successfully extracted");
    }

    public void UninstallMod()
    {
        SetState(InstallState.PendingUninstall);

        ModLoaderViewModel.RefreshModifiedFiles();
        ModLoaderViewModel.HasChanges = true;

        Logger.Info("Set mod '{0}' with version {1} and ID {2} to pending uninstall", Name, Metadata.Version, Metadata.Id);
    }

    public void LoadThumbnail()
    {
        Logger.Trace("Loading thumbnail for mod with ID {0}", Metadata.Id);

        try
        {
            Thumbnail = Mod.GetThumbnail();

            if (Thumbnail?.CanFreeze == true)
                Thumbnail.Freeze();
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

    public void Dispose()
    {
        PendingInstallTempDir?.Dispose();
    }

    #endregion

    #region Enums

    public enum InstallState
    {
        Installed,
        PendingInstall,
        PendingUninstall,
    }

    #endregion
}