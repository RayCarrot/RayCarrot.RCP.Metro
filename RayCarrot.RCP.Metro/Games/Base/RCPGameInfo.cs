using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base for Rayman Control Panel game data
    /// </summary>
    public abstract class RCPGameInfo : RCPGame
    {
        #region Protected Constants

        /// <summary>
        /// The group name to use for a dialog which requires reading/writing to a ubi.ini file
        /// </summary>
        protected const string UbiIniFileGroupName = "ubini-config";

        #endregion

        #region Public Properties

        /// <summary>
        /// The icon source for the game
        /// </summary>
        public string IconSource => $"{AppViewModel.ApplicationBasePath}Img/GameIcons/{Game}.png";

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The game display name
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// The game backup name
        /// </summary>
        public abstract string BackupName { get; }

        /// <summary>
        /// Gets the default file name for launching the game, if available
        /// </summary>
        public abstract string DefaultFileName { get; }

        #endregion

        #region Protected Virtual Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected virtual IList<BackupDir> GetBackupDirectories => null;

        #endregion

        #region Public Virtual Properties

        /// <summary>
        /// The options UI, if any is available
        /// </summary>
        public virtual FrameworkElement OptionsUI => null;

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public virtual FrameworkElement ConfigUI => null;

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public virtual IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        /// <summary>
        /// Gets the backup infos from the directories specified from <see cref="GetBackupDirectories"/>
        /// </summary>
        public virtual IList<IBackupInfo> GetBackupInfos
        {
            get
            {
                var backupDirectories = GetBackupDirectories;

                return backupDirectories == null
                    ? new List<IBackupInfo>()
                    : new List<IBackupInfo>()
                    {
                        new BaseBackupInfo(BackupName, backupDirectories, DisplayName)
                    };
            }
        }

        /// <summary>
        /// The group names to use for the options, config and utility dialog
        /// </summary>
        public virtual IEnumerable<string> DialogGroupNames => new string[0];

        /// <summary>
        /// Indicates if the game can be installed from a disc in this program
        /// </summary>
        public virtual bool CanBeInstalledFromDisc => false;

        #endregion

        #region Public Virtual Methods

        /// <summary>
        /// Gets the applied utilities for the specified game
        /// </summary>
        /// <returns>The applied utilities</returns>
        public virtual Task<IList<string>> GetAppliedUtilitiesAsync()
        {
            return Task.FromResult<IList<string>>(RCFRCP.App.GetUtilities(Game).SelectMany(x => x.GetAppliedUtilities()).ToArray());
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets a display view model for the game
        /// </summary>
        /// <returns>A new display view model</returns>
        public GameDisplayViewModel GetDisplayViewModel()
        {
            try
            {
                if (IsAdded)
                {
                    var actions = new List<OverflowButtonItemViewModel>();

                    // Get the Game data
                    var data = GameData;

                    // Get the manager
                    var manager = Game.GetManager(data.GameType);

                    // Add launch options if set to do so
                    if (data.LaunchMode == GameLaunchMode.AsAdminOption)
                    {
                        actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_RunAsAdmin, PackIconMaterialKind.Security, new AsyncRelayCommand(async () => await Game.GetManager().LaunchGameAsync(true))));

                        actions.Add(new OverflowButtonItemViewModel());
                    }

                    // Get the Game links
                    var links = GetGameFileLinks?.Where(x => x.Path.FileExists).ToArray();

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
                    var additionalItems = manager.GetAdditionalOverflowButtonItems;

                    // Add the items if there are any
                    if (additionalItems.Any())
                    {
                        actions.AddRange(additionalItems);

                        actions.Add(new OverflowButtonItemViewModel());
                    }

                    // Add open location
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_OpenLocation, PackIconMaterialKind.FolderOutline, new AsyncRelayCommand(async () =>
                    {
                        // Get the Game data
                        var GameInfo = Game.GetData();

                        // Get the install directory
                        var instDir = GameInfo.InstallDirectory;

                        // Select the file in Explorer if it exists
                        if ((instDir + DefaultFileName).FileExists)
                            instDir += DefaultFileName;

                        // Open the location
                        await RCFRCP.File.OpenExplorerLocationAsync(instDir);

                        RCFCore.Logger?.LogTraceSource($"The Game {Game} install location was opened");
                    }), UserLevel.Advanced));

                    actions.Add(new OverflowButtonItemViewModel(UserLevel.Advanced));

                    // Add Game options
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Options, PackIconMaterialKind.SettingsOutline, new RelayCommand(() =>
                    {
                        RCFCore.Logger?.LogTraceSource($"The Game {Game} options dialog is opening...");
                        GameOptions.Show(Game, GameOptionsPage.Options);
                    })));

                    return new GameDisplayViewModel(Game, DisplayName, IconSource, new ActionItemViewModel(Resources.GameDisplay_Launch, PackIconMaterialKind.Play, new AsyncRelayCommand(async () => await Game.GetManager().LaunchGameAsync(false))), actions);
                }
                else
                {
                    var actions = new List<OverflowButtonItemViewModel>();

                    // Get the purchase links
                    var links = Game.
                        // Get all available managers
                        GetManagers().
                        // Get the purchase links
                        SelectMany(x => x.GetGamePurchaseLinks);

                    // Add links
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

                    // Add disc installer options for specific Games
                    if (CanBeInstalledFromDisc)
                    {
                        // Add separator if there are previous actions
                        if (actions.Any())
                            actions.Add(new OverflowButtonItemViewModel());

                        // Add disc installer action
                        actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_DiscInstall, PackIconMaterialKind.Disk, new RelayCommand(() =>
                            // NOTE: This is a blocking dialog
                            new GameInstaller(Game).ShowDialog())));
                    }

                    // Create the command
                    var locateCommand = new AsyncRelayCommand(async () => await RCFRCP.App.LocateGameAsync(Game));

                    // Return the view model
                    return new GameDisplayViewModel(Game, DisplayName, IconSource,
                        new ActionItemViewModel(Resources.GameDisplay_Locate, PackIconMaterialKind.FolderOutline, locateCommand), actions);
                }
            }
            catch (Exception ex)
            {
                ex.HandleCritical("Getting game display view model");
                throw;
            }
        }

        #endregion
    }
}