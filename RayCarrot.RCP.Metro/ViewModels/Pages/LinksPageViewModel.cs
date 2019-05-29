using RayCarrot.CarrotFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    // TODO: Localize
    /// <summary>
    /// View model for the links page
    /// </summary>
    public class LinksPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LinksPageViewModel()
        {
            Refresh();

            RCF.Data.CultureChanged += (s, e) => Refresh();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The local link items
        /// </summary>
        public List<LinkItemViewModel[]> LocalLinkItems { get; set; }

        /// <summary>
        /// The community link items
        /// </summary>
        public LinkItemViewModel[][] CommunityLinkItems { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the link items
        /// </summary>
        public void Refresh()
        {
            LocalLinkItems = new List<LinkItemViewModel[]>();

            // ubi.ini files
            LocalLinkItems.Add(new LinkItemViewModel[]
            {
                new LinkItemViewModel(CommonPaths.UbiIniPath1, "Primary ubi.ini file"),
                new LinkItemViewModel(CommonPaths.UbiIniPath2, "Secondary ubi.ini file", UserLevel.Advanced),
                new LinkItemViewModel(Games.Rayman2.IsAdded() ? Games.Rayman2.GetInfo().InstallDirectory + "ubi.ini" : FileSystemPath.EmptyPath, "Rayman 2 ubi.ini file", UserLevel.Advanced),
            });

            // DOSBox files
            if (File.Exists(Data.DosBoxPath))
            {
                LocalLinkItems.Add(new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new FileSystemPath(Data.DosBoxPath), "DOSBox"),
                    new LinkItemViewModel(new FileSystemPath(Data.DosBoxConfig), "DOSBox configuration file", UserLevel.Technical),
                });
            }

            // Steam paths
            try
            {
                using (var key = RCFWinReg.RegistryManager.GetKeyFromFullPath(@"HKEY_CURRENT_USER\Software\Valve\Steam", RegistryView.Default))
                {
                    if (key != null)
                    {
                        FileSystemPath steamDir = key.GetValue("SteamPath", null)?.ToString();
                        string steamExe = key.GetValue("SteamExe", null)?.ToString();

                        if (steamDir.DirectoryExists)
                        {
                            LocalLinkItems.Add(new LinkItemViewModel[]
                            {
                                new LinkItemViewModel(steamDir + steamExe, "Steam"),
                                new LinkItemViewModel(steamDir + @"steamapps\common", "Steam games", UserLevel.Advanced),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Getting Steam Registry key");
            }

            // GOG paths
            try
            {
                using (var key = RCFWinReg.RegistryManager.GetKeyFromFullPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths", RegistryView.Default))
                {
                    if (key != null)
                    {
                        FileSystemPath gogDir = key.GetValue("client", null)?.ToString();

                        if (gogDir.DirectoryExists)
                        {
                            LocalLinkItems.Add(new LinkItemViewModel[]
                            {
                                new LinkItemViewModel(gogDir + "GalaxyClient.exe", "GOG Galaxy"),
                                new LinkItemViewModel(gogDir + @"Games", "GOG Galaxy games", UserLevel.Advanced),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Getting GOG Galaxy Registry key");
            }

            // Registry paths
            LocalLinkItems.Add(new LinkItemViewModel[]
            {
                new LinkItemViewModel(CommonPaths.RaymanRavingRabbidsRegistryKey, "Rayman Raving Rabbids settings", UserLevel.Technical), 
                new LinkItemViewModel(CommonPaths.RaymanOriginsRegistryKey, "Rayman Origins settings", UserLevel.Technical),
                new LinkItemViewModel(CommonPaths.RaymanLegendsRegistryKey, "Rayman Legends settings", UserLevel.Technical),
                new LinkItemViewModel(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide", "nGlide settings", UserLevel.Technical),
                new LinkItemViewModel(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide2", "nGlide 2.0 settings", UserLevel.Technical),
            });

            // Debug paths
            LocalLinkItems.Add(new LinkItemViewModel[]
            {
                new LinkItemViewModel(CommonPaths.UserDataBaseDir, "App user data", UserLevel.Technical),
                new LinkItemViewModel(CommonPaths.TempPath, "App temp directory", UserLevel.Debug),
                new LinkItemViewModel(CommonPaths.LogFile, "Log file", UserLevel.Debug),
                new LinkItemViewModel(CommonPaths.TPLSDir, "TPLS directory", UserLevel.Debug),
                new LinkItemViewModel(CommonPaths.RegistryBaseKey, "Registry app data", UserLevel.Technical),
            });

            // Community links
            CommunityLinkItems = new LinkItemViewModel[][]
            {
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://raymanpc.com/"), "Rayman Pirate-Community"),
                    new LinkItemViewModel(new Uri("https://raymanpc.com/wiki/en/Main_Page"), "RayWiki"),
                    new LinkItemViewModel(new Uri("https://raytunes.raymanpc.com/"), "RayTunes"),
                    new LinkItemViewModel(new Uri("https://raysaves.raymanpc.com/"), "RaySaves"),
                }, 
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("http://www.kmgassociates.com/rayman/index.html"), "KMG Associates - Rayman"),
                    new LinkItemViewModel(new Uri("http://www.rayman-fanpage.de/"), "Rayman Fanpage"),
                }, 
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://raym.app/"), "raym.app"),
                    new LinkItemViewModel(new Uri("https://raym.app/maps/"), "Raymap"),
                }, 
            };
        }

        #endregion
    }

    /// <summary>
    /// View model for a link item
    /// </summary>
    public class LinkItemViewModel : BaseRCPViewModel
    {
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
            LocalLinkPath = localLinkPath.CorrectPathCasing();
            IsLocal = true;
            DisplayText = displayText;
            IconKind = localLinkPath.FileExists ? PackIconMaterialKind.FileOutline : PackIconMaterialKind.FolderOutline;

            try
            {
                IconSource = LocalLinkPath.GetIconOrThumbnail(ShellThumbnailSize.Small).ToImageSource();
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
                IconSource = new FileSystemPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "regedit.exe")).GetIconOrThumbnail(ShellThumbnailSize.Small).ToImageSource();
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
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri($"{"https://www.google.com/s2/favicons?domain="}{ExternalLinkPath}"); ;
                bitmapImage.EndInit();
                IconSource = bitmapImage;
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Getting external link icon");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The minimum required user level for this link item
        /// </summary>
        public UserLevel MinUserLevel { get; }

        /// <summary>
        /// The icon image source
        /// </summary>
        public ImageSource IconSource { get; }

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
                    await RCF.MessageUI.DisplayMessageAsync("The link item could not be opened due to not being found", "Invalid link address", MessageType.Error);
            }
            else
            {
                App.OpenUrl(ExternalLinkPath.ToString());
            }
        }

        #endregion
    }
}