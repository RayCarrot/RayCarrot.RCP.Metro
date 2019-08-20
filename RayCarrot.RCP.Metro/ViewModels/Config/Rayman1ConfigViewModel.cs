using System;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

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
        public Rayman1ConfigViewModel() : base(Games.Rayman1)
        {
            IsMountLocationAvailable = true;
        }

        #endregion

        #region Private Fields

        private FileSystemPath _mountPath;

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

        /// <summary>
        /// The file or directory to mount
        /// </summary>
        public FileSystemPath MountPath
        {
            get => _mountPath;
            set
            {
                _mountPath = value;
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
            // Get the current DosBox options for the specified game
            var options = Data.DosBoxGames[Game];

            MountPath = options.MountPath;

            // Default game language to not be available
            IsGameLanguageAvailable = false;

            var instDir = Game.GetInfo().InstallDirectory;

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
            // Get the current DosBox options for the specified game
            var options = Data.DosBoxGames[Game];

            options.MountPath = MountPath;

            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, false, false, false, true));

            // If game language is available, update it
            if (IsGameLanguageAvailable)
                await SetR1LanguageAsync(Game.GetInfo().InstallDirectory + "RAYMAN.CFG", GameLanguage);
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
                using (var stream = File.OpenRead(configFile))
                {
                    switch (stream.ReadByte())
                    {
                        case 0:
                            return R1Languages.English;

                        case 1:
                            return R1Languages.French;

                        case 2:
                            return R1Languages.German;

                        default:
                            return null;
                    }
                }
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
                using (var stream = File.OpenWrite(configFile))
                    stream.WriteByte((byte)language);
            }
            catch (Exception ex)
            {
                ex.HandleError("Setting Rayman 1 game language from config file");
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.DosBoxConfig_SetLanguageError, Resources.DosBoxConfig_SetLanguageErrorHeader, MessageType.Error);
            }
        }

        #endregion
    }
}