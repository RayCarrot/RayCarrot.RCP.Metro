using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaDownloadableModViewModel : DownloadableModViewModel, IRecipient<ModDownloadedMessage>
{
    #region Constructor

    public GameBananaDownloadableModViewModel(
        GameBananaModsSource downloadableModsSource,
        ModLoaderViewModel modLoaderViewModel,
        HttpClient httpClient,
        int gameBananaId, 
        bool isFeatured) : base(downloadableModsSource)
    {
        _modLoaderViewModel = modLoaderViewModel;
        _httpClient = httpClient;

        GameBananaId = gameBananaId;
        IsFeatured = isFeatured;

        // Placeholder image
        Images = [new ImageViewModel()];

        ShowArchivedFiles = false;

        // Register messages
        Services.Messenger.RegisterAll(this, modLoaderViewModel.GameInstallation.InstallationId);

        // Create commands
        OpenInGameBananaCommand = new RelayCommand(OpenInGameBanana);
        OpenUserPageCommand = new RelayCommand(OpenUserPage);
        DownloadFileCommand = new AsyncRelayCommand(x => DownloadFileAsync((GameBananaFileViewModel)x!));
    }

    #endregion

    #region Constant Fields

    private const int MaxImages = 10;
    private const int ImageHeight = 180;

    #endregion

    #region Private Fields

    // NOTE: Might be worth abstracting this more in the future to not pass in these here
    private readonly ModLoaderViewModel _modLoaderViewModel;
    private readonly HttpClient _httpClient;

    private int _selectedImageIndex;

    #endregion

    #region Commands

    public ICommand OpenInGameBananaCommand { get; }
    public ICommand OpenUserPageCommand { get; }
    public ICommand DownloadFileCommand { get; }

    #endregion

    #region Public Properties

    public new GameBananaModsSource DownloadableModsSource => (GameBananaModsSource)base.DownloadableModsSource;

    public int GameBananaId { get; }
    public override string ModId => GameBananaId.ToString();
    public bool IsFeatured { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Text { get; set; }

    public string? SubmitterName { get; set; }
    public string? SubmitterProfileUrl { get; set; }

    // Do this to allow the name to be a link, but not the rest of the text
    public LocalizedString? UploadInfoPreText { get; set; }
    public LocalizedString? UploadInfoPostText { get; set; }

    public ModVersion? Version { get; set; }
    public override ModVersion? ModVersion => Version;
    public LocalizedString? VersionText { get; set; }
    public LocalizedString? LastUpdatedText { get; set; }

    public string? RootCategoryIconUrl { get; set; }
    public string? RootCategoryName { get; set; }

    public ImageViewModel? MainImage { get; set; }
    public double MainImageWidth { get; set; }
    public double MainImageHeight { get; set; }
    public ObservableCollection<ImageViewModel> Images { get; }
    public int SelectedImageIndex
    {
        get => _selectedImageIndex;
        set
        {
            _selectedImageIndex = value;
            UpdateCurrentImage();
        }
    }

    public int? LikesCount { get; set; }
    public int? DownloadsCount { get; set; }
    public int? ViewsCount { get; set; }

    public ObservableCollection<GameBananaFileViewModel>? Files { get; set; }
    public ObservableCollection<GameBananaFileViewModel>? ArchivedFiles { get; set; }
    public bool ShowArchivedFiles { get; set; }

    public ObservableCollection<CreditsGroupViewModel>? Credits { get; set; }

    #endregion

    #region Private Methods

    private void UpdateCurrentImage()
    {
        // Load the current image
        if (SelectedImageIndex >= 0 && SelectedImageIndex < Images.Count)
            Images[SelectedImageIndex].Load();
        
        // Preload the next image
        int nextImageIndex = SelectedImageIndex + 1;
        if (nextImageIndex >= 0 && nextImageIndex < Images.Count)
            Images[nextImageIndex].Load();
    }

    #endregion

    #region Public Methods

    public void OpenInGameBanana()
    {
        Services.App.OpenUrl($"https://gamebanana.com/mods/{GameBananaId}");
    }

    public void OpenUserPage()
    {
        if (SubmitterProfileUrl != null)
            Services.App.OpenUrl(SubmitterProfileUrl);
    }

    public async Task DownloadFileAsync(GameBananaFileViewModel file)
    {
        await _modLoaderViewModel.InstallModFromDownloadableFileAsync(
            source: DownloadableModsSource, 
            fileName: file.DownloadableFile.File, 
            downloadUrl: file.DownloadableFile.DownloadUrl, 
            fileSize: file.DownloadableFile.FileSize, 
            installData: new GameBananaInstallData(GameBananaId, file.DownloadableFile.Id));
    }

    public void LoadFeedDetails(GameBananaMod mod)
    {
        if (mod.Id != GameBananaId)
            throw new Exception($"GameBanana mod IDs {mod.Id} and {GameBananaId} do not match!");

        Name = mod.Name;

        SubmitterName = mod.Submitter?.Name;
        SubmitterProfileUrl = mod.Submitter?.ProfileUrl;

        if (mod.DateAdded != null && SubmitterName != null)
        {
            // TODO-UPDATE: Can we format date without weekday?
            // Uploaded by {0} on {1}
            UploadInfoPreText = new GeneratedLocString(() =>
            {
                string str = Resources.ModLoader_GameBanana_UploadInfo;
                str = str.Substring(0, str.IndexOf("{0}", StringComparison.InvariantCultureIgnoreCase));
                return String.Format(str, null, $"{mod.DateAdded:D}").TrimEnd();
            });
            UploadInfoPostText = new GeneratedLocString(() =>
            {
                string str = Resources.ModLoader_GameBanana_UploadInfo;
                str = str.Substring(str.IndexOf("{0}", StringComparison.InvariantCultureIgnoreCase) + 3);
                return String.Format(str, null, $"{mod.DateAdded:D}").TrimStart();
            });
        }

        // TODO-UPDATE: Can we format date without weekday?
        if (mod.DateModified != null && mod.DateModified != mod.DateAdded)
            LastUpdatedText = $"Last updated on {mod.DateModified:D}"; // TODO-LOC

        Version = ModVersion.TryParse(mod.Version, out ModVersion? v) ? v : null;
        if (Version != null)
            VersionText = $"Version {Version}"; // TODO-LOC

        if (mod.RootCategory != null)
        {
            if (!mod.RootCategory.IconUrl.IsNullOrEmpty())
                RootCategoryIconUrl = mod.RootCategory.IconUrl;

            RootCategoryName = mod.RootCategory.Name;
        }

        if (mod.PreviewMedia?.Images?.Length > 0)
        {
            GameBananaImage mainImage = mod.PreviewMedia.Images[0];
            MainImage = new ImageViewModel() { Url = $"{mainImage.BaseUrl}/{mainImage.File220}" };
            MainImage.Load();
            MainImageWidth = mainImage.File220Width;
            MainImageHeight = mainImage.File220Height;
        }

        LikesCount = mod.LikeCount ?? 0;
        ViewsCount = mod.ViewCount ?? 0;
    }

    public void LoadFullDetails(GameBananaMod mod)
    {
        if (mod.Id != GameBananaId)
            throw new Exception($"GameBanana mod IDs {mod.Id} and {GameBananaId} do not match!");

        Description = mod.Description;
        Text = mod.Text;

        if (mod.PreviewMedia?.Images?.Length > 1)
        {
            // Replace placeholder image
            Images[0].Url = $"{mod.PreviewMedia.Images[1].BaseUrl}/{mod.PreviewMedia.Images[1].File}";
            Images[0].DecodePixelHeight = ImageHeight;

            // Add remaining images
            foreach (GameBananaImage img in mod.PreviewMedia.Images.Skip(2).Take(MaxImages - 1))
                Images.Add(new ImageViewModel()
                {
                    Url = $"{img.BaseUrl}/{img.File}",
                    DecodePixelHeight = ImageHeight
                });
        }
        else
        {
            Images.Clear();
        }

        LikesCount = mod.LikeCount;
        DownloadsCount = mod.DownloadCount;
        ViewsCount = mod.ViewCount;

        // TODO-UPDATE: Filter out valid files (i.e. ones with 1-click mod manager)
        if (mod.Files != null)
        {
            ModViewModel? findDownloadedMod(GameBananaFile file) => _modLoaderViewModel.Mods.
                Where(x => x.DownloadableModsSource?.Id == DownloadableModsSource.Id).
                FirstOrDefault(x => x.IsDownloaded && x.DownloadedMod.InstallInfo.GetRequiredInstallData<GameBananaInstallData>().FileId == file.Id);

            Files = new ObservableCollection<GameBananaFileViewModel>(mod.Files.
                Where(x => !x.IsArchived).Select(x => new GameBananaFileViewModel(x)
                {
                    DownloadedMod = findDownloadedMod(x)
                }));
            ArchivedFiles = new ObservableCollection<GameBananaFileViewModel>(mod.Files.
                Where(x => x.IsArchived).Select(x => new GameBananaFileViewModel(x)
                {
                    DownloadedMod = findDownloadedMod(x)
                }));
        }

        if (mod.Credits != null)
        {
            Credits = mod.Credits.
                Select(x => new CreditsGroupViewModel(x.GroupName, x.Authors?.
                    Select(m => new AuthorViewModel(m.AvatarUrl != null
                            ? new ImageViewModel()
                            {
                                Url = m.AvatarUrl,
                                DecodePixelWidth = 25,
                            }
                            : null,
                        m.Name, m.Role, m.ProfileUrl ?? m.Url)).
                    ToObservableCollection())).
                ToObservableCollection();
        }
    }

    public override async Task LoadFullDetailsAsync()
    {
        GameBananaMod mod = await DownloadableModsSource.LoadModAsync(_httpClient, GameBananaId);
        LoadFullDetails(mod);
        UpdateCurrentImage();
    }

    public override void Dispose()
    {
        base.Dispose();

        Services.Messenger.UnregisterAll(this, _modLoaderViewModel.GameInstallation.InstallationId);
    }

    #endregion

    #region Message Receivers

    void IRecipient<ModDownloadedMessage>.Receive(ModDownloadedMessage message)
    {
        if (message.ModViewModel.DownloadableModsSource?.Id != DownloadableModsSource.Id ||
            Files == null)
            return;

        long fileId = message.DownloadedModViewModel.InstallInfo.GetRequiredInstallData<GameBananaInstallData>().FileId;

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

    #region Classes

    public class CreditsGroupViewModel : BaseViewModel
    {
        public CreditsGroupViewModel(string groupName, ObservableCollection<AuthorViewModel>? authors)
        {
            GroupName = groupName;
            Authors = authors;
        }

        public string GroupName { get; }
        public ObservableCollection<AuthorViewModel>? Authors { get; }
    }

    public class AuthorViewModel : BaseViewModel
    {
        public AuthorViewModel(ImageViewModel? avatar, string? name, string? role, string? profileUrl)
        {
            Avatar = avatar;
            Avatar?.Load();
            Name = name;
            Role = role;
            ProfileUrl = profileUrl;

            OpenUserPageCommand = new RelayCommand(OpenUserPage);
        }

        public ICommand OpenUserPageCommand { get; }

        public ImageViewModel? Avatar { get; }
        public string? Name { get; }
        public string? Role { get; }
        public string? ProfileUrl { get; }

        public void OpenUserPage()
        {
            if (ProfileUrl != null)
                Services.App.OpenUrl(ProfileUrl);
        }
    }

    public class ImageViewModel : BaseViewModel
    {
        public string? Url { get; set; }
        public int DecodePixelWidth { get; set; }
        public int DecodePixelHeight { get; set; }

        public ImageSource? ImageSource { get; set; }

        public void Load()
        {
            // Do not load if already loaded
            if (ImageSource != null)
                return;

            // Make sure we have a url
            if (Url == null)
                return;

            Uri uri = new(Url);

            BitmapImage imgSource = new();
            imgSource.BeginInit();
            imgSource.CacheOption = BitmapCacheOption.OnLoad;
            imgSource.DecodePixelWidth = DecodePixelWidth;
            imgSource.DecodePixelHeight = DecodePixelHeight;
            imgSource.UriSource = uri;
            imgSource.EndInit();

            ImageSource = imgSource;
        }
    }

    #endregion
}