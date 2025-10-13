using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadedModViewModel : BaseViewModel
{
    #region Constructor

    public DownloadedModViewModel(GameInstallation gameInstallation, DownloadableModsSource? downloadableModsSource, Mod mod, ModManifestEntry modEntry)
    {
        GameInstallation = gameInstallation;
        Mod = mod;
        InstallInfo = modEntry.InstallInfo;

        ModInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_Author)), Metadata.Author),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_Version)), Metadata.Version?.ToString()),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_FormatVersion)), Metadata.Format.ToString(), UserLevel.Technical),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_ID)), Metadata.Id, UserLevel.Technical),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_Size)), BinaryHelpers.BytesToString(modEntry.InstallInfo.Size)),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_InstallSource)), downloadableModsSource?.DisplayName ?? new ResourceLocString(nameof(Resources.ModLoader_LocalInstallSource))),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_Modules)), Mod.GetSupportedModules().JoinItems(", ", x => x.Id)),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_AddedFiles)), mod.GetAddedFiles().Count.ToString()),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_RemovedFiles)), mod.GetRemovedFiles().Count.ToString()),
            new(new ResourceLocString(nameof(Resources.ModLoader_ModInfo_PatchedFiles)), mod.GetPatchedFiles().Count.ToString()),
        };

        ReadOnlyCollection<string> unsupportedModules = Mod.GetUnsupportedModules();
        if (unsupportedModules.Any())
            UnsupportedModulesErrorMessage = new ResourceLocString(nameof(Resources.ModLoader_UnsupportedModulesInfo), unsupportedModules.JoinItems(", "));

        ChangelogEntries = new ObservableCollection<ModChangelogEntry>(Metadata.Changelog ?? Array.Empty<ModChangelogEntry>());

        CanOpenInDownloadPage = true; // TODO-UPDATE: Implement

        OpenWebsiteCommand = new RelayCommand(OpenWebsite);
        OpenInDownloadPageCommand = new RelayCommand(OpenInDownloadPage);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand OpenWebsiteCommand { get; }
    public ICommand OpenInDownloadPageCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public Mod Mod { get; }
    public ModInstallInfo InstallInfo { get; }
    public ModMetadata Metadata => Mod.Metadata;
    public ObservableCollection<DuoGridItemViewModel> ModInfo { get; }
    public ObservableCollection<ModChangelogEntry> ChangelogEntries { get; }

    public LocalizedString? UnsupportedModulesErrorMessage { get; }

    public string? Name => Metadata.Name;
    public string? Description => Metadata.Description;
    public string? Website => Metadata.Website;

    public bool HasWebsite => Uri.TryCreate(Metadata.Website, UriKind.Absolute, out _);
    public bool HasDescripton => !Metadata.Description.IsNullOrWhiteSpace();
    public ImageSource? Thumbnail { get; set; }

    public bool CanOpenInDownloadPage { get; }

    #endregion

    #region Public Methods

    public void LoadThumbnail()
    {
        Logger.Trace("Loading thumbnail for mod with ID {0}", Metadata.Id);

        try
        {
            FileSystemPath? thumbFilePath = Mod.GetThumbnailFilePath();

            if (thumbFilePath == null)
            {
                Thumbnail = null;
                return;
            }

            BitmapImage thumb = new();
            thumb.BeginInit();
            thumb.CreateOptions |= BitmapCreateOptions.IgnoreImageCache;
            thumb.CacheOption = BitmapCacheOption.OnLoad; // Required to allow the file to be deleted, such as if a temp file
            thumb.UriSource = new Uri(thumbFilePath);
            thumb.EndInit();

            if (thumb.CanFreeze)
                thumb.Freeze();

            Thumbnail = thumb;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading mod thumbnail");
        }
    }

    public void OpenWebsite()
    {
        if (Metadata.Website != null)
            Services.App.OpenUrl(Metadata.Website);
    }

    public void OpenInDownloadPage()
    {
        Services.Messenger.Send(new OpenModDownloadPageMessage(GameInstallation, InstallInfo));
    }

    #endregion
}