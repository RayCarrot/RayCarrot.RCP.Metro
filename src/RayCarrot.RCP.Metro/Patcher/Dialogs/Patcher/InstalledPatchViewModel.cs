using System.Windows.Input;
using System.Windows.Media;
using ByteSizeLib;
using RayCarrot.RCP.Metro.Patcher.FileInfo;
using RayCarrot.RCP.Metro.Patcher.Library;
using RayCarrot.RCP.Metro.Patcher.Metadata;

namespace RayCarrot.RCP.Metro.Patcher.Dialogs.Patcher;

public class InstalledPatchViewModel : BaseViewModel
{
    #region Constructor

    public InstalledPatchViewModel(PatcherViewModel patcherViewModel, InstalledPatch installedPatch, PatchManifestEntry patchEntry)
    {
        PatcherViewModel = patcherViewModel;
        InstalledPatch = installedPatch;
        _isEnabled = patchEntry.IsEnabled;
        // TODO-UPDATE: Perhaps we should check if the selected version is defined, otherwise go back to the default
        Version = patchEntry.Version ?? InstalledPatch.DefaultVersion;

        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Author)), Metadata.Author),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Size)), ByteSize.FromBytes(patchEntry.Size).ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_Version)), Metadata.Version?.ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_ID)), Metadata.Id, UserLevel.Debug),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_FormatVersion)), Metadata.Format.ToString(), UserLevel.Debug),

            // TODO-UPDATE: Update these values when the user changes version
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_AddedFiles)), installedPatch.GetAddedFiles(Version).Count().ToString()),
            new(new ResourceLocString(nameof(Resources.Patcher_PatchInfo_RemovedFiles)), installedPatch.GetRemovedFiles(Version).Count().ToString()),
        };

        ChangelogEntries = new ObservableCollection<PatchChangelogEntry>(Metadata.Changelog ?? Array.Empty<PatchChangelogEntry>());

        ExtractContentsCommand = new AsyncRelayCommand(async () => await PatcherViewModel.ExtractPatchContentsAsync(this));
        UninstallCommand = new AsyncRelayCommand(async () => await PatcherViewModel.UninstallPatchAsync(this));
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

    public PatcherViewModel PatcherViewModel { get; }
    public InstalledPatch InstalledPatch { get; }
    public PatchMetadata Metadata => InstalledPatch.Metadata;
    public ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }
    public ObservableCollection<PatchChangelogEntry> ChangelogEntries { get; }

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
            PatcherViewModel.RefreshPatchedFiles();
            PatcherViewModel.HasChanges = true;
        }
    }
    public string Version { get; set; } // TODO-UPDATE: Allow changing from UI

    #endregion

    #region Public Methods

    public void LoadThumbnail()
    {
        Logger.Trace("Loading thumbnail for patch with ID {0}", Metadata.Id);

        try
        {
            Thumbnail = InstalledPatch.GetThumbnail();

            if (Thumbnail?.CanFreeze == true)
                Thumbnail.Freeze();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading patch thumbnail");
        }
    }

    public void OpenWebsite()
    {
        if (Metadata.Website != null)
            Services.App.OpenUrl(Metadata.Website);
    }

    #endregion
}