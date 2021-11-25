#nullable disable
using ByteSizeLib;
using RayCarrot.IO;
using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Base config view model for Rayman M, Rayman Arena and Rayman 3
/// </summary>
public abstract class Config_UbiIni3_BaseViewModel<Handler, Language> : Config_UbiIni_BaseViewModel<Handler>
    where Handler : UbiIniHandler
    where Language : Enum
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="game">The game</param>
    protected Config_UbiIni3_BaseViewModel(Games game) : base(game)
    {
        // Set the available modem quality options
        ModemQualityOptions = new string[]
        {
            "Unknown",
            "Modem 56k",
            "RNIS",
            "xDSL or cable",
            "Local Area Network"
        };
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _fullscreenMode;

    private bool _triLinear;

    private bool _tnL;

    private bool _isTextures32Bit;

    private bool _compressedTextures;

    private int _videoQuality;

    private bool _autoVideoQuality;

    private bool _isVideo32Bpp;

    private Language _currentLanguage;

    private bool _controllerSupport;

    private bool _isDiscCheckRemoved;

    private bool _dynamicShadows;

    private bool _staticShadows;

    private int _verticalAxis;

    private int _horizontalAxis;

    private int _modemQualityIndex;

    #endregion

    #region Protected Abstract Properties

    /// <summary>
    /// The available game patches
    /// </summary>
    protected abstract FilePatcher_Patch[] Patches { get; }

    #endregion

    #region Protected Properties

    /// <summary>
    /// The game patcher to use for patching the disc check
    /// </summary>
    protected FilePatcher Patcher { get; set; }

    #endregion

    #region Public Abstract Properties

    /// <summary>
    /// Indicates if <see cref="DynamicShadows"/> and <see cref="StaticShadows"/> are available
    /// </summary>
    public abstract bool HasShadowConfig { get; }

    /// <summary>
    /// Indicates if <see cref="HorizontalAxis"/> and <see cref="VerticalAxis"/> are available
    /// </summary>
    public abstract bool HasControllerConfig { get; }

    /// <summary>
    /// Indicates if <see cref="ModemQualityIndex"/> is available
    /// </summary>
    public abstract bool HasNetworkConfig { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if fullscreen mode is enabled or if the game should run in windowed mode
    /// </summary>
    public bool FullscreenMode
    {
        get => _fullscreenMode;
        set
        {
            _fullscreenMode = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicated if TriLinear is enabled
    /// </summary>
    public bool TriLinear
    {
        get => _triLinear;
        set
        {
            _triLinear = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicated if Transform and Lightning is enabled
    /// </summary>
    public bool TnL
    {
        get => _tnL;
        set
        {
            _tnL = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// True if textures are 32-bit, false if they are 16-bit
    /// </summary>
    public bool IsTextures32Bit
    {
        get => _isTextures32Bit;
        set
        {
            _isTextures32Bit = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicates if the compressed textures should be used
    /// </summary>
    public bool CompressedTextures
    {
        get => _compressedTextures;
        set
        {
            _compressedTextures = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The video quality, between 0 and 4
    /// </summary>
    public int VideoQuality
    {
        get => _videoQuality;
        set
        {
            _videoQuality = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicates if the video quality should be auto adjusted
    /// </summary>
    public bool AutoVideoQuality
    {
        get => _autoVideoQuality;
        set
        {
            _autoVideoQuality = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// True if videos are 32 bits per pixel, false if they are 16 bits per pixel
    /// </summary>
    public bool IsVideo32Bpp
    {
        get => _isVideo32Bpp;
        set
        {
            _isVideo32Bpp = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The currently selected language
    /// </summary>
    public Language CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicates if controller support is enabled
    /// </summary>
    public bool ControllerSupport
    {
        get => _controllerSupport;
        set
        {
            _controllerSupport = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicates if the option to remove the disc check from the game is available
    /// </summary>
    public bool CanRemoveDiscCheck { get; set; }

    /// <summary>
    /// Indicates if the disc check is set to be removed
    /// </summary>
    public bool IsDiscCheckRemoved
    {
        get => _isDiscCheckRemoved;
        set
        {
            _isDiscCheckRemoved = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicates if dynamic shadows are enabled
    /// </summary>
    public bool DynamicShadows
    {
        get => _dynamicShadows;
        set
        {
            _dynamicShadows = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicates if static shadows are enabled
    /// </summary>
    public bool StaticShadows
    {
        get => _staticShadows;
        set
        {
            _staticShadows = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The vertical controller axis value
    /// </summary>
    public int VerticalAxis
    {
        get => _verticalAxis;
        set
        {
            _verticalAxis = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The horizontal controller axis value
    /// </summary>
    public int HorizontalAxis
    {
        get => _horizontalAxis;
        set
        {
            _horizontalAxis = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The available modem quality options
    /// </summary>
    public string[] ModemQualityOptions { get; }

    /// <summary>
    /// The current modem quality index
    /// </summary>
    public int ModemQualityIndex
    {
        get => _modemQualityIndex;
        set
        {
            _modemQualityIndex = value;
            UnsavedChanges = true;
        }
    }

    #endregion

    #region Protected Override Methods

    protected override object GetPageUI() => new Config_UbiIni3_UI()
    {
        DataContext = this
    };

    /// <summary>
    /// Setup
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task OnSetupAsync()
    {
        // Get the current dinput type
        var dinputType = CanModifyGame ? GetCurrentDinput() : DinputType.Unknown;

        Logger.Info("The dinput type has been retrieved as {0}", dinputType);

        ControllerSupport = dinputType == DinputType.Controller;

        // Check if the disc check has been removed
        CanRemoveDiscCheck = Patches != null;

        if (CanRemoveDiscCheck)
        {
            // Get the game file path
            var gameFile = Game.GetInstallDir(false) + Game.GetGameInfo().DefaultFileName;

            // Check if it exists
            if (gameFile.FileExists)
            {
                Patcher = new FilePatcher(gameFile, Patches);

                var result = Patcher.GetIsOriginal();

                if (result == true)
                {
                    IsDiscCheckRemoved = false;
                    CanRemoveDiscCheck = true;

                    Logger.Info("The game has not been modified to remove the disc checker");
                }
                else if (result == false)
                {
                    IsDiscCheckRemoved = true;
                    CanRemoveDiscCheck = true;

                    Logger.Info("The game has been modified to remove the disc checker");
                }
                else if (result == null)
                {
                    CanRemoveDiscCheck = false;

                    Logger.Info("The game disc checker status could not be read");
                }
            }
            else
            {
                CanRemoveDiscCheck = false;

                Logger.Info("The game file was not found");
            }
        }
        else
        {
            Logger.Trace("The disc checker can not be removed for this game");
        }

        // If the primary config file does not exist, create a new one
        if (!AppFilePaths.UbiIniPath1.FileExists)
        {
            try
            {
                // Create the file
                Services.File.CreateFile(AppFilePaths.UbiIniPath1);

                Logger.Info("A new ubi.ini file has been created under {0}", AppFilePaths.UbiIniPath1);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Creating ubi.ini file");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_InvalidUbiIni, AppFilePaths.UbiIniPath1.Parent));

                throw;
            }
        }

        // If the secondary config file does not exist, attempt to create a new one
        if (!AppFilePaths.UbiIniPath2.FileExists)
        {
            try
            {
                // Create the file
                Services.File.CreateFile(AppFilePaths.UbiIniPath2);

                Logger.Info("A new ubi.ini file has been created under {0}", AppFilePaths.UbiIniPath2);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Creating ubi.ini file");
            }
        }
    }

    /// <summary>
    /// Saving
    /// </summary>
    /// <returns>The task</returns>
    protected override Task OnSaveAsync()
    {
        // Attempt to copy data to secondary file
        if (AppFilePaths.UbiIniPath2.FileExists)
        {
            try
            {
                // Get the current data
                var sectionData = ConfigData.GetSectionData();

                // Load the file data
                var secondaryDataHandler = new DuplicateSectionUbiIniHandler(AppFilePaths.UbiIniPath2, ConfigData.SectionKey);

                // Duplicate the data
                secondaryDataHandler.Duplicate(sectionData);

                // Save the file
                secondaryDataHandler.Save();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Saving {0} ubi.ini secondary data", Game);
            }
        }

        if (CanModifyGame)
        {
            try
            {
                // Get the current dinput type
                var dt = GetCurrentDinput();
                var path = GetDinputPath();

                Logger.Info("The dinput type has been retrieved as {0}", dt);

                if (ControllerSupport)
                {
                    if (dt != DinputType.Controller)
                    {
                        if (dt != DinputType.None)
                            // Attempt to delete existing dinput file
                            Services.File.DeleteFile(path);

                        // Write controller patch
                        File.WriteAllBytes(path, Files.dinput8_controller);
                    }
                }
                else if (dt == DinputType.Controller)
                {
                    // Attempt to delete existing dinput file
                    Services.File.DeleteFile(path);
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Saving {0} dinput hack data", Game);
                throw;
            }

            if (CanRemoveDiscCheck)
            {
                try
                {
                    Patcher = new FilePatcher(Game.GetInstallDir() + Game.GetGameInfo().DefaultFileName, Patches);

                    var result = Patcher.GetIsOriginal();

                    if (result == true && IsDiscCheckRemoved)
                        Patcher.PatchFile(false);

                    else if (result == false && !IsDiscCheckRemoved)
                        Patcher.PatchFile(true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Saving {0} disc check modification", Game);
                    throw;
                }
            }
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the current dinput file used
    /// </summary>
    /// <returns>The current dinput file used</returns>
    private DinputType GetCurrentDinput()
    {
        var path = GetDinputPath();

        if (!path.FileExists)
            return DinputType.None;

        try
        {
            var size = path.GetSize();

            if (size == ByteSize.FromBytes(156160))
                return DinputType.Controller;

            // If the size equals that of the Rayman 2 dinput file, delete it
            // as the Rayman 2 dinput file was accidentally used prior to version 4.1.2
            if (size == ByteSize.FromBytes(66560))
            {
                Services.File.DeleteFile(path);
                return DinputType.None;
            }

            return DinputType.Unknown;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting {0} dinput file size", Game);
            return DinputType.Unknown;
        }
    }

    /// <summary>
    /// Gets the current dinput.dll path
    /// </summary>
    /// <returns>The path</returns>
    private FileSystemPath GetDinputPath()
    {
        return Game.GetInstallDir(false) + "dinput8.dll";
    }

    #endregion

    #region Private Enum

    /// <summary>
    /// The available types of the dinput8.dll file
    /// </summary>
    private enum DinputType
    {
        /// <summary>
        /// No file found
        /// </summary>
        None,

        /// <summary>
        /// Controller fix
        /// </summary>
        Controller,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    #endregion
}