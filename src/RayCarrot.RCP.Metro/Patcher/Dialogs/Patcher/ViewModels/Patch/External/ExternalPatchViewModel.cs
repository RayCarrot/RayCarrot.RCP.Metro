using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ByteSizeLib;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public class ExternalPatchViewModel : PatchViewModel
{
    public ExternalPatchViewModel(PatcherViewModel patcherViewModel, ExternalPatchManifest externalManifest) : base(patcherViewModel)
    {
        ExternalManifest = externalManifest;
        
        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("Author:", externalManifest.Author),
            new("Size:", ByteSize.FromBytes(externalManifest.TotalSize).ToString()),
            new("Download size:", ByteSize.FromBytes(externalManifest.PatchSize).ToString()),
            new("Last modified:", externalManifest.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
            new("Revision:", externalManifest.Version.ToString()),
            new("ID:", externalManifest.ID, UserLevel.Debug),
            new("Added files:", (externalManifest.AddedFilesCount).ToString()),
            new("Removed files:", (externalManifest.RemovedFilesCount).ToString()),
        };

        DownloadCommand = new AsyncRelayCommand(() => PatcherViewModel.DownloadPatchAsync(ExternalManifest));
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool _loadingThumbnail;

    public ICommand DownloadCommand { get; }

    public override string ID => ExternalManifest.ID;
    public override string Name => ExternalManifest.Name ?? String.Empty;
    public override string Description => ExternalManifest.Description ?? String.Empty;
    public override string Website => ExternalManifest.Website ?? String.Empty;
    public override ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }

    public ExternalPatchManifest ExternalManifest { get; }

    public async Task LoadThumbnailAsync(Uri baseUri)
    {
        if (_loadingThumbnail || Thumbnail != null || ExternalManifest.Thumbnail == null)
            return;

        Logger.Info("Loading external thumbnail for patch with ID {0}", ID);

        try
        {
            _loadingThumbnail = true;

            Uri thumbUrl = new(baseUri, ExternalManifest.Thumbnail);

            // Create the web client
            using WebClient wc = new();

            // Download the thumbnail as a byte array
            byte[] rawThumb = await wc.DownloadDataTaskAsync(thumbUrl);

            // Wrap in a memory stream
            using MemoryStream thumbStream = new(rawThumb);

            // Create the image from the stream
            Thumbnail = BitmapFrame.Create(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            if (Thumbnail.CanFreeze)
                Thumbnail.Freeze();

            Logger.Info("Loaded external thumbnail for patch with ID {0}", ID);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Downloading external thumbnail");
        }
        finally
        {
            _loadingThumbnail = false;
        }
    }
}