﻿using System.IO;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.Ray1;
using BinarySerializer.Ray1.PC;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public abstract class Ray1BaseConfigViewModel : ConfigPageViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor for a specific game installation
    /// </summary>
    /// <param name="gameDescriptor">The game descriptor</param>
    /// <param name="gameInstallation">The game installation</param>
    /// <param name="engineVersion">The Rayman 1 engine version</param>
    protected Ray1BaseConfigViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation, Ray1EngineVersion engineVersion)
    {
        GameDescriptor = gameDescriptor;
        GameInstallation = gameInstallation;
        EngineVersion = engineVersion;
        IsGameLanguageAvailable = EngineVersion is Ray1EngineVersion.PC;
        IsVoicesVolumeAvailable = EngineVersion is Ray1EngineVersion.PC_Edu or Ray1EngineVersion.PC_Kit or Ray1EngineVersion.PC_Fan;

        FrameRateOptions_Values = new Freq[]
        {
            Freq.Freq_50,
            Freq.Freq_60,
            Freq.Freq_70,
            Freq.Freq_80,
            Freq.Freq_100,
            Freq.Freq_Max
        };
        FrameRateOptions_Names = new string[]
        {
            "50 hz",
            "60 hz",
            "70 hz",
            "80 hz",
            "100 hz",
            "Max",
        };

        void keyChanged() => UnsavedChanges = true;

        KeyItems = new ObservableCollection<ButtonMapperKeyItemViewModel>()
        {
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Left)), Key.NumPad4, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Up)), Key.NumPad8, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Right)), Key.NumPad6, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Down)), Key.NumPad2, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Jump)), Key.LeftCtrl, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Fist)), Key.LeftAlt, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Action)), Key.X, keyChanged),
        };
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private Language _gameLanguage;
    private bool _isMusicEnabled;
    private bool _isStero;
    private int _soundVolume;
    private int _voicesVolume;
    private bool _showBackground;
    private bool _showParallaxBackground;
    private bool _showHud;
    private int _selectedFrameRateOption;
    private int _zoneOfPlay;
    private int _gamePadJump;
    private int _gamePadFist;
    private int _gamePadAction;
    private int _xPadMax;
    private int _xPadMin;
    private int _yPadMax;
    private int _yPadMin;
    private int _xPadCenter;
    private int _yPadCenter;
    private int _port;
    private int _irq;
    private int _dma;
    private int _param;
    private int _deviceId;
    private int _numCard;

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the option to use recommended options in the page is available
    /// </summary>
    public override bool CanUseRecommended => true;

    /// <summary>
    /// The game descriptor
    /// </summary>
    public MsDosGameDescriptor GameDescriptor { get; }

    /// <summary>
    /// The game installation
    /// </summary>
    public GameInstallation GameInstallation { get; }

    /// <summary>
    /// The Rayman 1 engine version
    /// </summary>
    public Ray1EngineVersion EngineVersion { get; }

    /// <summary>
    /// The game configuration
    /// </summary>
    public ConfigFile? Config { get; set; }

    /// <summary>
    /// The file name for the config file
    /// </summary>
    public string? ConfigFileName { get; set; }

    /// <summary>
    /// The serializer context
    /// </summary>
    public Context? Context { get; set; }

    // Language

    /// <summary>
    /// Indicates if changing the game language is available
    /// </summary>
    public bool IsGameLanguageAvailable { get; }

    /// <summary>
    /// The selected game language, if any
    /// </summary>
    public Language GameLanguage
    {
        get => _gameLanguage;
        set
        {
            _gameLanguage = value;
            UnsavedChanges = true;
        }
    }

    // Sound

    public bool IsMusicEnabled
    {
        get => _isMusicEnabled;
        set
        {
            _isMusicEnabled = value;
            UnsavedChanges = true;
        }
    }

    public bool IsStero
    {
        get => _isStero;
        set
        {
            _isStero = value;
            UnsavedChanges = true;
        }
    }

    public int SoundVolume
    {
        get => _soundVolume;
        set
        {
            _soundVolume = value;
            UnsavedChanges = true;
        }
    }

    public bool IsVoicesVolumeAvailable { get; }

    public int VoicesVolume
    {
        get => _voicesVolume;
        set
        {
            _voicesVolume = value;
            UnsavedChanges = true;
        }
    }

    // Graphics

    public bool ShowBackground
    {
        get => _showBackground;
        set
        {
            _showBackground = value;
            UnsavedChanges = true;
        }
    }

    public bool ShowParallaxBackground
    {
        get => _showParallaxBackground;
        set
        {
            _showParallaxBackground = value;
            UnsavedChanges = true;
        }
    }

    public bool ShowHUD
    {
        get => _showHud;
        set
        {
            _showHud = value;
            UnsavedChanges = true;
        }
    }

    public Freq[] FrameRateOptions_Values { get; }
    public string[] FrameRateOptions_Names { get; }

    public int SelectedFrameRateOption
    {
        get => _selectedFrameRateOption;
        set
        {
            _selectedFrameRateOption = value;
            UnsavedChanges = true;
        }
    }

    public Freq FrameRate
    {
        get => FrameRateOptions_Values[SelectedFrameRateOption];
        set => SelectedFrameRateOption = FrameRateOptions_Values.FindItemIndex(x => x == value);
    }

    public int ZoneOfPlay
    {
        get => _zoneOfPlay;
        set
        {
            _zoneOfPlay = value;
            UnsavedChanges = true;
        }
    }

    // Controls

    /// <summary>
    /// The key items
    /// </summary>
    public ObservableCollection<ButtonMapperKeyItemViewModel> KeyItems { get; }

    public int GamePad_Jump
    {
        get => _gamePadJump;
        set
        {
            _gamePadJump = value;
            UnsavedChanges = true;
        }
    }

    public int GamePad_Fist
    {
        get => _gamePadFist;
        set
        {
            _gamePadFist = value;
            UnsavedChanges = true;
        }
    }

    public int GamePad_Action
    {
        get => _gamePadAction;
        set
        {
            _gamePadAction = value;
            UnsavedChanges = true;
        }
    }

    public int XPadMax
    {
        get => _xPadMax;
        set
        {
            _xPadMax = value;
            UnsavedChanges = true;
        }
    }

    public int XPadMin
    {
        get => _xPadMin;
        set
        {
            _xPadMin = value;
            UnsavedChanges = true;
        }
    }

    public int YPadMax
    {
        get => _yPadMax;
        set
        {
            _yPadMax = value;
            UnsavedChanges = true;
        }
    }

    public int YPadMin
    {
        get => _yPadMin;
        set
        {
            _yPadMin = value;
            UnsavedChanges = true;
        }
    }

    public int XPadCenter
    {
        get => _xPadCenter;
        set
        {
            _xPadCenter = value;
            UnsavedChanges = true;
        }
    }

    public int YPadCenter
    {
        get => _yPadCenter;
        set
        {
            _yPadCenter = value;
            UnsavedChanges = true;
        }
    }

    // Device

    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            UnsavedChanges = true;
        }
    }

    public int IRQ
    {
        get => _irq;
        set
        {
            _irq = value;
            UnsavedChanges = true;
        }
    }

    public int DMA
    {
        get => _dma;
        set
        {
            _dma = value;
            UnsavedChanges = true;
        }
    }

    public int Param
    {
        get => _param;
        set
        {
            _param = value;
            UnsavedChanges = true;
        }
    }

    public int DeviceID
    {
        get => _deviceId;
        set
        {
            _deviceId = value;
            UnsavedChanges = true;
        }
    }

    public int NumCard
    {
        get => _numCard;
        set
        {
            _numCard = value;
            UnsavedChanges = true;
        }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Loads and sets up the current configuration properties
    /// </summary>
    /// <returns>The task</returns>
    protected override Task LoadAsync()
    {
        Logger.Info("{0} config is being set up", GameInstallation.FullId);

        // Get the config file name
        ConfigFileName = GetConfigFileName();

        AddConfigLocation(LinkItemViewModel.LinkType.BinaryFile, GameInstallation.InstallLocation.Directory + ConfigFileName);

        // Create the context to use
        Context = new RCPContext(GameInstallation.InstallLocation.Directory);
        Context.AddSettings(new Ray1Settings(EngineVersion));
        Context.AddFile(new LinearFile(Context, ConfigFileName));

        // Read the file if it exists
        if (File.Exists(Context.GetAbsoluteFilePath(ConfigFileName)))
        {
            using (Context)
                Config = FileFactory.Read<ConfigFile>(Context, ConfigFileName);
        }
        else
        {
            // If no config file exists we create the config manually
            Config = CreateDefaultConfig();
        }

        // Read button mapping
        for (int i = 0; i < KeyItems.Count; i++)
        {
            var item = KeyItems[i];
            var key = Config.Tab_Key[i];

            item.SetInitialNewKey(DirectXKeyHelpers.GetKey(key));
        }

        // Read config values
        GameLanguage = Config.Language;
        IsMusicEnabled = Config.MusicVolume != 0;
        IsStero = Config.SteroEnabled != 0;
        SoundVolume = Config.SoundVolume;
        VoicesVolume = Config.VoicesVolume;

        ShowBackground = Config.BackgroundOptionOn;
        ShowParallaxBackground = Config.ScrollDiffOn;
        ShowHUD = Config.FixOn;
        FrameRate = Config.Frequence;
        ZoneOfPlay = Config.SizeScreen;

        GamePad_Jump = Config.JumpKey;
        GamePad_Fist = Config.FistKey;
        GamePad_Action = Config.ActionKey;
        XPadMax = Config.XPadMax;
        XPadMin = Config.XPadMin;
        YPadMax = Config.YPadMax;
        YPadMin = Config.YPadMin;
        XPadCenter = Config.XPadCentre;
        YPadCenter = Config.YPadCentre;

        Port = (int)Config.Port;
        IRQ = (int)Config.Irq;
        DMA = (int)Config.Dma;
        Param = (int)Config.Param;
        DeviceID = (int)Config.DeviceID;
        NumCard = Config.NumCard;

        UnsavedChanges = false;

        // Verify values. If these values are incorrect they will cause the game to run without sound effects with the default DOSBox configuration.
        if (Port != 544 || IRQ != 5 || DMA != 5 || DeviceID != 57368 || NumCard != 3)
        {
            Port = 544;
            IRQ = 5;
            DMA = 5;
            DeviceID = 57368;
            NumCard = 3;
            UnsavedChanges = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the changes
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("{0} config is saving...", GameInstallation.FullId);

        try
        {
            if (Config == null || Context == null || ConfigFileName == null)
                throw new Exception("Saving can not be done before the config has been loaded");

            // Set button mapping
            for (int i = 0; i < KeyItems.Count; i++)
            {
                var item = KeyItems[i];

                Config.Tab_Key[i] = (byte)DirectXKeyHelpers.GetKeyCode(item.NewKey);
            }

            // Set config values
            Config.Language = GameLanguage;
            Config.MusicVolume = (ushort)(IsMusicEnabled ? 1 : 0);
            Config.SteroEnabled = (ushort)(IsStero ? 1 : 0);
            Config.SoundVolume = (ushort)SoundVolume;
            Config.VoicesVolume = (ushort)VoicesVolume;

            Config.BackgroundOptionOn = ShowBackground;
            Config.ScrollDiffOn = ShowParallaxBackground;
            Config.FixOn = ShowHUD;
            Config.Frequence = FrameRate;
            Config.SizeScreen = (byte)ZoneOfPlay;

            Config.JumpKey = (ushort)GamePad_Jump;
            Config.FistKey = (ushort)GamePad_Fist;
            Config.ActionKey = (ushort)GamePad_Action;
            Config.XPadMax = (short)XPadMax;
            Config.XPadMin = (short)XPadMin;
            Config.YPadMax = (short)YPadMax;
            Config.YPadMin = (short)YPadMin;
            Config.XPadCentre = (short)XPadCenter;
            Config.YPadCentre = (short)YPadCenter;

            Config.Port = (uint)Port;
            Config.Irq = (uint)IRQ;
            Config.Dma = (uint)DMA;
            Config.Param = (uint)Param;
            Config.DeviceID = (uint)DeviceID;
            Config.NumCard = (byte)NumCard;

            using (Context)
                FileFactory.Write(Context, ConfigFileName, Config);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving {0} configuration data", GameInstallation.FullId);

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, GameInstallation.GetDisplayName()), Resources.Config_SaveErrorHeader);
            return false;
        }
    }

    protected override void UseRecommended()
    {
        IsMusicEnabled = true;
        IsStero = true;
        ShowBackground = true;
        ShowParallaxBackground = true;
        FrameRate = Freq.Freq_60;
        ZoneOfPlay = 0;
        Port = 544;
        IRQ = 5;
        DMA = 5;
        DeviceID = 57368;
        NumCard = 3;
    }

    #endregion

    #region Public Methods

    public abstract string GetConfigFileName();

    public override void Dispose()
    {
        // Dispose base
        base.Dispose();

        Context?.Dispose();
        KeyItems.DisposeAll();
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Creates a new instance of <see cref="PC_ConfigFile"/> with default values for the specific game
    /// </summary>
    /// <returns>The config instance</returns>
    protected ConfigFile CreateDefaultConfig()
    {
        return new ConfigFile
        {
            Language = Language.English,
            Port = 544,
            Irq = 5,
            Dma = 5,
            Param = 0,
            DeviceID = 57368,
            NumCard = 3,
            JumpKey = 1,
            FistKey = 0,
            UnusedKey = 3,
            ActionKey = 2,
            MusicVolume = 1,
            SoundVolume = 18,
            SteroEnabled = 1,
            VoicesVolume = 18,
            Mode_Pad = false,
            Port_Pad = 0,
            XPadMax = 1610,
            XPadMin = 35,
            YPadMax = 1610,
            YPadMin = 35,
            XPadCentre = 830,
            YPadCentre = 830,
            NotBut = new byte[]
            {
                0x00, 0x00, 0x00, 0x00
            },
            Tab_Key = new byte[]
            {
                0x4B, 0x48, 0x4D, 0x50, 0x1D, 0x38, 0x2D
            },
            GameModeVideo = 0,
            P486 = 0,
            SizeScreen = 0,
            Frequence = 0,
            FixOn = true,
            BackgroundOptionOn = true,
            ScrollDiffOn = true,
            RefRam2VramNormalFix = new ushort[8],
            RefRam2VramNormal = new ushort[8],
            RefTransFondNormal = new ushort[8],
            RefSpriteNormal = new ushort[2],
            RefRam2VramX = new ushort[2],
            RefVram2VramX = new ushort[2],
            RefSpriteX = new ushort[2]
        };
    }

    #endregion
}