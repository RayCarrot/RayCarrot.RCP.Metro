using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ByteSizeLib;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public abstract class LocalPatchViewModel : PatchViewModel
{
    #region Constructor

    protected LocalPatchViewModel(PatcherViewModel patcherViewModel, PatchFile patchFile, FileSystemPath filePath, bool isEnabled) 
        : base(patcherViewModel)
    {
        PatchFile = patchFile;
        _isEnabled = isEnabled;
        FilePath = filePath;

        // TODO-UPDATE: Localize
        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("Author", patchFile.Metadata.Author),
            new("Size", ByteSize.FromBytes(patchFile.Metadata.TotalSize).ToString()),
            new("Date", patchFile.Metadata.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
            new("Revision", patchFile.Metadata.Revision.ToString()),
            new("ID", patchFile.Metadata.ID, UserLevel.Debug),
            new("File Version", patchFile.Version.ToString(), UserLevel.Debug),
            new("Added Files", (patchFile.AddedFiles?.Length ?? 0).ToString()),
            new("Removed Files", (patchFile.RemovedFiles?.Length ?? 0).ToString()),
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

    public override string ID => Metadata.ID;
    public override string Name => Metadata.Name ?? String.Empty;
    public override string Description => Metadata.Description ?? String.Empty;
    public override ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }

    public PatchFile PatchFile { get; }
    public PatchMetadata Metadata => PatchFile.Metadata;
    public FileSystemPath FilePath { get; }

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
        Logger.Trace("Loading thumbnail for patch with ID {0}", ID);

        if (!PatchFile.HasThumbnail)
        {
            Logger.Trace("No thumbnail was found");

            Thumbnail = null;
            return;
        }

        using MemoryStream thumbStream = new(PatchFile.Thumbnail);

        // TODO-UPDATE: Try/catch
        Thumbnail = BitmapFrame.Create(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

        if (Thumbnail?.CanFreeze == true)
            Thumbnail.Freeze();
    }

    #endregion
}