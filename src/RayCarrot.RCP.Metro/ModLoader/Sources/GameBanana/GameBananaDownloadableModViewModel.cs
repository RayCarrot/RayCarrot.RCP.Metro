using System.Text.RegularExpressions;
using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaDownloadableModViewModel : DownloadableModViewModel, IRecipient<ModInstalledMessage>
{
    #region Constructor

    public GameBananaDownloadableModViewModel(
        DownloadableModsSource downloadableModsSource,
        ModLoaderViewModel modLoaderViewModel,
        int gameBananaId, 
        string name, 
        string uploaderUserName, 
        string? uploaderUrl, 
        DateTime uploadDate, 
        string description, 
        string text,
        string version,
        GameBananaRootCategory? rootCategory, 
        GameBananaMedia? previewMedia, 
        int likesCount, 
        int downloadsCount, 
        int viewsCount, 
        IReadOnlyCollection<GameBananaFile> files,
        bool isFeatured)
    {
        _downloadableModsSource = downloadableModsSource;
        _modLoaderViewModel = modLoaderViewModel;

        GameBananaId = gameBananaId;
        Name = name;
        UploaderUserName = uploaderUserName;
        UploaderUrl = uploaderUrl;
        UploadDate = uploadDate;
        Description = description;
        Text = RemoveHthmlFromString(text);

        // Uploaded by {0} on {1}
        UploadInfoPreText = new GeneratedLocString(() =>
        {
            string str = Resources.ModLoader_GameBanana_UploadInfo;
            str = str.Substring(0, str.IndexOf("{0}", StringComparison.InvariantCultureIgnoreCase));
            return String.Format(str, null, $"{UploadDate:D}").TrimEnd();
        });
        UploadInfoPostText = new GeneratedLocString(() =>
        {
            string str = Resources.ModLoader_GameBanana_UploadInfo;
            str = str.Substring(str.IndexOf("{0}", StringComparison.InvariantCultureIgnoreCase) + 3);
            return String.Format(str, null, $"{UploadDate:D}").TrimStart();
        });

        Version = ModVersion.TryParse(version, out ModVersion? v) ? v : null;

        if (rootCategory != null)
        {
            if (!rootCategory.IconUrl.IsNullOrEmpty())
                RootCategoryIconUrl = rootCategory.IconUrl;

            RootCategoryName = rootCategory.Name;
        }

        if (previewMedia?.Images is { Length: > 0 } images)
        {
            GameBananaImage img = images[0];

            // Use the 220px version if it exists
            if (img.File220 != null)
            {
                ImageUrl = img.BaseUrl + "/" + img.File220;
                ImageWidth = img.File220Width;
                ImageHeight = img.File220Height;
            }
            else
            {
                ImageUrl = img.BaseUrl + "/" + img.File;
                ImageWidth = 220;
                ImageHeight = Double.NaN;
            }
        }

        LikesCount = likesCount;
        DownloadsCount = downloadsCount;
        ViewsCount = viewsCount;

        ModViewModel? findDownloadedMod(GameBananaFile file) => modLoaderViewModel.Mods.
            Where(mod => mod.DownloadableModsSource?.Id == downloadableModsSource.Id).
            FirstOrDefault(mod => mod.InstallInfo.GetRequiredInstallData<GameBananaInstallData>().FileId == file.Id);

        Files = new ObservableCollection<GameBananaFileViewModel>(files.Where(x => !x.IsArchived).Select(x => new GameBananaFileViewModel(x)
        {
            DownloadedMod = findDownloadedMod(x)
        }));
        ArchivedFiles = new ObservableCollection<GameBananaFileViewModel>(files.Where(x => x.IsArchived).Select(x => new GameBananaFileViewModel(x)
        {
            DownloadedMod = findDownloadedMod(x)
        }));

        ShowArchivedFiles = false;

        IsFeatured = isFeatured;

        HasViewed = Services.Data.ModLoader_ViewedMods.TryGetValue(downloadableModsSource.Id, out List<ViewedMod> viewedMod) &&
                    viewedMod.Any(x => x.Id == GameBananaId.ToString());

        Services.Messenger.RegisterAll(this, modLoaderViewModel.GameInstallation.InstallationId);

        OpenInGameBananaCommand = new RelayCommand(OpenInGameBanana);
        OpenUserPageCommand = new RelayCommand(OpenUserPage, UploaderUrl != null);
        DownloadFileCommand = new AsyncRelayCommand(x => DownloadFileAsync((GameBananaFileViewModel)x!));
    }

    #endregion

    #region Private Fields

    // NOTE: Might be worth abstracting this more in the future to not pass in these here
    private readonly DownloadableModsSource _downloadableModsSource;
    private readonly ModLoaderViewModel _modLoaderViewModel;

    #endregion

    #region Commands

    public ICommand OpenInGameBananaCommand { get; }
    public ICommand OpenUserPageCommand { get; }
    public ICommand DownloadFileCommand { get; }

    #endregion

    #region Public Properties

    public int GameBananaId { get; }

    public string Name { get; }
    public string UploaderUserName { get; }
    public string? UploaderUrl { get; }
    public DateTime UploadDate { get; }
    public string Description { get; }
    public string Text { get; }

    // Do this to allow the name to be a link, but not the rest of the text
    public LocalizedString UploadInfoPreText { get; }
    public LocalizedString UploadInfoPostText { get; }

    public ModVersion? Version { get; }

    public string? RootCategoryIconUrl { get; }
    public string? RootCategoryName { get; }

    public string? ImageUrl { get; }
    public double ImageWidth { get; }
    public double ImageHeight { get; }

    public int LikesCount { get; }
    public int DownloadsCount { get; }
    public int ViewsCount { get; }

    public ObservableCollection<GameBananaFileViewModel> Files { get; }
    public ObservableCollection<GameBananaFileViewModel> ArchivedFiles { get; }
    public bool ShowArchivedFiles { get; set; }

    public bool IsFeatured { get; set; }

    public bool HasViewed { get; set; }

    #endregion

    #region Private Methods

    private static string RemoveHthmlFromString(string html)
    {
        // Linebreaks
        html = html.Replace("<br>", Environment.NewLine);
        html = html.Replace(@"</li>", Environment.NewLine);
        html = html.Replace(@"</h3>", Environment.NewLine);
        html = html.Replace(@"</h2>", Environment.NewLine);
        html = html.Replace(@"</h1>", Environment.NewLine);
        html = html.Replace("<ul>", Environment.NewLine);
        
        // Bullet point
        html = html.Replace("<li>", "• ");
        
        // Unique spaces
        html = html.Replace("&nbsp;", " ");
        html = html.Replace(@"\u00a0", " ");

        // Unique characters
        html = html.Replace("&amp;", "&");
        html = html.Replace("&gt;", ">");
        
        // Remove tabs
        html = html.Replace("\t", String.Empty);
        
        // Remove all remaining html tags
        html = Regex.Replace(html, "<.*?>", String.Empty);
        
        // Convert newlines of 3 or more to 2 newlines
        html = Regex.Replace(html, "[\\r\\n]{3,}", "\n\n", RegexOptions.Multiline);
        
        // Trim extra whitespace at start and end
        return html.Trim();
    }

    #endregion

    #region Public Methods

    public void OpenInGameBanana()
    {
        Services.App.OpenUrl($"https://gamebanana.com/mods/{GameBananaId}");
    }

    public void OpenUserPage()
    {
        if (UploaderUrl != null)
            Services.App.OpenUrl(UploaderUrl);
    }

    public async Task DownloadFileAsync(GameBananaFileViewModel file)
    {
        await _modLoaderViewModel.InstallModFromDownloadableFileAsync(
            source: _downloadableModsSource, 
            fileName: file.DownloadableFile.File, 
            downloadUrl: file.DownloadableFile.DownloadUrl, 
            fileSize: file.DownloadableFile.FileSize, 
            installData: new GameBananaInstallData(GameBananaId, file.DownloadableFile.Id));
    }

    public override void OnSelected()
    {
        base.OnSelected();

        HasViewed = true;

        if (!Services.Data.ModLoader_ViewedMods.ContainsKey(_downloadableModsSource.Id))
            Services.Data.ModLoader_ViewedMods.Add(_downloadableModsSource.Id, new List<ViewedMod>());

        Services.Data.ModLoader_ViewedMods[_downloadableModsSource.Id].Add(new ViewedMod(GameBananaId.ToString(), DateTime.Now, Version));
    }

    public override void Dispose()
    {
        base.Dispose();

        Services.Messenger.UnregisterAll(this, _modLoaderViewModel.GameInstallation.InstallationId);
    }

    #endregion

    #region Message Receivers

    void IRecipient<ModInstalledMessage>.Receive(ModInstalledMessage message)
    {
        if (message.ModViewModel.DownloadableModsSource?.Id != _downloadableModsSource.Id)
            return;

        long fileId = message.ModViewModel.InstallInfo.GetRequiredInstallData<GameBananaInstallData>().FileId;

        foreach (GameBananaFileViewModel file in Files)
        {
            if (file.DownloadableFile.Id == fileId)
            {
                file.DownloadedMod = message.ModViewModel;
                break;
            }
        }
    }

    #endregion
}