using System;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.IO;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 1 configuration
    /// </summary>
    public class Rayman1ConfigViewModel : BaseDosBoxConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Constructor for using the default game, <see cref="Games.Rayman1"/>
        /// </summary>
        public Rayman1ConfigViewModel() : this(Games.Rayman1, true) { }

        /// <summary>
        /// Constructor for specifying a game
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="isMountLocationAvailable">Indicates if changing the game mount location is available</param>
        public Rayman1ConfigViewModel(Games game, bool isMountLocationAvailable) : base(game, GameType.DosBox) { }

        #endregion

        #region Private Fields

        private R1Languages _gameLanguage;

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected game language, if any
        /// </summary>
        public R1Languages GameLanguage
        {
            get => _gameLanguage;
            set
            {
                _gameLanguage = value;
                UnsavedChanges = true;
            }
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Performs additional setup for the game
        /// </summary>
        /// <returns>The task</returns>
        protected override Task SetupGameAsync()
        {
            // Default game language to not be available
            IsGameLanguageAvailable = false;

            var instDir = Game.GetInstallDir(false);

            // Attempt to get the game language from the .bat file or config file
            var configFile = instDir + "RAYMAN.CFG";

            if (configFile.FileExists)
            {
                var lang = GetR1Language(configFile);

                if (lang != null)
                {
                    GameLanguage = lang.Value;
                    IsGameLanguageAvailable = true;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs additional saving for the game
        /// </summary>
        /// <returns>The task</returns>
        protected override async Task SaveGameAsync()
        {
            // If game language is available, update it
            if (IsGameLanguageAvailable)
                await SetR1LanguageAsync(Game.GetInstallDir() + "RAYMAN.CFG", GameLanguage);
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the Rayman 1 language from the specified config file
        /// </summary>
        /// <param name="configFile">The config file to get the language from</param>
        /// <returns>The language or null if none was found</returns>
        private static R1Languages? GetR1Language(FileSystemPath configFile)
        {
            try
            {
                // Open the file
                using var stream = File.OpenRead(configFile);

                return stream.ReadByte() switch
                {
                    0 => R1Languages.English,
                    1 => R1Languages.French,
                    2 => R1Languages.German,
                    _ => (null as R1Languages?)
                };
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Rayman 1 language from config file");
                return null;
            }
        }

        /// <summary>
        /// Sets the Rayman 1 language in the specified config file
        /// </summary>
        /// <param name="configFile">The config file to set the language in</param>
        /// <param name="language">The language</param>
        /// <returns>The task</returns>
        private static async Task SetR1LanguageAsync(FileSystemPath configFile, R1Languages language)
        {
            try
            {
                using var stream = File.OpenWrite(configFile);

                stream.WriteByte((byte)language);
            }
            catch (Exception ex)
            {
                ex.HandleError("Setting Rayman 1 game language from config file");
                await WPF.Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.DosBoxConfig_SetLanguageError, Resources.DosBoxConfig_SetLanguageErrorHeader);
            }
        }

        #endregion
    }
}