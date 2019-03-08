using System;
using System.Threading.Tasks;
using IniParser.Model;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base class for ubi.ini game config view models
    /// </summary>
    /// <typeparam name="Handler">The config handler</typeparam>
    public abstract class BaseUbiIniGameConfigViewModel<Handler> : GameConfigViewModel
        where Handler : UbiIniHandler
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        protected BaseUbiIniGameConfigViewModel(Games game)
        {
            // Create the async lock
            AsyncLock = new AsyncLock();

            // Create the commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);

            // Set properties
            Game = game;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The async lock to use for saving the configuration
        /// </summary>
        protected AsyncLock AsyncLock { get; }

        /// <summary>
        /// The configuration data
        /// </summary>
        protected Handler ConfigData { get; set; }

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
            RCF.Logger.LogInformationSource($"{Game.GetDisplayName()} config is being set up");

            // Run setup code
            await OnSetupAsync();

            // Load the configuration data
            ConfigData = await LoadConfigAsync();

            RCF.Logger.LogInformationSource($"The ubi.ini file has been loaded");

            // Re-create the section if it doesn't exist
            if (!ConfigData.Exists)
            {
                ConfigData.ReCreate();
                RCF.Logger.LogInformationSource($"The ubi.ini section for {Game.GetDisplayName()} was recreated");
            }

            // Import config data
            await ImportConfigAsync();

            UnsavedChanges = false;

            RCF.Logger.LogInformationSource($"All section properties have been loaded");
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"{Game.GetDisplayName()} configuration is saving...");

                try
                {
                    // Update the config data
                    await UpdateConfigAsync();

                    // Save the config data
                    ConfigData.Save();

                    RCF.Logger.LogInformationSource($"{Game.GetDisplayName()} configuration has been saved");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving ubi.ini data");
                    await RCF.MessageUI.DisplayMessageAsync($"An error occurred when saving your {Game.GetDisplayName()} configuration", "Error saving", MessageType.Error);
                    return;
                }

                // Run save code
                await OnSaveAsync();

                UnsavedChanges = false;

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("Your changes have been saved");

                OnSave();
            }
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Override to run on setup
        /// </summary>
        /// <returns>The task</returns>
        protected virtual Task OnSetupAsync() => Task.CompletedTask;

        /// <summary>
        /// Override to run on saving
        /// </summary>
        /// <returns>The task</returns>
        protected virtual Task OnSaveAsync() => Task.CompletedTask;

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Loads the <see cref="ConfigData"/>
        /// </summary>
        /// <returns>The config data</returns>
        protected abstract Task<Handler> LoadConfigAsync();

        /// <summary>
        /// Imports the <see cref="ConfigData"/>
        /// </summary>
        /// <returns>The task</returns>
        protected abstract Task ImportConfigAsync();

        /// <summary>
        /// Updates the <see cref="ConfigData"/>
        /// </summary>
        /// <returns>The task</returns>
        protected abstract Task UpdateConfigAsync();

        #endregion

        #region Protected Classes

        /// <summary>
        /// Provides support to duplicate a section
        /// in a ubi ini file
        /// </summary>
        protected class DuplicateSectionUbiIniHandler : UbiIniHandler
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="path">The path of the ubi.ini file</param>
            /// <param name="sectionKey">The name of the section to retrieve, usually the name of the game</param>
            public DuplicateSectionUbiIniHandler(FileSystemPath path, string sectionKey) : base(path, sectionKey)
            {

            }

            public void Duplicate(KeyDataCollection sectionData)
            {
                // Recreate the section
                ReCreate();

                // Add all new keys
                sectionData.ForEach(x => Section.AddKey(x));
            }
        }

        #endregion
    }
}