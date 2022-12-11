using System.Diagnostics.CodeAnalysis;
using System.Windows;
using RayCarrot.RCP.Metro.Games.Emulators.DosBox;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class DosBoxGameConfigViewModel : EmulatorConfigPageViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor for a specific game
    /// </summary>
    /// <param name="gameInstallation">The game installation</param>
    /// <param name="dosBoxDescriptor">The DOSBox emulator descriptor</param>
    public DosBoxGameConfigViewModel(GameInstallation gameInstallation, DosBoxEmulatorDescriptor dosBoxDescriptor) 
    {
        GameInstallation = gameInstallation;
        DosBoxDescriptor = dosBoxDescriptor;

        // Set up the available resolution values
        AvailableFullscreenResolutionValues = new ObservableCollection<string>();
        AvailableWindowedResolutionValues = new ObservableCollection<string>();

        const double ratio = 16d / 10d;
        const int minHeight = 200;
        double maxHeight = SystemParameters.PrimaryScreenHeight;

        AvailableFullscreenResolutionValues.Add("Original");
        AvailableFullscreenResolutionValues.Add("Desktop");
        AvailableWindowedResolutionValues.Add("Original");

        for (int height = minHeight; height <= maxHeight; height += minHeight)
        {
            AvailableFullscreenResolutionValues.Add($"{height * ratio}x{height}");
            AvailableWindowedResolutionValues.Add($"{height * ratio}x{height}");
        }

        // TODO: Rewrite this to be clearer. Localize common options like "default" and update localized tooltips to reflect this. Also sue ObservableCollection.
        // NOTE: Below options are not localized

        // Set available DosBox outputs
        AvailableDosBoxOutputs = new[]
        {
            "default",
            "surface",
            "overlay",
            "opengl",
            "openglnb",
            "ddraw"
        };

        // Set available DosBox scalers
        AvailableDosBoxScalers = new[]
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
        AvailableDosBoxCoreModes = new[]
        {
            "default",
            "normal",
            "simple",
            "dynamic",
            "auto"
        };

        // Set available DosBox cycle modes
        AvailableDosBoxCycleModes = new[]
        {
            "default",
            "auto",
            "max"
        };
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool? _fullscreenEnabled;
    private bool? _fullDoubleEnabled;
    private bool? _aspectCorrectionEnabled;
    private double? _memorySize;
    private double? _frameskip;
    private string? _selectedOutput;
    private string? _fullscreenResolution;
    private string? _windowedResolution;
    private string? _selectedScaler;
    private string? _selectedCoreMode;
    private string? _selectedCycles;
    private string? _customCommands;

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

    #region Public Properties

    public override LocalizedString PageName => DosBoxDescriptor.DisplayName;

    /// <summary>
    /// The game installation
    /// </summary>
    public GameInstallation GameInstallation { get; }

    /// <summary>
    /// The DOSBox emulator descriptor
    /// </summary>
    public DosBoxEmulatorDescriptor DosBoxDescriptor { get; }

    /// <summary>
    /// The available resolution values to use for fullscreen
    /// </summary>
    public ObservableCollection<string> AvailableFullscreenResolutionValues { get; }

    /// <summary>
    /// The available resolution values to use for windowed mode
    /// </summary>
    public ObservableCollection<string> AvailableWindowedResolutionValues { get; }

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
    public string? SelectedOutput
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
    public string? FullscreenResolution
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
    public string? WindowedResolution
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
    public string? SelectedScaler
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
    public string? SelectedCoreMode
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
    public string? SelectedCycles
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
    [DisallowNull]
    public string? CustomCommands
    {
        get => _customCommands;
        set
        {
            _customCommands = value.Replace('\"', '\'');
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicates if the option to use recommended options in the page is available
    /// </summary>
    public override bool CanUseRecommended => true;

    #endregion

    #region Protected Methods

    /// <summary>
    /// Loads and sets up the current configuration properties
    /// </summary>
    /// <returns>The task</returns>
    protected override Task LoadAsync()
    {
        Logger.Info("DOSBox emulator game config for {0} is being set up", GameInstallation.FullId);

        // Get the config manager
        var configManager = new AutoConfigManager(DosBoxDescriptor.GetGameConfigFile(GameInstallation));

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

        Logger.Info("DOSBox emulator game config for {0} has been loaded", GameInstallation.FullId);

        UnsavedChanges = false;

        return Task.CompletedTask;

        // Helper methods for getting properties
        bool? GetBool(string propName) =>
            Boolean.TryParse(configData.Configuration.TryGetValue(propName), out bool output) ? output : null;
        string? GetString(string propName, string? defaultValue = null) => 
            configData.Configuration.TryGetValue(propName) ?? defaultValue;
        double? GetDouble(string propName) => 
            Double.TryParse(configData.Configuration.TryGetValue(propName), out double output) ? output : null;
    }

    /// <summary>
    /// Saves the changes
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("DOSBox emulator game config for {0} is saving...", GameInstallation.FullId);

        try
        {
            // Get the config manager
            var configManager = new AutoConfigManager(DosBoxDescriptor.GetGameConfigFile(GameInstallation));

            // Create config data
            var configData = new AutoConfigData();

            // Add custom commands
            if (CustomCommands != null)
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
            configData.SectionNames.Add("sdl", new[]
            {
                FullScreenKey,
                FullDoubleKey,
                OutputKey,
                FullscreenResolutionKey,
                WindowedResolutionKey
            });
            configData.SectionNames.Add("render", new[]
            {
                AspectCorrectionKey,
                FrameskipKey,
                ScalerKey
            });
            configData.SectionNames.Add("dosbox", new[]
            {
                MemorySizeKey,
            });
            configData.SectionNames.Add("cpu", new[]
            {
                CoreKey,
                CyclesKey
            });

            // Write to the config file
            configManager.WriteFile(configData);

            Logger.Info("DOSBox emulator game config for {0} has been saved", GameInstallation.FullId);

            return true;

            // Helper methods for setting properties
            void SetProp(string propName, object? value, bool ignoreDefault = false)
            {
                if ((value != null && (!ignoreDefault || !value.ToString().Equals("default", StringComparison.CurrentCultureIgnoreCase))))
                    configData.Configuration[propName] = value.ToString();
                else if (configData.Configuration.ContainsKey(propName))
                    configData.Configuration.Remove(propName);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving DOSBox emulator game config data");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Config_DosBoxSaveError, Resources.Config_SaveErrorHeader);
            return false;
        }
    }

    /// <summary>
    /// Applies the recommended settings for the specified game
    /// </summary>
    protected override void UseRecommended()
    {
        AspectCorrectionEnabled = false;
        MemorySize = 30;
        Frameskip = 0;
        SelectedOutput = "default";
        SelectedCycles = "20000";

        Logger.Trace("Recommended DosBox settings were applied");
    }

    #endregion
}