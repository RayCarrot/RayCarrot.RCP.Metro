using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles common actions and events for this application
    /// </summary>
    public class AppHandler
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AppHandler()
        {
            SaveUserDataAsyncLock = new AsyncLock();
        }

        #endregion

        #region Constant Fields

        /// <summary>
        /// The base path for this application
        /// </summary>
        public const string ApplicationBasePath = "pack://application:,,,/RayCarrot.RCP.Metro;component/";

        /// <summary>
        /// The Steam store base url
        /// </summary>
        public const string SteamStoreBaseUrl = "https://store.steampowered.com/app/";

        #endregion

        #region Public Properties

        /// <summary>
        /// An async lock for the <see cref="SaveUserDataAsync"/> method
        /// </summary>
        private AsyncLock SaveUserDataAsyncLock { get; }

        /// <summary>
        /// The current app version
        /// </summary>
        public Version CurrentVersion => new Version(0, 0, 0, 0);

        /// <summary>
        /// Gets a collection of the available <see cref="Games"/>
        /// </summary>
        public IEnumerable<Games> GetGames => Enum.GetValues(typeof(Games)).Cast<Games>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new game to the app data
        /// </summary>
        /// <param name="game">The game to add</param>
        /// <param name="type">The game type</param>
        /// <param name="installDirectory">The game install directory, if available</param>
        /// <returns>The task</returns>
        public async Task AddNewGameAsync(Games game, GameType type, FileSystemPath? installDirectory = null)
        {
            RCF.Logger.LogInformationSource($"The game {game} is being added of type {type}...");

            // Make sure the game hasn't already been added
            if (game.IsAdded())
            {
                RCF.Logger.LogWarningSource($"The game {game} has already been added");

                // TODO: Show error message

                return;
            }

            // Get the install directory
            if (installDirectory == null)   
            {
                if (type == GameType.Steam)
                {
                    try
                    {
                        // Get the key path
                        var keyPath = RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {game.GetSteamID()}");

                        using (var key = RCFWinReg.RegistryManager.GetKeyFromFullPath(keyPath, RegistryView.Registry64))
                            installDirectory = key?.GetValue("InstallLocation") as string;
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Getting Steam game install directory");
                    }
                }
                else if (type == GameType.WinStore)
                {
                    // TODO: Use Win10 API in .NET 4.8
                }
                else
                {
                    // TODO: Handle error
                    return;
                }

                RCF.Logger.LogInformationSource($"The game {game} install directory was retrieved as {installDirectory}");
            }

            // Add the game
            RCFRCP.Data.Games.Add(game, new GameInfo(type, installDirectory ?? FileSystemPath.EmptyPath));

            RCF.Logger.LogInformationSource($"The game {game} has been added");

            // Refresh
            OnRefreshRequired();
        }

        /// <summary>
        /// Saves all user data for the application
        /// </summary>
        public async Task SaveUserDataAsync()
        {
            // Lock the saving of user data
            using (await SaveUserDataAsyncLock.LockAsync())
            {
                try
                {
                    // Run it as a new task
                    await Task.Run(async () =>
                    {
                        // Save all user data
                        await RCFData.UserDataCollection.SaveAllAsync();
                    });

                    RCF.Logger.LogInformationSource($"The application user data was saved");
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Saving user data");
                }
            }
        }

        /// <summary>
        /// Resets all user data for the application
        /// </summary>
        /// <returns></returns>
        public void ResetData()
        {
            RCFData.UserDataCollection.ForEach(x => x.Reset());

            RCF.Logger.LogInformationSource($"The application user data was reset");

            OnRefreshRequired();
        }

        /// <summary>
        /// Fires the <see cref="RefreshRequired"/> event
        /// </summary>
        /// <param name="e">The event arguments, or null to use the default ones</param>
        public void OnRefreshRequired(EventArgs e = null)
        {
            RefreshRequired?.Invoke(this, e ?? EventArgs.Empty);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a refresh is required for the games
        /// </summary>
        public event EventHandler RefreshRequired;

        #endregion
    }
}