using Nito.AsyncEx;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public abstract class Ray_1_KIT_EDU_BaseConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor for a specific game
        /// </summary>
        /// <param name="game">The DosBox game</param>
        /// <param name="ray1Game">The Rayman 1 game</param>
        /// <param name="langMode">The language mode to use</param>
        protected Ray_1_KIT_EDU_BaseConfigViewModel(Games game, Ray1Game ray1Game, LanguageMode langMode)
        {
            Game = game;
            Ray1Game = ray1Game;
            LangMode = langMode;

            // Create the async lock
            AsyncLock = new AsyncLock();

            // Create the commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        #endregion

        #region Private Fields

        private R1Languages _gameLanguage;

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
        /// The Rayman 1 game
        /// </summary>
        public Ray1Game Ray1Game { get; }

        /// <summary>
        /// The language mode to use
        /// </summary>
        public LanguageMode LangMode { get; }

        /// <summary>
        /// The game configuration
        /// </summary>
        public Rayman1PCConfigData Config { get; set; }

        /// <summary>
        /// The file path for the config file
        /// </summary>
        public FileSystemPath ConfigFilePath { get; set; }

        /// <summary>
        /// Indicates if changing the game language is available
        /// </summary>
        public bool IsGameLanguageAvailable { get; set; }

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

        #region Commands

        public AsyncRelayCommand SaveCommand { get; }

        #endregion

        #region Public Methods

        public abstract FileSystemPath GetConfigPath();

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override Task SetupAsync()
        {
            RL.Logger?.LogInformationSource($"{Game} config is being set up");

            ConfigFilePath = GetConfigPath();

            if (ConfigFilePath.FileExists)
            {
                // If a config file exists we read it
                Config = BinarySerializableHelpers.ReadFromFile<Rayman1PCConfigData>(ConfigFilePath, Ray1Settings.GetDefaultSettings(Ray1Game, Platform.PC), App.GetBinarySerializerLogger(ConfigFilePath.Name));

                // Default all languages to be available. Sadly there is no way to determine which languages a specific release can use as most releases have all languages in the files, but have it hard-coded in the exe to only pick a specific one.
                IsEnglishAvailable = true;
                IsFrenchAvailable = true;
                IsGermanAvailable = true;
            }
            else
            {
                // If no config file exists we create the config manually
                Config = CreateDefaultConfig();
            }

            // Default game language to not be available
            IsGameLanguageAvailable = false;

            if (LangMode == LanguageMode.Config)
            {
                // Get the language from the config file
                GameLanguage = Config.Language;
                IsGameLanguageAvailable = true;
            }
            else if (LangMode == LanguageMode.Argument)
            {
                // Get the game install directory
                var installDir = Game.GetInstallDir();

                // Attempt to get the game language from the .bat file
                var batchFile = installDir + Game.GetGameInfo().DefaultFileName;

                if (batchFile.FullPath.EndsWith(".bat", StringComparison.InvariantCultureIgnoreCase) && batchFile.FileExists)
                {
                    // Check language availability
                    var pcmapDir = installDir + "pcmap";

                    // IDEA: Read from VERSION file instead
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
            }

            UnsavedChanges = false;

            return Task.CompletedTask;
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
                    // If game language is available, update it
                    if (IsGameLanguageAvailable)
                    {
                        if (LangMode == LanguageMode.Config)
                            Config.Language = GameLanguage;
                        else if (LangMode == LanguageMode.Argument)
                            await SetBatchFileLanguageAsync(Game.GetInstallDir() + Game.GetGameInfo().DefaultFileName, GameLanguage, Game);
                    }

                    // Save the config file
                    BinarySerializableHelpers.WriteToFile(Config, ConfigFilePath, Ray1Settings.GetDefaultSettings(Ray1Game, Platform.PC), App.GetBinarySerializerLogger(ConfigFilePath.Name));
                }
                catch (Exception ex)
                {
                    ex.HandleError($"Saving {Game} configuration data");

                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, Game.GetGameInfo().DisplayName), Resources.Config_SaveErrorHeader);
                    return;
                }

                UnsavedChanges = false;

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                OnSave();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates a new instance of <see cref="Rayman1PCConfigData"/> with default values for the specific game
        /// </summary>
        /// <returns>The config instance</returns>
        protected Rayman1PCConfigData CreateDefaultConfig()
        {
            // TODO-UPDATE: Set default values - perhaps also correct values when saving?
            return new Rayman1PCConfigData
            {
                Language = R1Languages.English,
                Port = 0,
                Irq = 0,
                Dma = 0,
                Param = 0,
                DeviceID = 0,
                NumCard = 0,
                KeyJump = 0,
                KeyWeapon = 0,
                Options_jeu_10 = 0,
                KeyAction = 0,
                MusicCdActive = 0,
                VolumeSound = 0,
                Options_jeu_14 = 0,
                EDU_VoiceSound = 0,
                Mode_Pad = 0,
                Port_Pad = 0,
                XPadMax = 0,
                XPadMin = 0,
                YPadMax = 0,
                YPadMin = 0,
                XPadCentre = 0,
                YPadCentre = 0,
                NotBut = new byte[]
                {
                },
                Tab_Key = new byte[]
                {
                },
                GameModeVideo = 0,
                P486 = 0,
                SizeScreen = 0,
                Frequence = 0,
                FixOn = false,
                BackgroundOptionOn = false,
                ScrollDiffOn = false,
                RefRam2VramNormalFix = new ushort[]
                {
                },
                RefRam2VramNormal = new ushort[]
                {
                },
                RefTransFondNormal = new ushort[]
                {
                },
                RefSpriteNormal = 0,
                RefRam2VramX = 0,
                RefVram2VramX = 0,
                RefSpriteX = 0
            };
        }

        /// <summary>
        /// Gets the current game language from the specified batch file
        /// </summary>
        /// <param name="batchFile">The batch file to get the language from</param>
        /// <returns>The language or null if none was found</returns>
        protected R1Languages? GetBatchFileanguage(FileSystemPath batchFile)
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
                ex.HandleError($"Getting {Game} language from batch file");
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
        protected async Task SetBatchFileLanguageAsync(FileSystemPath batchFile, R1Languages language, Games game)
        {
            try
            {
                var lang = language switch
                {
                    R1Languages.English => "usa",
                    R1Languages.French => "fr",
                    R1Languages.German => "al",
                    _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
                };

                // Delete the existing file
                RCPServices.File.DeleteFile(batchFile);

                // Create the .bat file
                File.WriteAllLines(batchFile, new string[]
                {
                    "@echo off",
                    $"{Path.GetFileNameWithoutExtension(game.GetManager<RCPDOSBoxGame>().ExecutableName)} ver={lang}"
                });
            }
            catch (Exception ex)
            {
                ex.HandleError($"Setting {Game} language from batch file");
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.DosBoxConfig_SetLanguageError, Resources.DosBoxConfig_SetLanguageErrorHeader);
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The available ways for Rayman 1 games to store the language setting
        /// </summary>
        public enum LanguageMode
        {
            /// <summary>
            /// The language is stored in the config file
            /// </summary>
            Config,

            /// <summary>
            /// The language is set from a launch argument
            /// </summary>
            Argument,

            /// <summary>
            /// The game does not allow custom languages
            /// </summary>
            None
        }

        #endregion
    }
}