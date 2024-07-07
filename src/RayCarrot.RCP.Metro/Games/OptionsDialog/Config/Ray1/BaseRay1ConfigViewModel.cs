using System.IO;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.Ray1;
using BinarySerializer.Ray1.PC;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public abstract class BaseRay1ConfigViewModel : ConfigPageViewModel
{
    #region Constructor

    protected BaseRay1ConfigViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation, Ray1EngineVersion engineVersion)
    {
        GameDescriptor = gameDescriptor;
        GameInstallation = gameInstallation;
        EngineVersion = engineVersion;
        IsGameLanguageAvailable = EngineVersion is Ray1EngineVersion.PC;
        IsVoicesVolumeAvailable = EngineVersion is Ray1EngineVersion.PC_Edu or Ray1EngineVersion.PC_Kit or Ray1EngineVersion.PC_Fan;

        FrameRateOptions_Values = new[]
        {
            Freq.Freq_50,
            Freq.Freq_60,
            Freq.Freq_70,
            Freq.Freq_80,
            Freq.Freq_100,
            Freq.Freq_Max
        };
        FrameRateOptions_Names = new[]
        {
            "50 hz",
            "60 hz",
            "70 hz",
            "80 hz",
            "100 hz",
            "Max",
        };

        Keys = new ObservableCollection<ButtonMapperKeyItemViewModel>()
        {
            new(new ResourceLocString(nameof(Resources.Config_Action_Left)), Key.NumPad4, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Up)), Key.NumPad8, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Right)), Key.NumPad6, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Down)), Key.NumPad2, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Jump)), Key.LeftCtrl, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Fist)), Key.LeftAlt, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Action)), Key.X, keyChanged),
        };

        void keyChanged() => UnsavedChanges = true;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override bool CanUseRecommended => true;
    public MsDosGameDescriptor GameDescriptor { get; }
    public GameInstallation GameInstallation { get; }
    public Ray1EngineVersion EngineVersion { get; }
    public ConfigFile? Config { get; set; }
    public string? ConfigFileName { get; set; }
    public Context? Context { get; set; }

    public bool IsGameLanguageAvailable { get; }
    public Language GameLanguage { get; set; }

    public bool IsMusicEnabled { get; set; }
    public bool IsStero { get; set; }
    public int SoundVolume { get; set; }

    public bool IsVoicesVolumeAvailable { get; }
    public int VoicesVolume { get; set; }

    public bool ShowBackground { get; set; }
    public bool ShowParallaxBackground { get; set; }
    public bool ShowHUD { get; set; }

    public Freq[] FrameRateOptions_Values { get; }
    public string[] FrameRateOptions_Names { get; }
    public int SelectedFrameRateOption { get; set; }
    public Freq FrameRate
    {
        get => FrameRateOptions_Values[SelectedFrameRateOption];
        set => SelectedFrameRateOption = FrameRateOptions_Values.FindItemIndex(x => x == value);
    }

    public int ZoneOfPlay { get; set; }

    public ObservableCollection<ButtonMapperKeyItemViewModel> Keys { get; }

    public int GamePad_Jump { get; set; }
    public int GamePad_Fist { get; set; }
    public int GamePad_Action { get; set; }

    public int XPadMax { get; set; }
    public int XPadMin { get; set; }
    public int YPadMax { get; set; }
    public int YPadMin { get; set; }
    public int XPadCenter { get; set; }
    public int YPadCenter { get; set; }

    public int Port { get; set; }
    public int IRQ { get; set; }
    public int DMA { get; set; }
    public int Param { get; set; }
    public int DeviceID { get; set; }
    public int NumCard { get; set; }

    #endregion

    #region Private Methods

    private static ConfigFile CreateDefaultConfig()
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

    #region Protected Methods

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
        for (int i = 0; i < Keys.Count; i++)
        {
            var item = Keys[i];
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

        Logger.Info("All config properties have been loaded");

        return Task.CompletedTask;
    }

    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("{0} config is saving...", GameInstallation.FullId);

        try
        {
            if (Config == null || Context == null || ConfigFileName == null)
                throw new Exception("Saving can not be done before the config has been loaded");

            // Set button mapping
            for (int i = 0; i < Keys.Count; i++)
                Config.Tab_Key[i] = (byte)DirectXKeyHelpers.GetKeyCode(Keys[i].NewKey);

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

    protected override void ConfigPropertyChanged(string propertyName)
    {
        if (propertyName is
            nameof(GameLanguage) or
            nameof(IsMusicEnabled) or 
            nameof(IsStero) or 
            nameof(SoundVolume) or 
            nameof(VoicesVolume) or 
            nameof(ShowBackground) or 
            nameof(ShowParallaxBackground) or 
            nameof(ShowHUD) or 
            nameof(SelectedFrameRateOption) or 
            nameof(ZoneOfPlay) or 
            nameof(GamePad_Jump) or 
            nameof(GamePad_Fist) or 
            nameof(GamePad_Action) or 
            nameof(XPadMax) or 
            nameof(XPadMin) or 
            nameof(YPadMax) or 
            nameof(YPadMin) or 
            nameof(XPadCenter) or 
            nameof(YPadCenter) or 
            nameof(Port) or 
            nameof(IRQ) or 
            nameof(DMA) or 
            nameof(Param) or 
            nameof(DeviceID) or 
            nameof(NumCard))
        {
            UnsavedChanges = true;
        }
    }

    #endregion

    #region Public Methods

    public abstract string GetConfigFileName();

    public override void Dispose()
    {
        // Dispose base
        base.Dispose();

        Context?.Dispose();
        Keys.DisposeAll();
    }

    #endregion
}