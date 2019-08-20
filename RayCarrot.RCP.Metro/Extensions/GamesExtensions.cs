using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using Package = Windows.ApplicationModel.Package;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="Games"/>
    /// </summary>
    public static class GamesExtensions
    {
        #region Helpers

        /// <summary>
        /// Gets the <see cref="BaseGameManager"/> for the added game
        /// </summary>
        /// <param name="game">The game to get the manager for</param>
        /// <param name="gameType">The game type, or null to use the current one if the game has been added</param>
        /// <returns>The game manager</returns>
        public static BaseGameManager GetGameManager(this Games game, GameType? gameType = null)
        {
            switch (gameType ?? game.GetInfo().GameType)
            {
                case GameType.Win32:
                    return new Win32GameManager(game);

                case GameType.Steam:
                    return new SteamGameManager(game);

                case GameType.WinStore:
                    return new WinStoreGameManager(game);

                case GameType.DosBox:
                    return new DOSBoxGameManager(game);

                case GameType.EducationalDosBox:
                    return new EducationalDosBoxGameManager(game); 

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the <see cref="BaseGameManager"/> for the added game
        /// </summary>
        /// <param name="game">The game to get the manager for</param>
        /// <returns>The game manager</returns>
        public static T GetGameManager<T>(this Games game)
            where T : BaseGameManager
        {
            return typeof(T).CreateInstance(game).CastTo<T>();
        }

        /// <summary>
        /// Gets the saved game info for the specified game
        /// </summary>
        /// <param name="game">The game to get the saved game info for</param>
        /// <returns>The saved game info</returns>
        public static GameInfo GetInfo(this Games game)
        {
            return RCFRCP.Data.Games[game];
        }

        /// <summary>
        /// Determines if the specified game has been added to the program
        /// </summary>
        /// <param name="game">The game to check if it's added</param>
        /// <returns>True if the game has been added, otherwise false</returns>
        public static bool IsAdded(this Games game)
        {
            return RCFRCP.Data.Games.ContainsKey(game);
        }

        /// <summary>
        /// Gets the backup directory for the specified game
        /// </summary>
        /// <param name="game">The game to get the backup directory for</param>
        /// <returns>The backup directory</returns>
        public static FileSystemPath GetBackupDir(this Games game)
        {
            return RCFRCP.Data.BackupLocation + AppViewModel.BackupFamily + game.GetBackupName();
        }

        /// <summary>
        /// Gets the backup file for the specified game if the backup is compressed
        /// </summary>
        /// <param name="game">The game to get the backup file for</param>
        /// <returns>The backup file</returns>
        public static FileSystemPath GetCompressedBackupFile(this Games game)
        {
            return RCFRCP.Data.BackupLocation + AppViewModel.BackupFamily + (game.GetBackupName() + CommonPaths.BackupCompressionExtension);
        }

        /// <summary>
        /// Gets the existing backup location for the specified game if one exists
        /// </summary>
        /// <param name="game">The game to get the backup location for</param>
        /// <returns>The backup location or null if none was found</returns>
        public static FileSystemPath? GetExistingBackup(this Games game)
        {
            // Get the backup locations
            var compressedLocation = game.GetCompressedBackupFile();
            var normalLocation = game.GetBackupDir();

            if (RCFRCP.Data.CompressBackups)
            {
                // Start by checking the location based on current setting
                if (compressedLocation.FileExists)
                    return compressedLocation;
                // Fall back to secondary location
                else if (normalLocation.DirectoryExists && Directory.GetFileSystemEntries(normalLocation).Any())
                    return normalLocation;
                else
                    // No valid location exists
                    return null;
            }
            else
            {
                // Start by checking the location based on current setting
                if (normalLocation.DirectoryExists && Directory.GetFileSystemEntries(normalLocation).Any())
                    return normalLocation;
                // Fall back to secondary location
                else if (compressedLocation.FileExists)
                    return compressedLocation;
                else
                    // No valid location exists
                    return null;
            }
        }

        /// <summary>
        /// Gets the installer items for the specified game
        /// </summary>
        /// <param name="game">The game to get the installer items for</param>
        /// <param name="outputPath">The output path for the installation</param>
        /// <returns>The installer items</returns>
        public static List<RayGameInstallItem> GetInstallerItems(this Games game, FileSystemPath outputPath)
        {
            // Create the result
            var result = new List<RayGameInstallItem>();

            // Attempt to get the text file
            if (!(InstallerGames.ResourceManager.GetObject($"{game}") is string file))
                throw new Exception("Installer item not found");

            using (StringReader reader = new StringReader(file))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    // Check if the item is optional, in which case
                    // it has a blank space before the path
                    bool optional = line.StartsWith(" ");

                    // Remove the blank space if optional
                    if (optional)
                        line = line.Substring(1);

                    result.Add(new RayGameInstallItem(line, outputPath + line, optional));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the DosBox configuration file path for the auto config for the specific game
        /// </summary>
        /// <param name="game">The game to get the file path for</param>
        /// <returns>The file path</returns>
        public static FileSystemPath GetDosBoxConfigFile(this Games game)
        {
            return CommonPaths.UserDataBaseDir + "DosBox" + (game + ".ini");
        }

        /// <summary>
        /// Gets the icon source for the specified game
        /// </summary>
        /// <param name="game">The game to get the icon source for</param>
        /// <returns>The icon source</returns>
        public static string GetIconSource(this Games game)
        {
            return $"{AppViewModel.ApplicationBasePath}Img/GameIcons/{game}.png";
        }

        #endregion

        #region Data

        /// <summary>
        /// Gets the display name for the specified game
        /// </summary>
        /// <param name="game">The game to get the display name for</param>
        /// <returns>The display name</returns>
        public static string GetDisplayName(this Games game)
        {
            // NOTE: Not localized

            switch (game)
            {
                case Games.Rayman1:
                    return "Rayman";

                case Games.RaymanDesigner:
                    return "Rayman Designer";

                case Games.RaymanByHisFans:
                    return "Rayman by his Fans";

                case Games.Rayman60Levels:
                    return "Rayman 60 Levels";

                case Games.Rayman2:
                    return "Rayman 2";

                case Games.RaymanM:
                    return "Rayman M";

                case Games.RaymanArena:
                    return "Rayman Arena";

                case Games.Rayman3:
                    return "Rayman 3";

                case Games.RaymanRavingRabbids:
                    return "Rayman Raving Rabbids";

                case Games.RaymanOrigins:
                    return "Rayman Origins";

                case Games.RaymanLegends:
                    return "Rayman Legends";

                case Games.RaymanJungleRun:
                    return "Rayman Jungle Run";

                case Games.RaymanFiestaRun:
                    return "Rayman Fiesta Run";

                case Games.RabbidsBigBang:
                    return "Rabbids Big Bang";

                case Games.EducationalDos:
                    return "Educational Games";

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets the backup name for the specified game
        /// </summary>
        /// <param name="game">The game to get the backup name for</param>
        /// <returns>The backup name</returns>
        public static string GetBackupName(this Games game)
        {
            // NOTE: These string can not be localized due to legacy support

            switch (game)
            {
                case Games.Rayman1:
                    return "Rayman 1";

                case Games.RaymanDesigner:
                    return "Rayman Designer";

                case Games.RaymanByHisFans:
                    return "Rayman by his Fans";

                case Games.Rayman60Levels:
                    return "Rayman 60 Levels";

                case Games.Rayman2:
                    return "Rayman 2";

                case Games.RaymanM:
                    return "Rayman M";

                case Games.RaymanArena:
                    return "Rayman Arena";

                case Games.Rayman3:
                    return "Rayman 3";

                case Games.RaymanRavingRabbids:
                    return "Rayman Raving Rabbids";

                case Games.RaymanOrigins:
                    return "Rayman Origins";

                case Games.RaymanLegends:
                    return "Rayman Legends";

                case Games.RaymanJungleRun:
                    return "Rayman Jungle Run";

                case Games.RaymanFiestaRun:
                    return $"Rayman Fiesta Run ({RCFRCP.Data.FiestaRunVersion})";

                case Games.RabbidsBigBang:
                    return "Rabbids Big Bang";

                // TODO: Educational games support

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets a display view model for the specified game
        /// </summary>
        /// <param name="game">The game to get the display view model for</param>
        /// <returns>A new display view model</returns>
        public static GameDisplayViewModel GetDisplayViewModel(this Games game)
        {
            if (game.IsAdded())
            {
                var actions = new List<OverflowButtonItemViewModel>();

                // Get the game info
                var info = game.GetInfo();

                // Add launch options if set to do so
                if (info.LaunchMode == GameLaunchMode.AsAdminOption)
                {
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_RunAsAdmin, PackIconMaterialKind.Security, new AsyncRelayCommand(async () => await game.GetGameManager().LaunchGameAsync(true))));

                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Get the game links
                var links = game.GetGameFileLinks()?.Where(x => x.Path.FileExists).ToList();

                // Add links if there are any
                if (links?.Any() ?? false)
                {
                    actions.AddRange(links.
                        Select(x =>
                        {
                            // Get the path
                            string path = x.Path;

                            // Create the command
                            var command = new AsyncRelayCommand(async () => (await RCFRCP.File.LaunchFileAsync(path))?.Dispose());

                            if (x.Icon != PackIconMaterialKind.None)
                                return new OverflowButtonItemViewModel(x.Header, x.Icon, command);

                            try
                            {
                                return new OverflowButtonItemViewModel(x.Header, new FileSystemPath(x.Path).GetIconOrThumbnail(ShellThumbnailSize.Small).ToImageSource(), command);
                            }
                            catch (Exception ex)
                            {
                                ex.HandleError("Getting file icon for overflow button item");
                                return new OverflowButtonItemViewModel(x.Header, x.Icon, command);
                            }
                        }));

                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Get additional items
                var additionalItems = game.GetGameManager().GetAdditionalOverflowButtonItems();

                // Add the items if there are any
                if (additionalItems.Any())
                {
                    actions.AddRange(additionalItems);

                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Add open location
                actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_OpenLocation, PackIconMaterialKind.FolderOutline, new AsyncRelayCommand(async () =>
                {
                    // Get the game info
                    var gameInfo = game.GetInfo();

                    // Get the install directory
                    var instDir = gameInfo.InstallDirectory;

                    // Select the file in Explorer if it exists
                    if ((instDir + game.GetLaunchName()).FileExists)
                        instDir += game.GetLaunchName();

                    // Open the location
                    await RCFRCP.File.OpenExplorerLocationAsync(instDir);

                    RCFCore.Logger?.LogTraceSource($"The game {game} install location was opened");
                }), UserLevel.Advanced));

                actions.Add(new OverflowButtonItemViewModel(UserLevel.Advanced));

                // Add game options
                actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Options, PackIconMaterialKind.SettingsOutline, new RelayCommand(() =>
                {
                    RCFCore.Logger?.LogTraceSource($"The game {game} options dialog is opening...");
                    GameOptions.Show(game, GameOptionsPage.Options);
                })));

                return new GameDisplayViewModel(game.GetDisplayName(), game.GetIconSource(), new ActionItemViewModel(Resources.GameDisplay_Launch, PackIconMaterialKind.Play, new AsyncRelayCommand(async () => await game.GetGameManager().LaunchGameAsync(false))), actions);
            }
            else
            {
                var actions = new List<OverflowButtonItemViewModel>();

                // Get the game links
                var links = game.GetGamePurchaseLinks();

                // Add links if there are any
                if (links?.Any() ?? false)
                {
                    actions.AddRange(links.
                        Select(x =>
                        {
                            // Get the path
                            string path = x.Path;

                            // Create the command
                            var command = new AsyncRelayCommand(async () => (await RCFRCP.File.LaunchFileAsync(path))?.Dispose());

                            // Return the item
                            return new OverflowButtonItemViewModel(x.Header, x.Icon, command);
                        }));
                }

                // Add disc installer options for specific games
                if (game.GetInstallerGifs()?.Any() == true)
                {
                    // Add separator if there are previous actions
                    if (actions.Any())
                        actions.Add(new OverflowButtonItemViewModel());

                    // Add disc installer action
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_DiscInstall, PackIconMaterialKind.Disk, new RelayCommand(() =>
                        // NOTE: This is a blocking dialog
                        new GameInstaller(game).ShowDialog())));
                }

                // Create the command
                var locateCommand = new AsyncRelayCommand(async () => await RCFRCP.App.LocateGameAsync(game));

                // Return the view model
                return new GameDisplayViewModel(game.GetDisplayName(), game.GetIconSource(),
                    new ActionItemViewModel(Resources.GameDisplay_Locate, PackIconMaterialKind.FolderOutline, locateCommand), actions);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="GamePurchaseLink"/> for the specified game
        /// </summary>
        /// <param name="game">The game to get the links for</param>
        /// <returns>The collection of game links</returns>
        public static List<GamePurchaseLink> GetGamePurchaseLinks(this Games game)
        {
            switch (game)
            {
                case Games.Rayman1:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_forever"),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
                    };

                case Games.RaymanDesigner:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_forever"),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
                    };

                case Games.RaymanByHisFans:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_forever"),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
                    };

                case Games.Rayman2:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_2_the_great_escape"),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman-2--the-great-escape/56c4947e88a7e300458b465c.html")
                    };

                case Games.Rayman3:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_3_hoodlum_havoc"),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--3--hoodlum-havoc/5800b15eef3aa5ab3e8b4567.html")
                    };

                case Games.RaymanRavingRabbids:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_Steam, AppViewModel.SteamStoreBaseUrl + game.GetGameManager<SteamGameManager>().GetSteamID()),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_raving_rabbids"),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman-raving-rabbids/56c4948888a7e300458b47de.html")
                    };

                case Games.RaymanOrigins:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_Steam, AppViewModel.SteamStoreBaseUrl + game.GetGameManager<SteamGameManager>().GetSteamID()),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_origins"),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman-origins/56c4948888a7e300458b47dc.html")
                    };

                case Games.RaymanLegends:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_Steam, AppViewModel.SteamStoreBaseUrl + game.GetGameManager<SteamGameManager>().GetSteamID()),
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--legends/56c4948888a7e300458b47da.html")
                    };

                case Games.RaymanJungleRun:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseWinStore, "https://www.microsoft.com/store/productId/9WZDNCRFJ13P")
                    };

                case Games.RaymanFiestaRun:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseWinStore, "https://www.microsoft.com/store/productId/9wzdncrdcw9b")
                    };

                case Games.RabbidsBigBang:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink(Resources.GameDisplay_PurchaseWinStore, "https://www.microsoft.com/store/productId/9WZDNCRFJCS3")
                    };

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="GameFileLink"/> for the specified game
        /// </summary>
        /// <param name="game">The game to get the links for</param>
        /// <returns>The collection of game links</returns>
        public static GameFileLink[] GetGameFileLinks(this Games game)
        {
            var info = game.GetInfo();

            switch (game)
            {
                case Games.RaymanDesigner:
                    return new GameFileLink[]
                    {
                        new GameFileLink(Resources.GameLink_RDMapper, info.InstallDirectory + "MAPPER.EXE")
                    };

                case Games.Rayman2:
                    return new GameFileLink[]
                    {
                        new GameFileLink(Resources.GameLink_Setup, info.InstallDirectory + "GXSetup.exe"),
                        new GameFileLink(Resources.GameLink_R2nGlide, info.InstallDirectory + "nglide_config.exe"),
                        new GameFileLink(Resources.GameLink_R2dgVoodoo, info.InstallDirectory + "dgVoodooCpl.exe")
                    };

                case Games.RaymanM:
                    return new GameFileLink[]
                    {
                        new GameFileLink(Resources.GameLink_Setup, info.InstallDirectory + "RM_Setup_DX8.exe")
                    };

                case Games.RaymanArena:
                    return new GameFileLink[]
                    {
                        new GameFileLink(Resources.GameLink_Setup, info.InstallDirectory + "RM_Setup_DX8.exe")
                    };

                case Games.Rayman3:
                    return new GameFileLink[]
                    {
                        new GameFileLink(Resources.GameLink_Setup, info.InstallDirectory + "R3_Setup_DX8.exe")
                    };

                case Games.RaymanRavingRabbids:
                    return new GameFileLink[]
                    {
                        new GameFileLink(Resources.GameLink_Setup, info.InstallDirectory + "SettingsApplication.exe")
                    };

                default:
                    return new GameFileLink[0];
            }
        }

        /// <summary>
        /// Gets the launch name for a game
        /// </summary>
        /// <param name="game">The game to get the launch name for</param>
        /// <returns>The launch name</returns>
        public static string GetLaunchName(this Games game)
        {
            switch (game)
            {
                case Games.Rayman1:
                    return "Rayman.exe";

                case Games.RaymanDesigner:
                    return "RAYKIT.bat";

                case Games.RaymanByHisFans:
                    return "rayfan.bat";

                case Games.Rayman60Levels:
                    return "Rayman.bat";

                case Games.Rayman2:
                    return "Rayman2.exe";

                case Games.RaymanM:
                    return "RaymanM.exe";

                case Games.RaymanArena:
                    return "R_Arena.exe";

                case Games.Rayman3:
                    return "Rayman3.exe";

                case Games.RaymanRavingRabbids:
                    return "CheckApplication.exe";

                case Games.RaymanOrigins:
                    return "Rayman Origins.exe";

                case Games.RaymanLegends:
                    return "Rayman Legends.exe";

                case Games.RaymanJungleRun:
                    return "UbisoftEntertainment.RaymanJungleRun_dbgk1hhpxymar";

                case Games.RaymanFiestaRun:
                    switch (RCFRCP.Data.FiestaRunVersion)
                    {
                        case FiestaRunEdition.Default:
                            return "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw";

                        case FiestaRunEdition.Preload:
                            return "UbisoftEntertainment.RaymanFiestaRunPreloadEdition_dbgk1hhpxymar";

                        case FiestaRunEdition.Win10:
                            return "Ubisoft.RaymanFiestaRunWindows10Edition_ngz4m417e0mpw";

                        default:
                            throw new ArgumentOutOfRangeException(nameof(RCFRCP.Data.FiestaRunVersion), RCFRCP.Data.FiestaRunVersion, null);
                    }

                case Games.RabbidsBigBang:
                    return "UbisoftEntertainment.RabbidsBigBang_dbgk1hhpxymar";

                case Games.EducationalDos:
                    return RCFRCP.Data.EducationalDosBoxGames.First().LaunchName;

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets a type for the specified game, or null if the operation was cancelled
        /// </summary>
        /// <param name="game">The game to get the type for</param>
        /// <returns>The type or null if the operation was cancelled</returns>
        public static async Task<GameTypeSelectionResult> GetGameTypeAsync(this Games game)
        {
            // TODO: Educational

            // Create the view model
            var vm = new GameTypeSelectionViewModel()
            {
                Title = "Select Game Type"
            };

            switch (game)
            {
                case Games.Rayman1:
                case Games.RaymanDesigner:
                case Games.RaymanByHisFans:
                case Games.Rayman60Levels:
                    return new GameTypeSelectionResult()
                    {
                        CanceledByUser = false,
                        SelectedType = GameType.DosBox
                    };

                case Games.RaymanM:
                case Games.RaymanArena:
                case Games.Rayman3:
                    return new GameTypeSelectionResult()
                    {
                        CanceledByUser = false,
                        SelectedType = GameType.Win32
                    };

                case Games.Rayman2:
                case Games.RaymanRavingRabbids:
                case Games.RaymanOrigins:
                case Games.RaymanLegends:
                    vm.AllowWin32 = true;
                    vm.AllowSteam = true;
                    break;

                case Games.RaymanJungleRun:
                case Games.RaymanFiestaRun:
                case Games.RabbidsBigBang:
                    return new GameTypeSelectionResult()
                    {
                        CanceledByUser = false,
                        SelectedType = GameType.WinStore
                    };

                case Games.EducationalDos:
                    return new GameTypeSelectionResult()
                    {
                        CanceledByUser = false,
                        SelectedType = GameType.EducationalDosBox
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }

            // Create and show the dialog and return the result
            return await RCFRCP.UI.SelectGameTypeAsync(vm);
        }

        /// <summary>
        /// Gets the config content for the specified game
        /// </summary>
        /// <param name="game">The game to get the config content for</param>
        /// <returns>The config content</returns>
        public static FrameworkElement GetConfigContent(this Games game)
        {
            switch (game)
            {
                case Games.Rayman1:
                case Games.RaymanDesigner:
                case Games.RaymanByHisFans:
                case Games.Rayman60Levels:
                case Games.EducationalDos:
                    return new DosBoxConfig(game);

                case Games.Rayman2:
                    return new Rayman2Config();

                case Games.RaymanM:
                case Games.RaymanArena:
                case Games.Rayman3:
                    return new Ray_M_Arena_3_Config(game);

                case Games.RaymanRavingRabbids:
                    return new RaymanRavingRabbidsConfig();

                case Games.RaymanOrigins:
                case Games.RaymanLegends:
                    return new Ray_Origins_Legends_Config(game);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the options content for the specified game
        /// </summary>
        /// <param name="game">The game to get the options content for</param>
        /// <returns>The options content</returns>
        public static FrameworkElement GetOptionsContent(this Games game)
        {
            switch (game)
            {
                case Games.RaymanFiestaRun:
                    return new FiestaRunOptions();

                case Games.EducationalDos:
                    return new EducationalDosOptions();

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the backup info for the specified game
        /// </summary>
        /// <param name="game">The game to get the backup info for</param>
        /// <returns>The game backup info</returns>
        public static List<BackupDir> GetBackupInfo(this Games game)
        {
            // TODO: Educational

            // Get the game info
            var gameInfo = game.GetInfo();

            switch (game)
            {
                case Games.Rayman1:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory,
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.sav",
                            ID = "0"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory,
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.cfg",
                            ID = "1"
                        },
                    };

                case Games.RaymanDesigner:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory,
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.cfg",
                            ID = "0"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "PCMAP",
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.sct",
                            ID = "1"
                        },
                        //
                        // Note:
                        // This will backup the pre-installed maps and the world files as well. This is due to how the backup manager works.
                        // In the future I might make a separate manager for the maps again, in which case the search pattern "MAPS???" should get the
                        // correct mapper directories withing each world directory
                        //
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "CAKE",
                            SearchOption = SearchOption.AllDirectories,
                            ExtensionFilter = "*",
                            ID = "Mapper0"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "CAVE",
                            SearchOption = SearchOption.AllDirectories,
                            ExtensionFilter = "*",
                            ID = "Mapper1"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "IMAGE",
                            SearchOption = SearchOption.AllDirectories,
                            ExtensionFilter = "*",
                            ID = "Mapper2"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "JUNGLE",
                            SearchOption = SearchOption.AllDirectories,
                            ExtensionFilter = "*",
                            ID = "Mapper3"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "MOUNTAIN",
                            SearchOption = SearchOption.AllDirectories,
                            ExtensionFilter = "*",
                            ID = "Mapper4"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "MUSIC",
                            SearchOption = SearchOption.AllDirectories,
                            ExtensionFilter = "*",
                            ID = "Mapper5"
                        },
                    };

                case Games.RaymanByHisFans:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory,
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.cfg",
                            ID = "1"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "PCMAP",
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.sct",
                            ID = "1"
                        },
                    };

                case Games.Rayman60Levels:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory,
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.cfg",
                            ID = "0"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "PCMAP",
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.sct",
                            ID = "1"
                        },
                    };

                case Games.Rayman2:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "Data\\SaveGame",
                            SearchOption = SearchOption.AllDirectories,
                            ID = "0"
                        },
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "Data\\Options",
                            SearchOption = SearchOption.AllDirectories,
                            ID = "1"
                        },
                    };

                case Games.Rayman3:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "GAMEDATA\\SaveGame",
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ID = "0"
                        }
                    };

                case Games.RaymanM:
                case Games.RaymanArena:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory + "Menu\\SaveGame",
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ID = "0"
                        }
                    };

                case Games.RaymanRavingRabbids:
                    var dirs = new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = gameInfo.InstallDirectory,
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.sav",
                            ID = "0"
                        },
                    };

                    if (gameInfo.GameType == GameType.Win32)
                        dirs.Add(new BackupDir()
                        {
                            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore", gameInfo.InstallDirectory.RemoveRoot()),
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.sav",
                            ID = "0"
                        });

                    return dirs;

                case Games.RaymanOrigins:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My games\\Rayman origins"),
                            SearchOption = SearchOption.AllDirectories,
                            ID = "0"
                        }
                    };

                case Games.RaymanLegends:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Rayman Legends"),
                            SearchOption = SearchOption.AllDirectories,
                            ID = "0"
                        }
                    };

                case Games.RaymanJungleRun:
                case Games.RaymanFiestaRun:
                case Games.RabbidsBigBang:
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", game.GetLaunchName()),
                            SearchOption = SearchOption.AllDirectories,
                            ID = "0"
                        }
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets the installer gif sources for a game
        /// </summary>
        /// <param name="game">The game to get the gif sources for</param>
        /// <returns>The gif sources, if any</returns>
        public static string[] GetInstallerGifs(this Games game)
        {
            var basePath = $"{AppViewModel.ApplicationBasePath}Installer/InstallerGifs/";

            if (game == Games.Rayman2)
            {
                return new string[]
                {
                    basePath + "ASTRO.gif",
                    basePath + "BAST.gif",
                    basePath + "CASK.gif",
                    basePath + "CHASE.gif",
                    basePath + "GLOB.gif",
                    basePath + "SKI.gif",
                    basePath + "WHALE.gif"
                };
            }
            else if (game == Games.RaymanM || game == Games.RaymanArena)
            {
                return new string[]
                {
                    basePath + "BAST.gif",
                    basePath + "CHASE.gif",
                    basePath + "GLOB.gif",
                    basePath + "RAY.gif"
                };
            }

            return null;
        }

        /// <summary>
        /// Gets the applied utilities for the specified game
        /// </summary>
        /// <param name="game">The game to get the applied utilities for</param>
        /// <returns>The applied utilities</returns>
        public static async Task<string[]> GetAppliedUtilitiesAsync(this Games game)
        {
            // Create the output
            var output = new List<string>();

            // Get hard-coded utilities
            if (game == Games.Rayman2)
            {
                if (await Rayman2ConfigViewModel.GetIsWidescreenHackAppliedAsync() == true)
                    output.Add(Resources.Config_WidescreenSupport);

                var dinput = Rayman2ConfigViewModel.GetCurrentDinput();

                if (dinput == Rayman2ConfigViewModel.R2Dinput.Controller)
                    output.Add(Resources.Config_UseController);

                if (dinput == Rayman2ConfigViewModel.R2Dinput.Mapping)
                    output.Add(Resources.Config_ButtonMapping);
            }

            // Get other utilities
            output.AddRange(RCFRCP.App.GetUtilities(game).SelectMany(x => x.GetAppliedUtilities()));

            return output.ToArray();
        }

        #endregion
    }
}