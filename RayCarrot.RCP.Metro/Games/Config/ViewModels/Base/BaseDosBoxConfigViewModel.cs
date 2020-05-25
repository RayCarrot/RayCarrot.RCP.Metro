using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Nito.AsyncEx;
using RayCarrot.Common;
using RayCarrot.Logging;
using RayCarrot.UI;

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
            UseRecommendedCommand = new RelayCommand(UseRecommended);

            // Set up the available resolution values
            AvailableResolutionValues = new ObservableCollection<string>();

            const double ratio = 16d / 10d;
            const int minHeight = 200;
            double maxHeight = SystemParameters.PrimaryScreenHeight;

            AvailableResolutionValues.Add($"Original");

            for (int height = minHeight; height <= maxHeight; height += minHeight)
                AvailableResolutionValues.Add($"{height * ratio}x{height}");

            // NOTE: Below options are not localized

            // Set available DosBox outputs
            AvailableDosBoxOutputs = new string[]
            {
                "default",
                "surface",
                "overlay",
                "opengl",
                "openglnb",
                "ddraw"
            };

            // Set available DosBox scalers
            AvailableDosBoxScalers = new string[]
            {
                "default",
                "none",
                "normal2x",
                "normal3x",
                "advmame2x",
                "advmame3x",
                "hq2x",
                "hq3x",
                "2xsai",
                "super2xsai",
                "supereagle",
                "advinterp2x",
                "advinterp3x",
                "tv2x",
                "tv3x",
                "rgb2x",
                "rgb3x",
                "scan2x",
                "scan3x"
            };

            // Set available DosBox core modes
            AvailableDosBoxCoreModes = new string[]
            {
                "default",
                "normal",
                "simple",
                "dynamic",
                "auto"
            };

            // Set available DosBox cycle modes
            AvailableDosBoxCycleModes = new string[]
            {
                "default",
                "auto",
                "max"
            };
        }

        #endregion

        #region Private Fields

        private bool? _fullscreenEnabled;

        private bool? _fullDoubleEnabled;

        private bool? _aspectCorrectionEnabled;

        private double? _memorySize;

        private double? _frameskip;

        private string _selectedOutput;

        private string _fullscreenResolution;

        private string _windowedResolution;

        private string _selectedScaler;

        private string _selectedCoreMode;

        private string _selectedCycles;

        private string _customCommands;

        #endregion

        #region Private Constants

        public const string FullScreenKey = "fullscreen";

        public const string FullDoubleKey = "fulldouble";

        public const string AspectCorrectionKey = "aspect";

        public const string MemorySizeKey = "memsize";

        public const string FrameskipKey = "frameskip";

        public const string OutputKey = "output";

        public const string FullscreenResolutionKey = "fullresolution";

        public const string WindowedResolutionKey = "windowresolution";

        public const string ScalerKey = "scaler";

        public const string CoreKey = "core";

        public const string CyclesKey = "cycles";

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
        /// The available resolution values to use
        /// </summary>
        public ObservableCollection<string> AvailableResolutionValues { get; }

        /// <summary>
        /// The available DosBox outputs to use
        /// </summary>
        public string[] AvailableDosBoxOutputs { get; }

        /// <summary>
        /// The available DosBox scalers to use
        /// </summary>
        public string[] AvailableDosBoxScalers { get; }

        /// <summary>
        /// The available DosBox core modes to use
        /// </summary>
        public string[] AvailableDosBoxCoreModes { get; }

        /// <summary>
        /// The available DosBox cycle modes to use
        /// </summary>
        public string[] AvailableDosBoxCycleModes { get; }

        /// <summary>
        /// Indicates if fullscreen has been enabled
        /// </summary>
        public bool? FullscreenEnabled
        {
            get => _fullscreenEnabled;
            set
            {
                _fullscreenEnabled = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if fullscreen double buffering has been enabled
        /// </summary>
        public bool? FullDoubleEnabled
        {
            get => _fullDoubleEnabled;
            set
            {
                _fullDoubleEnabled = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if aspect ratio correction has been enabled
        /// </summary>
        public bool? AspectCorrectionEnabled
        {
            get => _aspectCorrectionEnabled;
            set
            {
                _aspectCorrectionEnabled = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected memory size
        /// </summary>
        public double? MemorySize
        {
            get => _memorySize;
            set
            {
                _memorySize = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected frame skip value
        /// </summary>
        public double? Frameskip
        {
            get => _frameskip;
            set
            {
                _frameskip = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected output to use
        /// </summary>
        public string SelectedOutput
        {
            get => _selectedOutput;
            set
            {
                _selectedOutput = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected fullscreen resolution
        /// </summary>
        public string FullscreenResolution
        {
            get => _fullscreenResolution;
            set
            {
                _fullscreenResolution = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected fullscreen resolution
        /// </summary>
        public string WindowedResolution
        {
            get => _windowedResolution;
            set
            {
                _windowedResolution = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected scaler to use
        /// </summary>
        public string SelectedScaler
        {
            get => _selectedScaler;
            set
            {
                _selectedScaler = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected core mode to use
        /// </summary>
        public string SelectedCoreMode
        {
            get => _selectedCoreMode;
            set
            {
                _selectedCoreMode = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected cycles to use
        /// </summary>
        public string SelectedCycles
        {
            get => _selectedCycles;
            set
            {
                _selectedCycles = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The custom DosBox commands to use
        /// </summary>
        public string CustomCommands
        {
            get => _customCommands;
            set
            {
                _customCommands = value.Replace('\"', '\'');
                UnsavedChanges = true;
            }
        }

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

        public RelayCommand UseRecommendedCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override async Task SetupAsync()
        {
            RL.Logger?.LogInformationSource($"{Game} config is being set up");

            // Get the config manager
            var configManager = new DosBoxAutoConfigManager(Game.GetManager<RCPDOSBoxGame>(GameType).DosBoxConfigFile);

            // Create the file
            configManager.Create();

            // Read the content
            var configData = configManager.ReadFile();

            FullscreenEnabled = GetBool(FullScreenKey);
            FullDoubleEnabled = GetBool(FullDoubleKey);
            AspectCorrectionEnabled = GetBool(AspectCorrectionKey);
            MemorySize = GetDouble(MemorySizeKey);
            Frameskip = GetDouble(FrameskipKey);
            SelectedOutput = GetString(OutputKey, "default");
            FullscreenResolution = GetString(FullscreenResolutionKey, "Original");
            WindowedResolution = GetString(WindowedResolutionKey, "Original");
            SelectedScaler = GetString(ScalerKey, "default");
            SelectedCoreMode = GetString(CoreKey, "default");
            SelectedCycles = GetString(CyclesKey, "default");

            CustomCommands = configData.CustomLines.JoinItems(Environment.NewLine);

            RL.Logger?.LogInformationSource($"DosBox configuration for {Game} has been loaded");

            RL.Logger?.LogInformationSource($"Additional setup for {Game} is being processed");

            // Perform addition setup
            await SetupGameAsync();

            RL.Logger?.LogInformationSource($"Additional setup for {Game} has been processed");

            UnsavedChanges = false;

            // Helper methods for getting properties
            bool? GetBool(string propName) => Boolean.TryParse(configData.Configuration.TryGetValue(propName), out bool output) ? (bool?)output : null;
            string GetString(string propName, string defaultValue = null) => configData.Configuration.TryGetValue(propName) ?? defaultValue;
            double? GetDouble(string propName) => Double.TryParse(configData.Configuration.TryGetValue(propName), out double output) ? (double?)output : null;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RL.Logger?.LogInformationSource($"DosBox configuration for {Game} is saving...");

                try
                {
                    // Perform additional saving
                    await SaveGameAsync();

                    // Get the config manager
                    var configManager = new DosBoxAutoConfigManager(Game.GetManager<RCPDOSBoxGame>().DosBoxConfigFile);

                    // Create config data
                    var configData = new DosBoxAutoConfigData();

                    // Add custom commands
                    configData.CustomLines.AddRange(CustomCommands.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

                    // Add commands
                    SetProp(FullScreenKey, FullscreenEnabled);
                    SetProp(FullDoubleKey, FullDoubleEnabled);
                    SetProp(AspectCorrectionKey, AspectCorrectionEnabled);
                    SetProp(MemorySizeKey, MemorySize);
                    SetProp(FrameskipKey, Frameskip);
                    SetProp(OutputKey, SelectedOutput, true);
                    SetProp(FullscreenResolutionKey, FullscreenResolution);
                    SetProp(WindowedResolutionKey, WindowedResolution);
                    SetProp(ScalerKey, SelectedScaler, true);
                    SetProp(CoreKey, SelectedCoreMode, true);
                    SetProp(CyclesKey, SelectedCycles, true);

                    // Add section names
                    configData.SectionNames.Add("sdl", new []
                    {
                        FullScreenKey,
                        FullDoubleKey,
                        OutputKey,
                        FullscreenResolutionKey,
                        WindowedResolutionKey
                    });
                    configData.SectionNames.Add("render", new []
                    {
                        AspectCorrectionKey,
                        FrameskipKey,
                        ScalerKey
                    });
                    configData.SectionNames.Add("dosbox", new []
                    {
                        MemorySizeKey,
                    });
                    configData.SectionNames.Add("cpu", new []
                    {
                        CoreKey,
                        CyclesKey
                    });

                    // Write to the config file
                    configManager.WriteFile(configData);

                    RL.Logger?.LogInformationSource($"DosBox configuration for {Game} has been saved");

                    // Helper methods for setting properties
                    void SetProp(string propName, object value, bool ignoreDefault = false)
                    {
                        if ((value != null && (!ignoreDefault || !value.ToString().Equals("default", StringComparison.CurrentCultureIgnoreCase))))
                            configData.Configuration[propName] = value.ToString();
                        else if (configData.Configuration.ContainsKey(propName))
                            configData.Configuration.Remove(propName);
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving DosBox configuration data");
                    await WPF.Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Config_DosBoxSaveError, Resources.Config_SaveErrorHeader);
                    return;
                }

                UnsavedChanges = false;

                await WPF.Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                OnSave();
            }
        }

        /// <summary>
        /// Applies the recommended settings for the specified game
        /// </summary>
        public void UseRecommended()
        {
            AspectCorrectionEnabled = false;
            MemorySize = 30;
            Frameskip = 0;
            SelectedOutput = "default";
            SelectedCycles = "20000";

            RL.Logger?.LogTraceSource($"Recommended DosBox settings were applied");
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