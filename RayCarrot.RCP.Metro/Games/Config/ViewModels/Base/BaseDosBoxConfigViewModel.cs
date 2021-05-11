using Nito.AsyncEx;
using RayCarrot.Logging;
using RayCarrot.UI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public abstract class BaseDosBoxConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor for a specific game
        /// </summary>
        /// <param name="game">The DosBox game</param>
        /// <param name="gameType">The type of game</param>
        protected BaseDosBoxConfigViewModel(Games game, GameType gameType)
        {
            Game = game;
            GameType = gameType;

            // Create the async lock
            AsyncLock = new AsyncLock();

            // Create the commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for saving the configuration
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The DosBox game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The type of game
        /// </summary>
        public GameType GameType { get; }

        /// <summary>
        /// Indicates if changing the game language is available
        /// </summary>
        public bool IsGameLanguageAvailable { get; set; }

        /// <summary>
        /// Indicates if changing the game mount location is available
        /// </summary>
        public bool IsMountLocationAvailable { get; set; }

        /// <summary>
        /// The allowed drive types when browsing for a mount path
        /// </summary>
        public DriveType[] MountPathAllowedDriveTypes => new DriveType[]
        {
            DriveType.CDRom
        };

        #endregion

        #region Commands

        public AsyncRelayCommand SaveCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override async Task SetupAsync()
        {
            RL.Logger?.LogInformationSource($"{Game} config is being set up");

            // Perform setup
            await SetupGameAsync();

            UnsavedChanges = false;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RL.Logger?.LogInformationSource($"{Game} config is saving...");

                try
                {
                    // Perform saving
                    await SaveGameAsync();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving DosBox game configuration data");

                    await WPF.Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, Game.GetGameInfo().DisplayName), Resources.Config_SaveErrorHeader);
                    return;
                }

                UnsavedChanges = false;

                await WPF.Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                OnSave();
            }
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Performs additional setup for the game
        /// </summary>
        /// <returns>The task</returns>
        protected abstract Task SetupGameAsync();

        /// <summary>
        /// Performs additional saving for the game
        /// </summary>
        /// <returns>The task</returns>
        protected abstract Task SaveGameAsync();

        #endregion
    }
}