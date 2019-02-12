using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

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
            return $"{AppHandler.ApplicationBasePath}Img/GameIcons/{game}.png";
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
                case GameType.Win32:
                    return (installDir + game.GetLaunchName()).FileExists;

                case GameType.Steam:
                    return RCFWinReg.RegistryManager.KeyExists(RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {game.GetSteamID()}"), RegistryView.Registry64);

                case GameType.WinStore:
                    // TODO: Add check
                    return true;

                case GameType.DosBox:
                    // TODO: Add check
                    return true;

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
                var links = game.GetGameFileLinks()?.Where(x => x.Path.FileExists);

                // Add links if there are any
                if (links?.Any() ?? false)
                {
                    actions.AddRange(links.
                        Select(x =>
                        {
                            // Get the path
                            string path = x.Path;

                            // Create the command
                            var command = new AsyncRelayCommand(async () => await RCFRCP.File.LaunchFileAsync(path));

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
                        await RCFRCP.File.OpenExplorerLocationAsync($"https://store.steampowered.com/app/" + game.GetSteamID());
                        RCF.Logger.LogTraceSource($"The game {game} Steam store page was opened");
                    })));
                    actions.Add(new OverflowButtonItemViewModel("Open community page", PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
                    {
                        await RCFRCP.File.OpenExplorerLocationAsync($"https://steamcommunity.com/app/" + game.GetSteamID());
                        RCF.Logger.LogTraceSource($"The game {game} Steam community page was opened");
                    })));

                    actions.Add(new OverflowButtonItemViewModel());
                }

                if (RCFRCP.Data.UserLevel >= UserLevel.Advanced)
                {
                    // Add open location
                    actions.Add(new OverflowButtonItemViewModel("Open Location", PackIconMaterialKind.FolderOutline, new AsyncRelayCommand(async () =>
                    {
                        await RCFRCP.File.OpenExplorerLocationAsync(game.GetInfo().InstallDirectory);
                        RCF.Logger.LogTraceSource($"The game {game} install location was opened");
                    })));

                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Add game options
                actions.Add(new OverflowButtonItemViewModel("Options", PackIconMaterialKind.SettingsOutline, new RelayCommand(() =>
                {
                    RCF.Logger.LogTraceSource($"The game {game} options dialog is opening...");
                    new GameOptions(game).ShowDialog();
                })));

                return new GameDisplayViewModel(game.GetDisplayName(), game.GetIconSource(), 
                    new ActionItemViewModel("Launch", PackIconMaterialKind.Play, new AsyncRelayCommand(async () =>
                    {
                        // Get the launch info
                        var launchInfo = game.GetLaunchInfo();

                        RCF.Logger.LogTraceSource($"The game {game} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

                        // Launch the game
                        await RCFRCP.File.LaunchFileAsync(launchInfo.Path, false, launchInfo.Args);

                        RCF.Logger.LogInformationSource($"The game {game} has been launched");
                    })) , actions);
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
                            var command = new AsyncRelayCommand(async () => await RCFRCP.File.LaunchFileAsync(path));

                            // Return the item
                            return new OverflowButtonItemViewModel(x.Header, x.Icon, command);
                        }));
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
                        new GamePurchaseLink("Get from Steam", AppHandler.SteamStoreBaseUrl + game.GetSteamID()),
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_raving_rabbids"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman-raving-rabbids/56c4948888a7e300458b47de.html")
                    };

                case Games.RaymanOrigins:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from Steam", AppHandler.SteamStoreBaseUrl + game.GetSteamID()),
                        new GamePurchaseLink("Get from GOG", "https://www.gog.com/game/rayman_origins"),
                        new GamePurchaseLink("Get from Uplay", "https://store.ubi.com/eu/rayman-origins/56c4948888a7e300458b47dc.html")
                    };

                case Games.RaymanLegends:
                    return new List<GamePurchaseLink>()
                    {
                        new GamePurchaseLink("Get from Steam", AppHandler.SteamStoreBaseUrl + game.GetSteamID()),
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
                    return new GameLaunchInfo("shell:appsFolder\\" + $"{game.GetLaunchName()}!App", null);

                case GameType.DosBox:
                    var dosBoxConfig = RCFRCP.Data.DosBoxGames[game];
                    return new GameLaunchInfo(RCFRCP.Data.DosBoxPath, DosBoxHelpers.GetDosBoxArgument(RCFRCP.Data.DosBoxConfig, info.InstallDirectory, dosBoxConfig.MountPath, dosBoxConfig.Commands, game.GetLaunchName()));

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
                    return "RAYKIT.EXE ver=usa";

                case Games.RaymanByHisFans:
                    return "@rayfan ver=usa";

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

                // TODO: Support Win10 and preload editions
                case Games.RaymanFiestaRun:
                    return "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw";

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

            // TODO: Move to UI manager
            // Create and show the dialog and return the result
            return await new GameTypeSelectionDialog(vm).ShowDialogAsync();
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
                    return new DosBoxConfig(parentDialogWindow, game);

                case Games.RaymanDesigner:
                    return new DosBoxConfig(parentDialogWindow, game);

                case Games.RaymanByHisFans:
                    return new DosBoxConfig(parentDialogWindow, game);

                case Games.Rayman60Levels:
                    return new DosBoxConfig(parentDialogWindow, game);

                case Games.Rayman2:
                    return new Rayman2Config(parentDialogWindow);

                case Games.RaymanM:
                    return null;

                case Games.RaymanArena:
                    return null;

                case Games.Rayman3:
                    return null;

                case Games.RaymanRavingRabbids:
                    return null;

                case Games.RaymanOrigins:
                    return null;

                case Games.RaymanLegends:
                    return null;

                case Games.RaymanJungleRun:
                    return null;

                case Games.RaymanFiestaRun:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }
    }
}