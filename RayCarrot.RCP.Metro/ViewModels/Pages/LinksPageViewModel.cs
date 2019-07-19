using Microsoft.Win32;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Windows.Registry;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
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
            LocalLinkItems = new ObservableCollection<LinkItemViewModel[]>();
            CommunityLinkItems = new ObservableCollection<LinkItemViewModel[]>();
            ForumLinkItems = new ObservableCollection<LinkItemViewModel[]>();
            ToolsLinkItems = new ObservableCollection<LinkItemViewModel[]>();

            Refresh();

            RCFCore.Data.CultureChanged += (s, e) => Refresh();

            RefreshCommand = new RelayCommand(Refresh);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The local link items
        /// </summary>
        public ObservableCollection<LinkItemViewModel[]> LocalLinkItems { get; }

        /// <summary>
        /// The community link items
        /// </summary>
        public ObservableCollection<LinkItemViewModel[]> CommunityLinkItems { get; }

        /// <summary>
        /// The forum link items
        /// </summary>
        public ObservableCollection<LinkItemViewModel[]> ForumLinkItems { get; }

        /// <summary>
        /// The tools link items
        /// </summary>
        public ObservableCollection<LinkItemViewModel[]> ToolsLinkItems { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the link items
        /// </summary>
        public void Refresh()
        {
            LocalLinkItems.Clear();

            // ubi.ini files
            LocalLinkItems.Add(new LinkItemViewModel[]
            {
                new LinkItemViewModel(CommonPaths.UbiIniPath1, Resources.Links_Local_PrimaryUbiIni),
                new LinkItemViewModel(CommonPaths.UbiIniPath2, Resources.Links_Local_SecondaryUbiIni, UserLevel.Advanced),
                new LinkItemViewModel(Games.Rayman2.IsAdded()
                        ? Games.Rayman2.GetInfo().InstallDirectory + "ubi.ini"
                        : FileSystemPath.EmptyPath, Resources.Links_Local_R2UbiIni, UserLevel.Advanced)
            });

            // DOSBox files
            if (File.Exists(Data.DosBoxPath))
                LocalLinkItems.Add(new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new FileSystemPath(Data.DosBoxPath), Resources.Links_Local_DOSBox),
                    new LinkItemViewModel(new FileSystemPath(Data.DosBoxConfig), Resources.Links_Local_DOSBoxConfig, UserLevel.Technical)
                });

            // Steam paths
            try
            {
                using (RegistryKey key =
                    RCFWinReg.RegistryManager.GetKeyFromFullPath(@"HKEY_CURRENT_USER\Software\Valve\Steam", RegistryView.Default))
                {
                    if (key != null)
                    {
                        FileSystemPath steamDir = key.GetValue("SteamPath", null)?.ToString();
                        var steamExe = key.GetValue("SteamExe", null)?.ToString();

                        if (steamDir.DirectoryExists)
                            LocalLinkItems.Add(new LinkItemViewModel[]
                            {
                                new LinkItemViewModel(steamDir + steamExe, Resources.Links_Local_Steam),
                                new LinkItemViewModel(steamDir + @"steamapps\common", Resources.Links_Local_SteamGames, UserLevel.Advanced)
                            });
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
                using (RegistryKey key = RCFWinReg.RegistryManager.GetKeyFromFullPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths", RegistryView.Default))
                {
                    if (key != null)
                    {
                        FileSystemPath gogDir = key.GetValue("client", null)?.ToString();

                        if (gogDir.DirectoryExists)
                            LocalLinkItems.Add(new LinkItemViewModel[]
                            {
                                new LinkItemViewModel(gogDir + "GalaxyClient.exe", Resources.Links_Local_GOGClient),
                                new LinkItemViewModel(gogDir + @"Games", Resources.Links_Local_GOGGames, UserLevel.Advanced)
                            });
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
                new LinkItemViewModel(CommonPaths.RaymanRavingRabbidsRegistryKey, Resources.Links_Local_RRRRegSettings, UserLevel.Technical),
                new LinkItemViewModel(CommonPaths.RaymanOriginsRegistryKey, Resources.Links_Local_RORegSettings, UserLevel.Technical),
                new LinkItemViewModel(CommonPaths.RaymanLegendsRegistryKey, Resources.Links_Local_RLRegSettings, UserLevel.Technical),
                new LinkItemViewModel(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide", Resources.Links_Local_nGlideRegSettings, UserLevel.Technical),
                new LinkItemViewModel(@"HKEY_CURRENT_USER\Software\Zeus Software\nGlide2", Resources.Links_Local_nGlide2RegSettings, UserLevel.Technical)
            });

            // Debug paths
            LocalLinkItems.Add(new LinkItemViewModel[]
            {
                new LinkItemViewModel(CommonPaths.UserDataBaseDir, Resources.Links_Local_AppData, UserLevel.Technical),
                new LinkItemViewModel(CommonPaths.TempPath, Resources.Links_Local_TempDir, UserLevel.Debug),
                new LinkItemViewModel(CommonPaths.LogFile, Resources.Links_Local_LogFile, UserLevel.Debug),
                new LinkItemViewModel(CommonPaths.UtilitiesBaseDir, Resources.Links_Local_Utilities, UserLevel.Debug),
                new LinkItemViewModel(CommonPaths.RegistryBaseKey, Resources.Links_Local_RegAppData, UserLevel.Technical)
            });

            // Community links
            CommunityLinkItems.Clear();

            CommunityLinkItems.AddRange(new LinkItemViewModel[][]
            {
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://raymanpc.com/"), Resources.Links_Community_RPC),
                    new LinkItemViewModel(new Uri("https://raymanpc.com/wiki/en/Main_Page"), Resources.Links_Community_RayWiki),
                    new LinkItemViewModel(new Uri("https://raytunes.raymanpc.com/"), Resources.Links_Community_RayTunes),
                    new LinkItemViewModel(new Uri("https://raysaves.raymanpc.com/"), Resources.Links_Community_RaySaves)
                },
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://twitter.com/RaymanCentral"), Resources.Links_Community_RaymanCentral),
                    new LinkItemViewModel(new Uri("https://twitter.com/RaymanTogether"), Resources.Links_Community_RaymanTogether),
                },
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://raym.app/"), Resources.Links_Community_raym_app),
                    new LinkItemViewModel(new Uri("https://raym.app/maps/"), Resources.Links_Community_Raymap),
                    new LinkItemViewModel(new Uri("https://raym.app/menezis/"), Resources.Links_Community_Menezis_Browser),
                },
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("http://www.kmgassociates.com/rayman/index.html"), Resources.Links_Community_KMG),
                    new LinkItemViewModel(new Uri("http://www.rayman-fanpage.de/"), Resources.Links_Community_Fanpage)
                },
            });

            // Forum links
            ForumLinkItems.Clear();

            ForumLinkItems.AddRange(new LinkItemViewModel[][]
            {
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://raymanpc.com/forum/index.php"), Resources.Links_Forums_RPC), 
                }, 
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://forums.ubi.com/forumdisplay.php/47-Rayman"), Resources.Links_Forums_Ubisoft), 
                    new LinkItemViewModel(new Uri("https://www.gog.com/forum/rayman_series"), Resources.Links_Forums_GOG), 
                }, 
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://steamcommunity.com/app/15060/discussions/"), Resources.Links_Forums_Steam_R2), 
                    new LinkItemViewModel(new Uri("https://steamcommunity.com/app/15080/discussions/"), Resources.Links_Forums_Steam_RRR), 
                    new LinkItemViewModel(new Uri("https://steamcommunity.com/app/207490/discussions/"), Resources.Links_Forums_Steam_RO), 
                    new LinkItemViewModel(new Uri("https://steamcommunity.com/app/242550/discussions/"), Resources.Links_Forums_Steam_RL), 
                }, 
            });

            // Tools links
            ToolsLinkItems.Clear();

            ToolsLinkItems.AddRange(new LinkItemViewModel[][]
            {
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://raymanpc.com/forum/viewtopic.php?t=5755"), Resources.Links_Tools_RDEditor), 
                    new LinkItemViewModel(new Uri("https://raymanpc.com/forum/viewtopic.php?t=25867"), Resources.Links_Tools_RayPlus), 
                },
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://raymanpc.com/forum/viewtopic.php?t=25013"), Resources.Links_Tools_RayTwol), 
                    new LinkItemViewModel(new Uri("https://raymanpc.com/forum/viewtopic.php?t=23423"), Resources.Links_Tools_R2Tools), 
                    new LinkItemViewModel(new Uri("https://raymanpc.com/forum/viewtopic.php?t=23420"), Resources.Links_Tools_R2Moonjump), 
                    new LinkItemViewModel(new Uri("https://github.com/rtsonneveld/Rayman2FunBox/releases"), Resources.Links_Tools_R2FunBox), 
                }, 
                new LinkItemViewModel[]
                {
                    new LinkItemViewModel(new Uri("https://raymanpc.com/forum/viewtopic.php?t=12854"), Resources.Links_Tools_BetterR3), 
                    new LinkItemViewModel(new Uri("https://raymanpc.com/forum/viewtopic.php?t=25053"), Resources.Links_Tools_R3GCTexturePack), 
                }, 
            });
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }

        #endregion
    }
}