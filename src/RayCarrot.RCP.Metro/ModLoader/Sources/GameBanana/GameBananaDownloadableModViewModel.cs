using System.Net.Http;
using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaDownloadableModViewModel : DownloadableModViewModel
{
    #region Constructor

    public GameBananaDownloadableModViewModel(
        GameBananaModsSource downloadableModsSource,
        ModLoaderViewModel modLoaderViewModel,
        WebImageCache webImageCache,
        HttpClient httpClient,
        int gameBananaId, 
        bool isFeatured) : base(downloadableModsSource, gameBananaId.ToString())
    {
        _modLoaderViewModel = modLoaderViewModel;
        _webImageCache = webImageCache;
        _httpClient = httpClient;

        GameBananaId = gameBananaId;
        IsFeatured = isFeatured;

        // Placeholder image
        Images = [new WebImageViewModel(_webImageCache)];

        ShowArchivedFiles = false;

        // Create commands
        OpenInGameBananaCommand = new RelayCommand(OpenInGameBanana);
        OpenUserPageCommand = new RelayCommand(OpenUserPage);
    }

    #endregion

    #region Constant Fields

    private const int MaxImages = 10;
    private const int ImageHeight = 180;

    #endregion

    #region Private Fields

    // NOTE: Might be worth abstracting this more in the future to not pass in these here
    private readonly ModLoaderViewModel _modLoaderViewModel;
    private readonly WebImageCache _webImageCache;
    private readonly HttpClient _httpClient;

    private int _selectedImageIndex;

    #endregion

    #region Commands

    public ICommand OpenInGameBananaCommand { get; }
    public ICommand OpenUserPageCommand { get; }

    #endregion

    #region Public Properties

    public new GameBananaModsSource DownloadableModsSource => (GameBananaModsSource)base.DownloadableModsSource;

    public int GameBananaId { get; }
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

    public WebImageViewModel? RootCategoryIconImage { get; set; }
    public string? RootCategoryName { get; set; }

    public WebImageViewModel? MainImage { get; set; }
    public double MainImageWidth { get; set; }
    public double MainImageHeight { get; set; }
    public ObservableCollection<WebImageViewModel> Images { get; }
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
    public bool HasNoValidFiles { get; set; }

    public bool HasLoadedFeedDetails { get; set; }

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
        file.IsAddedToLibrary = true;

        await _modLoaderViewModel.InstallModFromDownloadableFileAsync(
            source: DownloadableModsSource, 
            existingMod: null,
            fileName: file.DownloadableFile.File, 
            downloadUrl: file.DownloadableFile.DownloadUrl, 
            fileSize: file.DownloadableFile.FileSize, 
            installData: new GameBananaInstallData(GameBananaId, file.DownloadableFile.Id),
            modName: Name);
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

        if (mod.DateUpdated != null && mod.DateUpdated != mod.DateAdded)
            LastUpdatedText = $"Last updated on {mod.DateUpdated:D}"; // TODO-LOC

        Version = ModVersion.TryParse(mod.Version, out ModVersion? v) ? v : null;
        if (Version != null)
            VersionText = $"Version {Version}"; // TODO-LOC

        if (mod.RootCategory != null)
        {
            if (!mod.RootCategory.IconUrl.IsNullOrEmpty())
            {
                RootCategoryIconImage = new WebImageViewModel(_webImageCache) { Url = mod.RootCategory.IconUrl, };
                RootCategoryIconImage.Load();
            }

            RootCategoryName = mod.RootCategory.Name;
        }

        if (mod.PreviewMedia?.Images?.Length > 0)
        {
            GameBananaImage mainImage = mod.PreviewMedia.Images[0];
            MainImage = new WebImageViewModel(_webImageCache) { Url = $"{mainImage.BaseUrl}/{mainImage.File220}" };
            MainImage.Load();
            MainImageWidth = mainImage.File220Width;
            MainImageHeight = mainImage.File220Height;
        }

        LikesCount = mod.LikeCount ?? 0;
        ViewsCount = mod.ViewCount ?? 0;

        HasLoadedFeedDetails = true;
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
                Images.Add(new WebImageViewModel(_webImageCache)
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

        if (mod.Files != null)
        {
            List<GameBananaFile> validFiles = DownloadableModsSource.GetValidFiles(mod.Files);

            IReadOnlyCollection<ModViewModel> mods = _modLoaderViewModel.GetMods();
            bool isModAddedToLibrary(GameBananaFile file) => mods.
                Where(x => x.DownloadableModsSource?.Id == DownloadableModsSource.Id).
                Any(x => (x.InstallData as GameBananaInstallData)?.FileId == file.Id);

            Files = new ObservableCollection<GameBananaFileViewModel>(validFiles.
                Where(x => !x.IsArchived).Select(x => new GameBananaFileViewModel(x, DownloadFileAsync)
                {
                    IsAddedToLibrary = isModAddedToLibrary(x)
                }));
            ArchivedFiles = new ObservableCollection<GameBananaFileViewModel>(validFiles.
                Where(x => x.IsArchived).Select(x => new GameBananaFileViewModel(x, DownloadFileAsync)
                {
                    IsAddedToLibrary = isModAddedToLibrary(x)
                }));

            HasNoValidFiles = Files.Count == 0 && ArchivedFiles.Count == 0;
        }

        if (mod.Credits != null)
        {
            Credits = mod.Credits.
                Select(x => new CreditsGroupViewModel(x.GroupName, x.Authors?.
                    Select(m => new AuthorViewModel(m.AvatarUrl != null
                            ? new WebImageViewModel(_webImageCache)
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

        if (!HasLoadedFeedDetails)
            LoadFeedDetails(mod);

        LoadFullDetails(mod);
        UpdateCurrentImage();
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
        public AuthorViewModel(WebImageViewModel? avatar, string? name, string? role, string? profileUrl)
        {
            Avatar = avatar;
            Avatar?.Load();
            Name = name;
            Role = role;
            ProfileUrl = profileUrl;

            OpenUserPageCommand = new RelayCommand(OpenUserPage);
        }

        public ICommand OpenUserPageCommand { get; }

        public WebImageViewModel? Avatar { get; }
        public string? Name { get; }
        public string? Role { get; }
        public string? ProfileUrl { get; }

        public void OpenUserPage()
        {
            if (ProfileUrl != null)
                Services.App.OpenUrl(ProfileUrl);
        }
    }

    #endregion
}