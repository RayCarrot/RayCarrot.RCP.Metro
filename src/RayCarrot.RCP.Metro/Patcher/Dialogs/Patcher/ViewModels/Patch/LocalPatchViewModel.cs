using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ByteSizeLib;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public class LocalPatchViewModel : PatchViewModel
{
    #region Constructor

    public LocalPatchViewModel(PatcherViewModel patcherViewModel, PatchManifest manifest, bool isEnabled, IPatchDataSource dataSource) 
        : base(patcherViewModel)
    {
        Manifest = manifest;
        _isEnabled = isEnabled;
        DataSource = dataSource;

        // TODO-UPDATE: Localize
        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("Author", manifest.Author),
            new("Size", ByteSize.FromBytes(manifest.TotalSize).ToString()),
            new("Date", manifest.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
            new("Revision", manifest.Revision.ToString()),
            new("ID", manifest.ID, UserLevel.Debug),
            new("Version", manifest.PatchVersion.ToString(), UserLevel.Debug),
            new("Added Files", (manifest.AddedFiles?.Length ?? 0).ToString()),
            new("Removed Files", (manifest.RemovedFiles?.Length ?? 0).ToString()),
        };

        ExtractContentsCommand = new AsyncRelayCommand(async () => await PatcherViewModel.ExtractPatchContentsAsync(this));
        ExportCommand = new AsyncRelayCommand(async () => await PatcherViewModel.ExportPatchAsync(this));
        UpdateCommand = new AsyncRelayCommand(async () => await PatcherViewModel.UpdatePatchAsync(this));
        RemoveCommand = new RelayCommand(() => PatcherViewModel.RemovePatch(this, true));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ExtractContentsCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand RemoveCommand { get; }

    #endregion

    #region Private Fields

    private bool _isEnabled;

    #endregion

    #region Public Properties

    public override string Name => Manifest.Name ?? String.Empty;
    public override string Description => Manifest.Description ?? String.Empty;
    public override ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }

    public PatchManifest Manifest { get; }
    public IPatchDataSource DataSource { get; }

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

    #endregion

    #region Public Methods

    public void LoadThumbnail()
    {
        Logger.Trace("Loading thumbnail for patch with ID {0}", Manifest.ID);

        if (!Manifest.HasAsset(PatchAsset.Thumbnail))
        {
            Logger.Trace("No thumbnail asset was found");

            Thumbnail = null;
            return;
        }

        using Stream thumbStream = DataSource.GetAsset(PatchAsset.Thumbnail);

        // This doesn't seem to work when reading from a zip archive as read-only due to the stream
        // not supporting seeking. Specifying the format directly using a PngBitmapDecoder still works.
        //Thumbnail = BitmapFrame.Create(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        Thumbnail = new PngBitmapDecoder(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.FirstOrDefault();

        if (Thumbnail?.CanFreeze == true)
            Thumbnail.Freeze();
    }

    public override void Dispose()
    {
        base.Dispose();

        DataSource.Dispose();
    }

    #endregion
}