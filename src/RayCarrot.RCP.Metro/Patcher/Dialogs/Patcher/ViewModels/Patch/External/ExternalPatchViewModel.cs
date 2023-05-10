using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ByteSizeLib;

namespace RayCarrot.RCP.Metro.Patcher;

public class ExternalPatchViewModel : PatchViewModel
{
    public ExternalPatchViewModel(PatcherViewModel patcherViewModel, ExternalPatchMetaData externalPatchMetaData, Uri baseUri) 
        : base(patcherViewModel)
    {
        ExternalPatchMetaData = externalPatchMetaData;
        BaseUri = baseUri;
        ID = externalPatchMetaData.Id ??
             throw new ArgumentException("The patch metadata has no id", nameof(externalPatchMetaData));

        if (ExternalPatchMetaData.PatchUrl == null)
            throw new ArgumentException("The patch metadata has no patch url", nameof(externalPatchMetaData));

        ChangelogEntries = new ObservableCollection<PatchChangelogEntry>(ExternalPatchMetaData.ChangelogEntries?.Select(
            x => new PatchChangelogEntry
            {
                Version = x?.Version ?? new PatchVersion(0, 0, 0),
                Date = x?.Date ?? DateTime.MinValue,
                Description = x?.Description ?? String.Empty
            }) ?? Array.Empty<PatchChangelogEntry>());

        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("Author:", externalPatchMetaData.Author),
            new("Size:", externalPatchMetaData.TotalSize == null 
                ? null 
                : ByteSize.FromBytes(externalPatchMetaData.TotalSize.Value).ToString()),
            new("Download size:", externalPatchMetaData.FileSize == null
                ? null
                : ByteSize.FromBytes(externalPatchMetaData.FileSize.Value).ToString()),
            new("Last modified:", externalPatchMetaData.ModifiedDate?.ToString("D", CultureInfo.CurrentCulture)),
            new("Revision:", externalPatchMetaData.Version?.ToString()),
            new("ID:", externalPatchMetaData.Id, UserLevel.Debug),
            new("Added files:", externalPatchMetaData.AddedFilesCount?.ToString()),
            new("Removed files:", externalPatchMetaData.RemovedFilesCount?.ToString()),
        };

        DownloadCommand = new AsyncRelayCommand(() =>
        {
            Uri patchURL = new(BaseUri, ExternalPatchMetaData.PatchUrl);
            return PatcherViewModel.DownloadPatchAsync(patchURL);
        });
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool _loadingThumbnail;

    public ICommand DownloadCommand { get; }

    public override string ID { get; }
    public override string Name => ExternalPatchMetaData.Name ?? String.Empty;
    public override string Description => ExternalPatchMetaData.Description ?? String.Empty;
    public override string Website => ExternalPatchMetaData.Website ?? String.Empty;
    public override ObservableCollection<PatchChangelogEntry> ChangelogEntries { get; }
    public override ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }

    public ExternalPatchMetaData ExternalPatchMetaData { get; }
    public Uri BaseUri { get; }
    public bool IsUpdate { get; set; }

    public async Task LoadThumbnailAsync()
    {
        if (_loadingThumbnail || Thumbnail != null || ExternalPatchMetaData.ThumbnailUrl == null)
            return;

        Logger.Info("Loading external thumbnail for patch with ID {0}", ID);

        try
        {
            _loadingThumbnail = true;

            Uri thumbUrl = new(BaseUri, ExternalPatchMetaData.ThumbnailUrl);

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