using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a link item
    /// </summary>
    public class LinkItemViewModel : BaseRCPViewModel
    {
        #region Static Constructor

        static LinkItemViewModel()
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
        public LinkItemViewModel(FileSystemPath localLinkPath, string displayText, UserLevel minUserLevel = UserLevel.Normal)
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
                ex.HandleUnexpected("Getting link item thumbnail");
            }

            OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);
        }

        /// <summary>
        /// Creates a new link item for a Registry path
        /// </summary>
        /// <param name="registryLinkPath">The Registry link path</param>
        /// <param name="displayText">The text to display for the link</param>
        /// <param name="minUserLevel">The minimum required user level for this link item</param>
        public LinkItemViewModel(string registryLinkPath, string displayText, UserLevel minUserLevel = UserLevel.Normal)
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
                ex.HandleUnexpected("Getting Registry link item thumbnail");
            }

            OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);
        }

        /// <summary>
        /// Creates a new link item for an external path
        /// </summary>
        /// <param name="externalLinkPath">The external link path</param>
        /// <param name="displayText">The text to display for the link</param>
        /// <param name="minUserLevel">The minimum required user level for this link item</param>
        public LinkItemViewModel(Uri externalLinkPath, string displayText, UserLevel minUserLevel = UserLevel.Normal)
        {
            MinUserLevel = minUserLevel;
            LocalLinkPath = FileSystemPath.EmptyPath;
            ExternalLinkPath = externalLinkPath;
            DisplayText = displayText;
            IconKind = PackIconMaterialKind.AccessPointNetwork;

            OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);

            try
            {
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
                ex.HandleUnexpected("Getting external link icon");
            }
        }

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
        public bool IsValid => !IsLocal || (IsRegistryPath ? RCFWinReg.RegistryManager.KeyExists(RegistryLinkPath) : LocalLinkPath.Exists);

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

            var image = path.GetIconOrThumbnail(ShellThumbnailSize.Small).ToImageSource();

            image.Freeze();

            RCFCore.Logger?.LogDebugSource($"The link item image source has been created for the path '{path}'");

            IconCache.Add(path.FullPath, image);

            return image;
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
                    await RCFRCP.File.OpenRegistryKeyAsync(RegistryLinkPath);

                else if (LocalLinkPath.FileExists)
                    await RCFRCP.File.LaunchFileAsync(LocalLinkPath);

                else if (LocalLinkPath.DirectoryExists)
                    await RCFRCP.File.OpenExplorerLocationAsync(LocalLinkPath);

                else
                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.Links_OpenErrorNotFound, Resources.Links_OpenErrorNotFoundHeader, MessageType.Error);
            }
            else
            {
                App.OpenUrl(ExternalLinkPath.ToString());
            }
        }

        #endregion
    }
}