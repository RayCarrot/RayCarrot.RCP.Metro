using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaDownloadableModViewModel : DownloadableModViewModel, IRecipient<ModInstalledMessage>
{
    #region Constructor

    public GameBananaDownloadableModViewModel(
        GameBananaModsSource downloadableModsSource,
        ModLoaderViewModel modLoaderViewModel,
        HttpClient httpClient,
        int gameBananaId, 
        bool isFeatured)
    {
        _downloadableModsSource = downloadableModsSource;
        _modLoaderViewModel = modLoaderViewModel;
        _httpClient = httpClient;

        GameBananaId = gameBananaId;
        IsFeatured = isFeatured;

        // Placeholder image
        Images = [new ImageViewModel()];
        HasLoadedImages = false;

        ShowArchivedFiles = false;

        HasViewed = Services.Data.ModLoader_ViewedMods.TryGetValue(downloadableModsSource.Id, out List<ViewedMod> viewedMod) &&
                    viewedMod.Any(x => x.Id == GameBananaId.ToString());

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
    private readonly GameBananaModsSource _downloadableModsSource;
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

    public string? RootCategoryIconUrl { get; set; }
    public string? RootCategoryName { get; set; }

    public ImageViewModel? MainImage { get; set; }
    public double MainImageWidth { get; set; }
    public double MainImageHeight { get; set; }
    public ObservableCollection<ImageViewModel> Images { get; }
    public bool HasLoadedImages { get; set; }
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

    public bool IsLoaded { get; set; }
    public bool HasViewed { get; set; }

    #endregion

    #region Private Methods

    private void UpdateCurrentImage()
    {
        if (!HasLoadedImages)
            return;

        // Load the current image
        Images[SelectedImageIndex].Load();
        
        // Preload the next image
        if (SelectedImageIndex + 1 < Images.Count)
        {
            ImageViewModel nextImg = Images[SelectedImageIndex + 1];
            nextImg.Load();
        }
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
            source: _downloadableModsSource, 
            fileName: file.DownloadableFile.File, 
            downloadUrl: file.DownloadableFile.DownloadUrl, 
            fileSize: file.DownloadableFile.FileSize, 
            installData: new GameBananaInstallData(GameBananaId, file.DownloadableFile.Id));
    }

    public void LoadDetailsFromMod(GameBananaMod mod)
    {
        if (mod.Id != GameBananaId)
            throw new Exception($"GameBanana mod IDs {mod.Id} and {GameBananaId} do not match!");

        if (Name == null)
            Name = mod.Name;

        if (mod.Description != null && Description == null)
            Description = mod.Description;

        if (mod.Text != null && Text == null)
            Text = mod.Text;

        if (mod.Submitter != null && SubmitterName == null)
        {
            SubmitterName = mod.Submitter.Name;
            SubmitterProfileUrl = mod.Submitter.ProfileUrl;
        }

        if (mod.DateAdded != null)
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

        if (mod.Version != null && Version == null)
            Version = ModVersion.TryParse(mod.Version, out ModVersion? v) ? v : null;

        if (mod.RootCategory != null)
        {
            if (!mod.RootCategory.IconUrl.IsNullOrEmpty())
                RootCategoryIconUrl = mod.RootCategory.IconUrl;

            RootCategoryName = mod.RootCategory.Name;
        }

        if (mod.PreviewMedia?.Images != null)
        {
            if (mod.PreviewMedia.Images.Length > 0 && MainImage == null)
            {
                GameBananaImage mainImage = mod.PreviewMedia.Images[0];
                MainImage = new ImageViewModel() { Url = $"{mainImage.BaseUrl}/{mainImage.File220}" };
                MainImage.Load();
                MainImageWidth = mainImage.File220Width;
                MainImageHeight = mainImage.File220Height;
            }

            if (mod.PreviewMedia.Images.Length > 1 && !HasLoadedImages)
            {
                // Replace placeholder image
                Images[0].Url = $"{mod.PreviewMedia.Images[0].BaseUrl}/{mod.PreviewMedia.Images[0].File}";
                Images[0].DecodePixelHeight = ImageHeight;

                // Add remaining images
                foreach (GameBananaImage img in mod.PreviewMedia.Images.Skip(2).Take(MaxImages - 1))
                    Images.Add(new ImageViewModel()
                    {
                        Url = $"{img.BaseUrl}/{img.File}", 
                        DecodePixelHeight = ImageHeight
                    });

                HasLoadedImages = true;
            }
        }

        if (mod.LikeCount != null && LikesCount == null)
            LikesCount = mod.LikeCount;
        if (mod.DownloadCount != null && DownloadsCount == null)
            DownloadsCount = mod.DownloadCount;
        if (mod.ViewCount != null && ViewsCount == null)
            ViewsCount = mod.ViewCount;

        // TODO-UPDATE: Filter out valid files (i.e. ones with 1-click mod manager)
        if (mod.Files != null && Files == null)
        {
            ModViewModel? findDownloadedMod(GameBananaFile file) => _modLoaderViewModel.Mods.
                Where(x => x.DownloadableModsSource?.Id == _downloadableModsSource.Id).
                FirstOrDefault(x => x.InstallInfo.GetRequiredInstallData<GameBananaInstallData>().FileId == file.Id);

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

        if (mod.Credits != null && Credits == null)
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
                        m.Name, m.Role)).
                    ToObservableCollection())).
                ToObservableCollection();
        }
    }

    public override async Task LoadAsync()
    {
        await base.LoadAsync();

        if (IsLoaded)
            return;

        IsLoaded = true;

        // Mark as viewed
        HasViewed = true;
        if (!Services.Data.ModLoader_ViewedMods.ContainsKey(_downloadableModsSource.Id))
            Services.Data.ModLoader_ViewedMods.Add(_downloadableModsSource.Id, new List<ViewedMod>());
        Services.Data.ModLoader_ViewedMods[_downloadableModsSource.Id].Add(new ViewedMod(GameBananaId.ToString(), DateTime.Now, Version));

        // TODO-UPDATE: Catch exceptions
        // Load info
        GameBananaMod mod = await _downloadableModsSource.LoadModDetailsAsync(_httpClient, GameBananaId);
        LoadDetailsFromMod(mod);
        UpdateCurrentImage(); // TODO-UPDATE: Should this be here?
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
        if (message.ModViewModel.DownloadableModsSource?.Id != _downloadableModsSource.Id ||
            Files == null)
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
        public AuthorViewModel(ImageViewModel? avatar, string? name, string? role)
        {
            Avatar = avatar;
            Avatar?.Load();
            Name = name;
            Role = role;
        }

        public ImageViewModel? Avatar { get; }
        public string? Name { get; }
        public string? Role { get; }
    }

    public class ImageViewModel : BaseViewModel
    {
        public string? Url { get; set; }
        public int DecodePixelWidth { get; set; }
        public int DecodePixelHeight { get; set; }

        public ImageSource? ImageSource { get; set; }
        public bool IsLoadingImage { get; set; } = true;

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
            imgSource.DownloadCompleted += (_, _) => IsLoadingImage = false;
            imgSource.DecodePixelWidth = DecodePixelWidth;
            imgSource.DecodePixelHeight = DecodePixelHeight;
            imgSource.UriSource = uri;
            imgSource.EndInit();

            ImageSource = imgSource;
        }
    }

    #endregion
}