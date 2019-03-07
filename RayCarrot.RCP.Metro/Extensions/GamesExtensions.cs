using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;
using Package = Windows.ApplicationModel.Package;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="Games"/>
    /// </summary>
    public static class GamesExtensions
    {
        /// <summary>
        /// Gets the icon source for the specified game
        /// </summary>
        /// <param name="game">The game to get the icon source for</param>
        /// <returns>The icon source</returns>
        public static string GetIconSource(this Games game)
        {
            return $"{AppViewModel.ApplicationBasePath}Img/GameIcons/{game}.png";
        }

        /// <summary>
        /// Gets the display name for the specified game
        /// </summary>
        /// <param name="game">The game to get the display name for</param>
        /// <returns>The display name</returns>
        public static string GetDisplayName(this Games game)
        {
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
                    return "Rayman Fiesta Run";

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
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
        /// Determines if the specified game is valid
        /// </summary>
        /// <param name="game">The game to check if it's valid</param>
        /// <param name="type">The game type</param>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public static bool IsValid(this Games game, GameType type, FileSystemPath installDir)
        {
            switch (type)
            {
                case GameType.DosBox:
                case GameType.Win32:
                    return (installDir + game.GetLaunchName()).FileExists;

                case GameType.Steam:
                    return RCFWinReg.RegistryManager.KeyExists(RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {game.GetSteamID()}"), RegistryView.Registry64);

                case GameType.WinStore:
                    return game.GetGamePackage() != null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
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

                if (info.GameType == GameType.Steam)
                {
                    // Add Steam links
                    actions.Add(new OverflowButtonItemViewModel("Open store page", PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
                    {
                        (await RCFRCP.File.LaunchFileAsync($"https://store.steampowered.com/app/" + game.GetSteamID()))?.Dispose();
                        RCF.Logger.LogTraceSource($"The game {game} Steam store page was opened");
                    })));
                    actions.Add(new OverflowButtonItemViewModel("Open community page", PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
                    {
                        (await RCFRCP.File.LaunchFileAsync($"https://steamcommunity.com/app/" + game.GetSteamID()))?.Dispose();
                        RCF.Logger.LogTraceSource($"The game {game} Steam community page was opened");
                    })));

                    actions.Add(new OverflowButtonItemViewModel());
                }

                if (RCFRCP.Data.UserLevel >= UserLevel.Advanced)
                {
                    // Add open location
                    actions.Add(new OverflowButtonItemViewModel("Open Location", PackIconMaterialKind.FolderOutline, new AsyncRelayCommand(async () =>
                    {
                        // Get the game info
                        var gameInfo = game.GetInfo();

                        // Get the install directory
                        var instDir = gameInfo.InstallDirectory;

                        // Select the file if not a Windows store game
                        if (gameInfo.GameType != GameType.WinStore)
                            instDir = instDir + game.GetLaunchName();

                        // Open the location
                        await RCFRCP.File.OpenExplorerLocationAsync(instDir);

                        RCF.Logger.LogTraceSource($"The game {game} install location was opened");
                    })));

                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Check if the game has utilities
                if (game.HasUtilities())
                {
                    // Add game utilities
                    actions.Add(new OverflowButtonItemViewModel("Utilities", PackIconMaterialKind.BriefcaseOutline, new RelayCommand(() =>
                    {
                        RCF.Logger.LogTraceSource($"The game {game} utilities dialog is opening...");
                        new GameOptions(game, false).ShowDialog();
                    })));
                }

                // Add game options
                actions.Add(new OverflowButtonItemViewModel("Options", PackIconMaterialKind.SettingsOutline, new RelayCommand(() =>
                {
                    RCF.Logger.LogTraceSource($"The game {game} options dialog is opening...");
                    new GameOptions(game, true).ShowDialog();
                })));

                return new GameDisplayViewModel(game.GetDisplayName(), game.GetIconSource(),
                    new ActionItemViewModel("Launch", PackIconMaterialKind.Play, new AsyncRelayCommand(async () => await RCFRCP.Game.LaunchGameAsync(game))), actions);
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
                    actions.Add(new OverflowButtonItemViewModel("Install from disc", PackIconMaterialKind.Disk, new RelayCommand(() => new GameInstaller(game).ShowDialog())));
                }

                // Create the command
                var locateCommand = new AsyncRelayCommand(async () => await RCFRCP.Game.LocateGameAsync(game));

                // Return the view model
                return new GameDisplayViewModel(game.GetDisplayName(), game.GetIconSource(),
                    new ActionItemViewModel("Locate", PackIconMaterialKind.FolderOutline, locateCommand), actions);
            }
        }

        /// <summary>
        /// Gets a Steam ID for the specified game
        /// </summary>
        /// <param name="game">The game to get the Steam ID for</param>
        /// <returns>The Steam ID</returns>
        public static string GetSteamID(this Games game)
        {
            switch (game)
            {
                case Games.Rayman2:
                    return "15060";

                case Games.RaymanRavingRabbids:
                    return "15080";

                case Games.RaymanOrigins:
                    return "207490";

                case Games.RaymanLegends:
                    return "242550";

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, "The game is not a Steam game");
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
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_forever"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
                    };

                case Games.RaymanDesigner:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_forever"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
                    };

                case Games.RaymanByHisFans:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_forever"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
                    };

                case Games.Rayman60Levels:
                    return null;

                case Games.Rayman2:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_2_the_great_escape"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman-2--the-great-escape/56c4947e88a7e300458b465c.html")
                    };

                case Games.RaymanM:
                    return null;

                case Games.RaymanArena:
                    return null;

                case Games.Rayman3:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_3_hoodlum_havoc"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman--3--hoodlum-havoc/5800b15eef3aa5ab3e8b4567.html")
                    };

                case Games.RaymanRavingRabbids:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from Steam", AppViewModel.SteamStoreBaseUrl + game.GetSteamID()),
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_raving_rabbids"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman-raving-rabbids/56c4948888a7e300458b47de.html")
                    };

                case Games.RaymanOrigins:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from Steam", AppViewModel.SteamStoreBaseUrl + game.GetSteamID()),
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_origins"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman-origins/56c4948888a7e300458b47dc.html")
                    };

                case Games.RaymanLegends:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from Steam", AppViewModel.SteamStoreBaseUrl + game.GetSteamID()),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman--legends/56c4948888a7e300458b47da.html")
                    };

                case Games.RaymanJungleRun:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from Windows Store", "https://www.microsoft.com/store/productId/9WZDNCRFJ13P")
                    };

                case Games.RaymanFiestaRun:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from Windows Store", "https://www.microsoft.com/store/productId/9NBLGGH59M6B")
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="GameFileLink"/> for the specified game
        /// </summary>
        /// <param name="game">The game to get the links for</param>
        /// <returns>The collection of game links</returns>
        public static List<GameFileLink> GetGameFileLinks(this Games game)
        {
            var info = game.GetInfo();

            switch (game)
            {
                case Games.RaymanDesigner:
                    return new List<GameFileLink>()
                    {
                        new GameFileLink("Mapper", info.InstallDirectory + "MAPPER.EXE")
                    };

                case Games.Rayman2:
                    return new List<GameFileLink>()
                    {
                        new GameFileLink("Setup", info.InstallDirectory + "GXSetup.exe"),
                        new GameFileLink("nGlide config", info.InstallDirectory + "nglide_config.exe")
                    };

                case Games.RaymanM:
                    return new List<GameFileLink>()
                    {
                        new GameFileLink("Setup", info.InstallDirectory + "RM_Setup_DX8.exe")
                    };

                case Games.RaymanArena:
                    return new List<GameFileLink>()
                    {
                        new GameFileLink("Setup", info.InstallDirectory + "RM_Setup_DX8.exe")
                    };

                case Games.Rayman3:
                    return new List<GameFileLink>()
                    {
                        new GameFileLink("Setup", info.InstallDirectory + "R3_Setup_DX8.exe")
                    };

                case Games.RaymanRavingRabbids:
                    return new List<GameFileLink>()
                    {
                        new GameFileLink("Setup", info.InstallDirectory + "SettingsApplication.exe")
                    };

                case Games.Rayman1:
                case Games.RaymanByHisFans:
                case Games.Rayman60Levels:
                case Games.RaymanOrigins:
                case Games.RaymanLegends:
                case Games.RaymanJungleRun:
                case Games.RaymanFiestaRun:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets the launch information for the specified game
        /// </summary>
        /// <param name="game">The game to get the launch information for</param>
        /// <returns>The launch info</returns>
        public static GameLaunchInfo GetLaunchInfo(this Games game)
        {
            // Get the info
            var info = game.GetInfo();

            switch (info.GameType)
            {
                case GameType.Win32:
                    return new GameLaunchInfo(info.InstallDirectory + game.GetLaunchName(), null);

                case GameType.Steam:
                    return new GameLaunchInfo(@"steam://rungameid/" + game.GetSteamID(), null);

                case GameType.WinStore:
                    // throw new ArgumentOutOfRangeException(nameof(info.GameType), info.GameType, "Launch info can not be obtained for a Windows Store application");
                    return new GameLaunchInfo("shell:appsFolder\\" + $"{game.GetLaunchName()}!App", null);

                case GameType.DosBox:
                    var dosBoxConfig = RCFRCP.Data.DosBoxGames[game];
                    return new GameLaunchInfo(RCFRCP.Data.DosBoxPath, DosBoxHelpers.GetDosBoxArgument(RCFRCP.Data.DosBoxConfig, info.InstallDirectory, dosBoxConfig.MountPath, dosBoxConfig.GetCommands(), game.GetLaunchName()));

                default:
                    throw new ArgumentOutOfRangeException(nameof(info.GameType));
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
                    return RCFRCP.Data.IsFiestaRunWin10Edition ? "Ubisoft.RaymanFiestaRunWindows10Edition_ngz4m417e0mpw" : "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw";
                    // throw new ArgumentOutOfRangeException(nameof(game), game, "A launch name can not be obtained from a Windows Store application");

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
                    return new GameTypeSelectionResult()
                    {
                        CanceledByUser = false,
                        SelectedType = GameType.WinStore
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
        /// <param name="parentDialogWindow">The parent dialog window</param>
        /// <returns>The config content</returns>
        public static FrameworkElement GetConfigContent(this Games game, Window parentDialogWindow)
        {
            switch (game)
            {
                case Games.Rayman1:
                case Games.RaymanDesigner:
                case Games.RaymanByHisFans:
                case Games.Rayman60Levels:
                    return new DosBoxConfig(parentDialogWindow, game);

                case Games.Rayman2:
                    return new Rayman2Config(parentDialogWindow);

                case Games.RaymanM:
                case Games.RaymanArena:
                case Games.Rayman3:
                    return new Ray_M_Arena_3_Config(parentDialogWindow, game);

                case Games.RaymanRavingRabbids:
                    return new RaymanRavingRabbidsConfig(parentDialogWindow);

                case Games.RaymanOrigins:
                case Games.RaymanLegends:
                    return new Ray_Origins_Legends_Config(parentDialogWindow, game);

                case Games.RaymanJungleRun:
                case Games.RaymanFiestaRun:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets a value indicating if the specified game has utilities
        /// </summary>
        /// <param name="game">The game to check</param>
        /// <returns>True if the game has utilities, otherwise false</returns>
        public static bool HasUtilities(this Games game)
        {
            switch (game)
            {
                case Games.Rayman1:
                case Games.RaymanDesigner:
                case Games.Rayman2:
                case Games.RaymanOrigins:
                case Games.RaymanLegends:
                    return true;

                case Games.RaymanByHisFans:
                case Games.Rayman60Levels:
                case Games.RaymanM:
                case Games.RaymanArena:
                case Games.Rayman3:
                case Games.RaymanRavingRabbids:
                case Games.RaymanJungleRun:
                case Games.RaymanFiestaRun:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the utilities content for the specified game
        /// </summary>
        /// <param name="game">The game to get the utilities content for</param>
        /// <returns>The utilities content</returns>
        public static FrameworkElement GetUtilitiesContent(this Games game)
        {
            switch (game)
            {
                case Games.Rayman1:
                    return new Rayman1Utilities();

                case Games.RaymanDesigner:
                    return new RaymanDesignerUtilities();

                case Games.Rayman2:
                    return new Rayman2Utilities();

                case Games.RaymanOrigins:
                    return new RaymanOriginsUtilities();

                case Games.RaymanLegends:
                    return new RaymanLegendsUtilities();

                case Games.RaymanByHisFans:
                case Games.Rayman60Levels:
                case Games.RaymanM:
                case Games.RaymanArena:
                case Games.Rayman3:
                case Games.RaymanRavingRabbids:
                case Games.RaymanJungleRun:
                case Games.RaymanFiestaRun:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets the game package for a Windows Store game
        /// </summary>
        /// <param name="game">The game to get the package for</param>
        /// <returns>The package or null if not found</returns>
        public static Package GetGamePackage(this Games game)
        {
            // Make sure version is at least Windows 8
            if (Environment.OSVersion.Version < new Version(6, 2, 0, 0))
                return null;

            switch (game)
            {
                case Games.RaymanJungleRun:
                case Games.RaymanFiestaRun:
                    return new PackageManager().FindPackagesForUser(String.Empty).FindItem(x => x.Id.Name == game.GetPackageName());

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, "A game package can only be retrieved for Rayman Jungle Run or Rayman Fiesta Run");
            }
        }

        /// <summary>
        /// Gets the package name for a Windows Store game
        /// </summary>
        /// <param name="game">The game to get the package name for</param>
        /// <returns>The package name</returns>
        public static string GetPackageName(this Games game)
        {
            if (game == Games.RaymanJungleRun)
                return "UbisoftEntertainment.RaymanJungleRun";
            else if (game == Games.RaymanFiestaRun)
                return RCFRCP.Data.IsFiestaRunWin10Edition ? "Ubisoft.RaymanFiestaRunWindows10Edition" : "Ubisoft.RaymanFiestaRun";

            throw new ArgumentOutOfRangeException(nameof(game), game, "A package name can not be obtained from the specified game");
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
        /// Gets the backup info for the specified game
        /// </summary>
        /// <param name="game">The game to get the backup info for</param>
        /// <returns>The game backup info</returns>
        public static List<BackupDir> GetBackupInfo(this Games game)
        {
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
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = RCFRCP.Data.RRRIsSaveDataInInstallDir ? gameInfo.InstallDirectory : new FileSystemPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore\\Program Files(x86)\\Ubisoft\\Rayman Raving Rabbids")),
                            SearchOption = SearchOption.TopDirectoryOnly,
                            ExtensionFilter = "*.sav",
                            ID = "0"
                        }
                    };

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
                    return new List<BackupDir>()
                    {
                        new BackupDir()
                        {
                            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", game.GetLaunchName()),
                            SearchOption = SearchOption.AllDirectories,
                            ID = "0"
                        }
                    };

                case Games.RaymanFiestaRun:
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
    }
}