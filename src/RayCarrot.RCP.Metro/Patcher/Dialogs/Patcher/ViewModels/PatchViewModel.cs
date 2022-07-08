﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ByteSizeLib;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public PatchViewModel(PatcherViewModel patcherViewModel, PatchManifest manifest, bool isEnabled, IPatchDataSource dataSource)
    {
        PatcherViewModel = patcherViewModel;
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
            new("ID", manifest.ID, UserLevel.Technical),
            new("Version", manifest.PatchVersion.ToString(), UserLevel.Debug),
            new("Added Files", (manifest.AddedFiles?.Length ?? 0).ToString()),
            new("Removed Files", (manifest.RemovedFiles?.Length ?? 0).ToString()),
        };

        ExtractContentsCommand = new AsyncRelayCommand(async () => await PatcherViewModel.ExtractPatchContentsAsync(this));
        ExportCommand = new AsyncRelayCommand(async () => await PatcherViewModel.ExportPatchAsync(this));
        UpdateCommand = new AsyncRelayCommand(async () => await PatcherViewModel.UpdatePatchAsync(this));
        RemoveCommand = new RelayCommand(() => PatcherViewModel.RemovePatch(this));
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

    public PatcherViewModel PatcherViewModel { get; }
    public PatchManifest Manifest { get; }
    public IPatchDataSource DataSource { get; }
    public ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }
    public ImageSource? Thumbnail { get; private set; }

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

    // Currently unused, but can be used to allow patches to be downloaded
    public string? PatchURL { get; set; }
    [MemberNotNullWhen(true, nameof(PatchURL))]
    public bool IsDownloaded { get; set; }
    public bool IsDownloadable => PatchURL != null && !IsDownloaded;

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

    public void Dispose()
    {
        DataSource.Dispose();
    }

    #endregion
}