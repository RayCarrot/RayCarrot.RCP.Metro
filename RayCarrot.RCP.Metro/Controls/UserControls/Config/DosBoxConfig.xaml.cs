using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for DosBoxConfig.xaml
    /// </summary>
    public partial class DosBoxConfig : BaseUserControl<DosBoxConfigViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window">The parent window</param>
        /// <param name="game">The DosBox game</param>
        public DosBoxConfig(Window window, Games game) : base(new DosBoxConfigViewModel(game))
        {
            InitializeComponent();
            ParentWindow = window;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The parent window
        /// </summary>
        private Window ParentWindow { get; }

        #endregion

        #region Event Handlers

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Close();
        }

        #endregion
    }

    /// <summary>
    /// View model for DosBox configuration
    /// </summary>
    public class DosBoxConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor without default game <see cref="Games.Rayman1"/>
        /// </summary>
        public DosBoxConfigViewModel() : this(Games.Rayman1)
        { }

        /// <summary>
        /// Default constructor for a specific game
        /// </summary>
        /// <param name="game">The DosBox game</param>
        public DosBoxConfigViewModel(Games game)
        {
            Game = game;

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

            for (int height = minHeight; height <= maxHeight; height = height + minHeight)
                AvailableResolutionValues.Add($"{height * ratio}x{height}");
        }

        #endregion

        #region Private Fields

        private FileSystemPath _mountPath;

        private bool? _fullscreenEnabled;

        private bool? _fullDoubleEnabled;

        private string _customCommands;

        private string _fullscreenResolution;

        private string _windowedResolution;

        #endregion

        #region Private Constants

        private const string FullScreenKey = "fullscreen";

        private const string FullDoubleKey = "fulldouble";

        private const string FullscreenResolutionKey = "fullresolution";

        private const string WindowedResolutionKey = "windowresolution";

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
        /// The available resolution values to use
        /// </summary>
        public ObservableCollection<string> AvailableResolutionValues { get; }

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
        public override Task SetupAsync()
        {
            // Get the current DosBox options for the specified game
            var options = RCFRCP.Data.DosBoxGames[Game];

            MountPath = options.MountPath;
            CustomCommands = options.Commands.JoinItems(Environment.NewLine);

            FullscreenEnabled = GetBool(FullScreenKey);
            FullDoubleEnabled = GetBool(FullDoubleKey);
            FullscreenResolution = GetString(FullscreenResolutionKey, "Original");
            WindowedResolution = GetString(WindowedResolutionKey, "Original");

            UnsavedChanges = false;

            return Task.CompletedTask;

            // Helper methods for getting properties
            bool? GetBool(string propName) => Boolean.TryParse(options.ConfigCommands.TryGetValue(propName), out bool output) ? (bool?)output : null;
            string GetString(string propName, string defaultValue = null) => options.ConfigCommands.TryGetValue(propName) ?? defaultValue;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"DosBox configuration for {Game} is saving...");

                try
                {
                    // Get the current DosBox options for the specified game
                    var options = RCFRCP.Data.DosBoxGames[Game];

                    options.MountPath = MountPath;
                    options.Commands = CustomCommands.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    SetProp(FullScreenKey, FullscreenEnabled);
                    SetProp(FullDoubleKey, FullDoubleEnabled);
                    SetProp(FullscreenResolutionKey, FullscreenResolution);
                    SetProp(WindowedResolutionKey, WindowedResolution);

                    RCF.Logger.LogInformationSource($"DosBox configuration for {Game} has been saved");

                    // Helper methods for setting properties
                    void SetProp(string propName, object value)
                    {
                        if (value == null && options.ConfigCommands.ContainsKey(propName))
                            options.ConfigCommands.Remove(propName);
                        else if (value != null)
                            options.ConfigCommands[propName] = value.ToString();
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving DosBox configuration data");
                    await RCF.MessageUI.DisplayMessageAsync("An error occurred when saving your DosBox configuration", "Error saving", MessageType.Error);
                    return;
                }

                UnsavedChanges = false;

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("Your changes have been saved");
            }
        }

        /// <summary>
        /// Applies the recommended settings for the specified game
        /// </summary>
        public void UseRecommended()
        {
            // TODO: Fill out
        }

        #endregion
    }

    /*
     CONFIG -set 'memsize=30'
     CONFIG -set 'frameskip=1'
     CONFIG -set 'cycles=20000'
     
     [sdl]
       # output -- What to use for output: surface,overlay,opengl,openglnb,ddraw.
       
       output=overlay
       
       [dosbox]
       # memsize -- Amount of memory DOSBox has in megabytes.
       
       memsize=16
       
       [render]
       # frameskip -- How many frames DOSBox skips before drawing one.
       # aspect -- Do aspect correction, if your output method doesn't support scaling this can slow things down!.
       # scaler -- Scaler used to enlarge/enhance low resolution modes.
       #           Supported are none,normal2x,normal3x,advmame2x,advmame3x,hq2x,hq3x,
       #                         2xsai,super2xsai,supereagle,advinterp2x,advinterp3x,
       #                         tv2x,tv3x,rgb2x,rgb3x,scan2x,scan3x.
       #           If forced is appended (like scaler=hq2x forced), the scaler will be used
       #           even if the result might not be desired.
       
       frameskip=0
       aspect=false
       scaler=normal2x
       
       [cpu]
       # core -- CPU Core used in emulation: normal,simple,dynamic,auto.
       #         auto switches from normal to dynamic if appropriate.
       # cycles -- Amount of instructions DOSBox tries to emulate each millisecond.
       #           Setting this value too high results in sound dropouts and lags.
       #           You can also let DOSBox guess the correct value by setting it to max.
       #           The default setting (auto) switches to max if appropriate.
       
       core=normal
       cycles=80000
          
     */
}