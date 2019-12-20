using System;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Designer configuration
    /// </summary>
    public class RaymanDesignerConfigViewModel : BaseDosBoxConfigViewModel
    {
        #region Constructors

        /// <summary>
        /// Constructor for using the default game, <see cref="Games.RaymanDesigner"/>
        /// </summary>
        public RaymanDesignerConfigViewModel() : this(Games.RaymanDesigner)
        {
            
        }

        /// <summary>
        /// Constructor for specifying a game
        /// </summary>
        /// <paramref name="game">The game</paramref>
        public RaymanDesignerConfigViewModel(Games game) : base(game, GameType.DosBox)
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
        /// Indicates if <see cref="R1Languages.English"/> is available
        /// </summary>
        public bool IsEnglishAvailable { get; set; }

        /// <summary>
        /// Indicates if <see cref="R1Languages.French"/> is available
        /// </summary>
        public bool IsFrenchAvailable { get; set; }

        /// <summary>
        /// Indicates if <see cref="R1Languages.German"/> is available
        /// </summary>
        public bool IsGermanAvailable { get; set; }

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

            var instDir = Game.GetInstallDir(false);

            // Attempt to get the game language from the .bat file or config file
            var batchFile = instDir + Game.GetGameInfo().DefaultFileName;

            if (batchFile.FullPath.EndsWith(".bat", StringComparison.InvariantCultureIgnoreCase) && batchFile.FileExists)
            {
                // Check language availability
                var pcmapDir = instDir + "pcmap";

                IsEnglishAvailable = (pcmapDir + "usa").DirectoryExists;
                IsFrenchAvailable = (pcmapDir + "fr").DirectoryExists;
                IsGermanAvailable = (pcmapDir + "al").DirectoryExists;

                // Make sure at least one language is available
                if (IsEnglishAvailable || IsFrenchAvailable || IsGermanAvailable)
                {
                    var lang = GetBatchFileanguage(batchFile);

                    if (lang != null)
                    {
                        GameLanguage = lang.Value;
                        IsGameLanguageAvailable = true;
                    }
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
                await SetBatchFileLanguageAsync(Game.GetInstallDir() + Game.GetGameInfo().DefaultFileName, GameLanguage, Game);
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the current game language from the specified batch file
        /// </summary>
        /// <param name="batchFile">The batch file to get the language from</param>
        /// <returns>The language or null if none was found</returns>
        private static R1Languages? GetBatchFileanguage(FileSystemPath batchFile)
        {
            try
            {
                // Read the file into an array
                var file = File.ReadAllLines(batchFile);

                // Check each line for the launch argument
                foreach (string line in file)
                {
                    // Find the argument
                    var index = line.IndexOf("ver=", StringComparison.Ordinal);

                    if (index == -1)
                        continue;

                    string lang = line.Substring(index + 4);

                    if (lang.Equals("usa", StringComparison.InvariantCultureIgnoreCase))
                        return R1Languages.English;

                    if (lang.Equals("fr", StringComparison.InvariantCultureIgnoreCase))
                        return R1Languages.French;

                    if (lang.Equals("al", StringComparison.InvariantCultureIgnoreCase))
                        return R1Languages.German;

                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting DOSBox game language from batch file");
                return null;
            }
        }

        /// <summary>
        /// Sets the current game language in the specified batch file
        /// </summary>
        /// <param name="batchFile">The batch file to set the language in</param>
        /// <param name="language">The language</param>
        /// <param name="game">The game to set the language for</param>
        /// <returns>The task</returns>
        private static async Task SetBatchFileLanguageAsync(FileSystemPath batchFile, R1Languages language, Games game)
        {
            try
            {
                string lang;

                switch (language)
                {
                    case R1Languages.English:
                        lang = "usa";
                        break;

                    case R1Languages.French:
                        lang = "fr";
                        break;

                    case R1Languages.German:
                        lang = "al";
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(language), language, null);
                }

                // Delete the existing file
                RCFRCP.File.DeleteFile(batchFile);

                // Create the .bat file
                File.WriteAllLines(batchFile, new string[]
                {
                    "@echo off",
                    $"{Path.GetFileNameWithoutExtension(game.GetManager<RCPDOSBoxGame>().ExecutableName)} ver={lang}"
                });
            }
            catch (Exception ex)
            {
                ex.HandleError("Setting DOSBox game language from batch file");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.DosBoxConfig_SetLanguageError, Resources.DosBoxConfig_SetLanguageErrorHeader);
            }
        }

        #endregion
    }
}