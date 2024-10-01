using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro.Games.Settings;

/// <summary>
/// View model for a link item
/// </summary>
public class LinkItemViewModel : BaseRCPViewModel
{
    #region Constructors

    public LinkItemViewModel(LinkType type, string linkPath)
    {
        Type = type;
        LinkPath = linkPath;

        if (type is LinkType.File or LinkType.BinaryFile or LinkType.Directory && IsValid)
            LinkPath = new FileSystemPath(LinkPath).CorrectPathCasing();

        IconKind = type switch
        {
            LinkType.File or LinkType.BinaryFile => PackIconMaterialKind.FileOutline,
            LinkType.Directory => PackIconMaterialKind.FolderOutline,
            LinkType.RegistryKey => PackIconMaterialKind.FileOutline,
            LinkType.Web => PackIconMaterialKind.AccessPointNetwork,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Static Properties

    /// <summary>
    /// The icon cache for the icon image sources
    /// </summary>
    private static Dictionary<string, ImageSource> IconCache { get; } = new();

    #endregion

    #region Commands

    public ICommand OpenLinkCommand { get; }

    #endregion

    #region Public Properties

    public LinkType Type { get; }

    /// <summary>
    /// The link path
    /// </summary>
    public string LinkPath { get; }

    /// <summary>
    /// The icon image source
    /// </summary>
    public ImageSource? IconSource { get; set; }

    /// <summary>
    /// The icon for the link
    /// </summary>
    public PackIconMaterialKind IconKind { get; }

    /// <summary>
    /// Indicates if the link is valid
    /// </summary>
    public bool IsValid => Type switch
    {
        LinkType.File or LinkType.BinaryFile => File.Exists(LinkPath),
        LinkType.Directory => Directory.Exists(LinkPath),
        LinkType.RegistryKey => RegistryHelpers.KeyExists(LinkPath),
        LinkType.Web => Uri.TryCreate(LinkPath, UriKind.Absolute, out _),
        _ => false
    };

    #endregion

    #region Private Static Methods

    /// <summary>
    /// Gets the image source for the specified path
    /// </summary>
    /// <param name="path">The path to get the image source for</param>
    /// <returns>The image source</returns>
    private static ImageSource GetImageSource(FileSystemPath path)
    {
        lock (IconCache)
        {
            if (IconCache.ContainsKey(path.FullPath))
                return IconCache[path.FullPath];

            ImageSource image = WindowsHelpers.GetIconOrThumbnail(path, ShellThumbnailSize.Small).ToImageSource();
            image.Freeze();

            IconCache.Add(path.FullPath, image);

            return image;
        }
    }

    #endregion

    #region Public Method

    public void LoadIcon()
    {
        try
        {
            switch (Type)
            {
                case LinkType.File:
                case LinkType.BinaryFile:
                case LinkType.Directory:
                    IconSource = GetImageSource(LinkPath);
                    break;

                case LinkType.RegistryKey:
                    IconSource = GetImageSource(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "regedit.exe"));
                    break;

                case LinkType.Web:
                    BitmapImage bitmapImage = new();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri("https://www.google.com/s2/favicons?domain=" + LinkPath);
                    bitmapImage.EndInit();
                    IconSource = bitmapImage;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting link item icon");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Opens the link
    /// </summary>
    public async Task OpenLinkAsync()
    {
        switch (Type)
        {
            case LinkType.File:
                await Services.File.LaunchFileAsync(LinkPath);
                break;

            case LinkType.BinaryFile: // Open in file explorer for now. Eventually maybe change to use hex editor.
            case LinkType.Directory:
                await Services.File.OpenExplorerLocationAsync(LinkPath);
                break;

            case LinkType.RegistryKey:
                await Services.File.OpenRegistryKeyAsync(LinkPath);
                break;

            case LinkType.Web:
                App.OpenUrl(LinkPath);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Enums

    public enum LinkType
    {
        File,
        BinaryFile,
        Directory,
        RegistryKey,
        Web,
    }

    #endregion
}