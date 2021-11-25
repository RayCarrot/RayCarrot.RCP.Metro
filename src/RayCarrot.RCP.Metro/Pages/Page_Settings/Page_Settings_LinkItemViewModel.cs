#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;
using RayCarrot.IO;
using NLog;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a link item
/// </summary>
public class Page_Settings_LinkItemViewModel : BaseRCPViewModel
{
    #region Static Constructor

    static Page_Settings_LinkItemViewModel()
    {
        IconCache = new Dictionary<string, ImageSource>();
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new link item for a local path
    /// </summary>
    /// <param name="localLinkPath">The local link path</param>
    /// <param name="displayText">The text to display for the link</param>
    /// <param name="minUserLevel">The minimum required user level for this link item</param>
    public Page_Settings_LinkItemViewModel(FileSystemPath localLinkPath, string displayText, UserLevel minUserLevel = UserLevel.Normal)
    {
        MinUserLevel = minUserLevel;
        LocalLinkPath = localLinkPath.Exists ? localLinkPath.CorrectPathCasing() : localLinkPath;
        IsLocal = true;
        DisplayText = displayText;
        IconKind = localLinkPath.FileExists ? PackIconMaterialKind.FileOutline : PackIconMaterialKind.FolderOutline;

        if (!IsValid)
            return;

        try
        {
            IconSource = GetImageSource(LocalLinkPath);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting link item thumbnail");
        }

        OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);
    }

    /// <summary>
    /// Creates a new link item for a Registry path
    /// </summary>
    /// <param name="registryLinkPath">The Registry link path</param>
    /// <param name="displayText">The text to display for the link</param>
    /// <param name="minUserLevel">The minimum required user level for this link item</param>
    public Page_Settings_LinkItemViewModel(string registryLinkPath, string displayText, UserLevel minUserLevel = UserLevel.Normal)
    {
        MinUserLevel = minUserLevel;
        RegistryLinkPath = registryLinkPath;
        IsLocal = true;
        IsRegistryPath = true;
        DisplayText = displayText;
        IconKind = PackIconMaterialKind.FileOutline;

        try
        {
            IconSource = GetImageSource(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "regedit.exe"));
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting Registry link item thumbnail");
        }

        OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);
    }

    /// <summary>
    /// Creates a new link item for an external path
    /// </summary>
    /// <param name="externalLinkPath">The external link path</param>
    /// <param name="displayText">The text to display for the link</param>
    /// <param name="minUserLevel">The minimum required user level for this link item</param>
    public Page_Settings_LinkItemViewModel(Uri externalLinkPath, string displayText, UserLevel minUserLevel = UserLevel.Normal)
    {
        MinUserLevel = minUserLevel;
        LocalLinkPath = FileSystemPath.EmptyPath;
        ExternalLinkPath = externalLinkPath;
        DisplayText = displayText;
        IconKind = PackIconMaterialKind.AccessPointNetwork;

        OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);

        try
        {
            // ReSharper disable once PossibleNullReferenceException
            Application.Current.Dispatcher.Invoke(() =>
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri("https://www.google.com/s2/favicons?domain=" + ExternalLinkPath);
                bitmapImage.EndInit();
                IconSource = bitmapImage;
            });
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting external link icon");
        }
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Static Properties

    /// <summary>
    /// The icon cache for the icon image sources
    /// </summary>
    private static Dictionary<string, ImageSource> IconCache { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The minimum required user level for this link item
    /// </summary>
    public UserLevel MinUserLevel { get; }

    /// <summary>
    /// The icon image source
    /// </summary>
    public ImageSource IconSource { get; set; }

    /// <summary>
    /// The local link path
    /// </summary>
    public FileSystemPath LocalLinkPath { get; }

    /// <summary>
    /// The external link path
    /// </summary>
    public Uri ExternalLinkPath { get; }

    /// <summary>
    /// Indicates if the path is local or external
    /// </summary>
    public bool IsLocal { get; }

    /// <summary>
    /// Indicates if the link is to a Registry path
    /// </summary>
    public bool IsRegistryPath { get; }

    /// <summary>
    /// The Registry link path
    /// </summary>
    public string RegistryLinkPath { get; }

    /// <summary>
    /// The text to display for the link
    /// </summary>
    public string DisplayText { get; }

    /// <summary>
    /// The icon for the link
    /// </summary>
    public PackIconMaterialKind IconKind { get; }

    /// <summary>
    /// The path to display
    /// </summary>
    public string DisplayPath => !IsLocal ? ExternalLinkPath?.ToString() : IsRegistryPath ? RegistryLinkPath : LocalLinkPath.FullPath;

    /// <summary>
    /// Indicates if the link is valid
    /// </summary>
    public bool IsValid => !IsLocal || (IsRegistryPath ? RegistryHelpers.KeyExists(RegistryLinkPath) : LocalLinkPath.Exists);

    #endregion

    #region Commands

    public ICommand OpenLinkCommand { get; }

    #endregion

    #region Private Static Methods

    /// <summary>
    /// Gets the image source for the specified path
    /// </summary>
    /// <param name="path">The path to get the image source for</param>
    /// <returns>The image source</returns>
    private static ImageSource GetImageSource(FileSystemPath path)
    {
        if (IconCache.ContainsKey(path.FullPath))
            return IconCache[path.FullPath];

        // ReSharper disable once PossibleNullReferenceException
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var image = WindowsHelpers.GetIconOrThumbnail(path, ShellThumbnailSize.Small).ToImageSource();

            image.Freeze();

            Logger.Debug("The link item image source has been created for the path '{0}'", path);

            IconCache.Add(path.FullPath, image);

            return image;
        });
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Opens the link
    /// </summary>
    public async Task OpenLinkAsync()
    {
        if (IsLocal)
        {
            if (IsRegistryPath)
                await Services.File.OpenRegistryKeyAsync(RegistryLinkPath);

            else if (LocalLinkPath.FileExists)
                await Services.File.LaunchFileAsync(LocalLinkPath);

            else if (LocalLinkPath.DirectoryExists)
                await Services.File.OpenExplorerLocationAsync(LocalLinkPath);

            else
                await Services.MessageUI.DisplayMessageAsync(Resources.Links_OpenErrorNotFound, Resources.Links_OpenErrorNotFoundHeader, MessageType.Error);
        }
        else
        {
            App.OpenUrl(ExternalLinkPath.ToString());
        }
    }

    #endregion
}