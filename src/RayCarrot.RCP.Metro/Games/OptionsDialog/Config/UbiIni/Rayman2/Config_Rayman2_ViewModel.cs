#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ByteSizeLib;
using RayCarrot.IO;
using NLog;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman 2 configuration
/// </summary>
public class Config_Rayman2_ViewModel : Config_UbiIni_BaseViewModel<R2UbiIniHandler>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Config_Rayman2_ViewModel() : base(Games.Rayman2)
    {
        // Set default properties
        IsHorizontalWidescreen = true;

        void keyChanged() => UnsavedChanges = true;

        KeyItems = new ObservableCollection<ButtonMapperKeyItemViewModel>()
        {
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Up)), Key.Up, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Down)), Key.Down, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Left)), Key.Left, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Right)), Key.Right, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_JumpSwimUp)), Key.A, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Config_Action_SwimDown)), Key.Z, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Shoot)), Key.Space, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_WalkSlow)), Key.LeftShift, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Strafe)), Key.LeftCtrl, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_CamRight)), Key.Q, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_CamLeft)), Key.W, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_CamCenter)), Key.End, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Look)), Key.NumPad0, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Screenshot)), Key.F8, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_HUD)), Key.J, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_R2Guide)), Key.F1, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Confirm)), Key.Enter, keyChanged),
            new ButtonMapperKeyItemViewModel(new ResourceLocString(nameof(Resources.Config_Action_Cancel)), Key.Escape, keyChanged)
        };
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Constants

    private const string GLI_DllFile_DirectX = "GliDX6";

    private const string GLI_Dll_DirectX = "DirectX6";

    private const string GLI_Driver_DirectX = "display";

    private const string GLI_Device_DirectX = "Direct3D HAL";

    private const string GLI_DllFile_Glide = "GliVd1";

    private const string GLI_Dll_Glide = "Glide2";

    #endregion

    #region Private Fields

    private bool _widescreenSupport;

    private bool _controllerSupport;

    private R2Languages _currentLanguage;

    private R2GraphicsMode _selectedGraphicsMode;

    private string _gliDllFile;

    private string _gliDll;

    private string _gliDriver;

    private string _gliDevice;

    #endregion

    #region Public Properties

    /// <summary>
    /// The ubi.ini config file path
    /// </summary>
    public FileSystemPath ConfigPath { get; set; }

    /// <summary>
    /// Indicates if widescreen support is enabled
    /// </summary>
    public bool WidescreenSupport
    {
        get => _widescreenSupport;
        set
        {
            _widescreenSupport = value;
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
    /// Indicates if the widescreen support is horizontal, otherwise it is vertical
    /// </summary>
    public bool IsHorizontalWidescreen { get; set; }

    /// <summary>
    /// The currently selected language
    /// </summary>
    public R2Languages CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The key items
    /// </summary>
    public ObservableCollection<ButtonMapperKeyItemViewModel> KeyItems { get; }

    /// <summary>
    /// The currently selected graphics mode
    /// </summary>
    public R2GraphicsMode SelectedGraphicsMode
    {
        get => _selectedGraphicsMode;
        set
        {
            _selectedGraphicsMode = value;

            if (SelectedGraphicsMode == R2GraphicsMode.DirectX)
            {
                GLI_DllFile = GLI_DllFile_DirectX;
                GLI_Dll = GLI_Dll_DirectX;
                GLI_Driver = GLI_Driver_DirectX;
                GLI_Device = GLI_Device_DirectX;
            }
            else if (SelectedGraphicsMode == R2GraphicsMode.Glide)
            {
                GLI_DllFile = GLI_DllFile_Glide;
                GLI_Dll = GLI_Dll_Glide;
                GLI_Driver = String.Empty;
                GLI_Device = String.Empty;
            }
        }
    }

    /// <summary>
    /// The GLI_DllFile field
    /// </summary>
    public string GLI_DllFile
    {
        get => _gliDllFile;
        set
        {
            _gliDllFile = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The GLI_Dll field
    /// </summary>
    public string GLI_Dll
    {
        get => _gliDll;
        set
        {
            _gliDll = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The GLI_Driver field
    /// </summary>
    public string GLI_Driver
    {
        get => _gliDriver;
        set
        {
            _gliDriver = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The GLI_Device field
    /// </summary>
    public string GLI_Device
    {
        get => _gliDevice;
        set
        {
            _gliDevice = value;
            UnsavedChanges = true;
        }
    }

    #endregion

    #region Protected Override Methods

    protected override object GetPageUI() => new Config_Rayman2_UI()
    {
        DataContext = this
    };

    /// <summary>
    /// Loads the <see cref="Config_UbiIni_BaseViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The config data</returns>
    protected override Task<R2UbiIniHandler> LoadConfigAsync()
    {
        // Load the configuration data
        return Task.FromResult(new R2UbiIniHandler(ConfigPath));
    }

    /// <summary>
    /// Imports the <see cref="Config_UbiIni_BaseViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task ImportConfigAsync()
    {
        var gliMode = ConfigData.FormattedGLI_Mode;

        GraphicsMode.GetAvailableResolutions();

        if (gliMode != null)
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(gliMode.ResX, gliMode.ResY);
        else
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(800, 600);

        CurrentLanguage = ConfigData.FormattedLanguage ?? R2Languages.English;

        GLI_DllFile = ConfigData.GLI_DllFile;
        GLI_Dll = ConfigData.GLI_Dll;
        GLI_Driver = ConfigData.GLI_Driver;
        GLI_Device = ConfigData.GLI_Device;

        SetGraphicsMode();

        if (!CanModifyGame)
            return;

        // Check if the aspect ratio has been modified
        if (await GetIsWidescreenHackAppliedAsync() == true)
            WidescreenSupport = true;

        if (GetCurrentDinput() == R2Dinput.Mapping)
        {
            HashSet<Config_Rayman2_ButtonMappingItem> result;

            Logger.Info("Loading current button mapping...");

            try
            {
                // Load current mapping
                result = await Config_Rayman2_ButtonMappingManager.LoadCurrentMappingAsync(GetDinputPath());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Loading current R2 button mapping");
                return;
            }

            if (result == null)
                return;

            Logger.Debug("{0} key items retrieved", result.Count);

            // Process each item
            foreach (var item in result)
            {
                // Get the original key
                var originalKey = DirectXKeyHelpers.GetKey(item.OriginalKey);

                // Attempt to get corresponding Rayman 2 key
                var r2Item = KeyItems.FirstOrDefault(x => x.OriginalKey == originalKey);

                // If one was found, set the new key
                r2Item?.SetInitialNewKey(DirectXKeyHelpers.GetKey(item.NewKey));
            }
        }
    }

    /// <summary>
    /// Updates the <see cref="Config_UbiIni_BaseViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected override Task UpdateConfigAsync()
    {
        ConfigData.GLI_Mode = new RayGLI_Mode()
        {
            ColorMode = ConfigData.FormattedGLI_Mode?.ColorMode ?? 16,
            IsWindowed = ConfigData.FormattedGLI_Mode?.IsWindowed ?? false,
            ResX = GraphicsMode.Width,
            ResY = GraphicsMode.Height,
        }.ToString();

        ConfigData.Language = CurrentLanguage.ToString();
        ConfigData.GLI_DllFile = GLI_DllFile;
        ConfigData.GLI_Dll = GLI_Dll;
        ConfigData.GLI_Driver = GLI_Driver;
        ConfigData.GLI_Device = GLI_Device;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Setup
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task OnSetupAsync()
    {
        // Get the config path
        ConfigPath = GetUbiIniPath();

        Logger.Info("The ubi.ini path has been retrieved as {0}", ConfigPath);

        // Get the dinput type
        var dt = CanModifyGame ? GetCurrentDinput() : R2Dinput.Unknown;

        Logger.Info("The dinput type has been retrieved as {0}", dt);

        ControllerSupport = dt == R2Dinput.Controller;

        // If the file does not exist, create a new one
        if (!ConfigPath.FileExists)
        {
            // Check if the game is the GOG version, in which case the file is located in the install directory
            bool isGOG = (Games.Rayman2.GetInstallDir(false) + "goggame.sdb").FileExists;

            // Get the new file path
            var newFile = isGOG ? Games.Rayman2.GetInstallDir(false) + "ubi.ini" : AppFilePaths.UbiIniPath1;

            try
            {
                // Create the file
                Services.File.CreateFile(newFile);

                Logger.Info("A new ubi.ini file has been created under {0}", newFile.FullPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Creating ubi.ini file");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_InvalidUbiIni, newFile.Parent.FullPath));

                throw;
            }
        }
    }

    /// <summary>
    /// Saving
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task OnSaveAsync()
    {
        if (!CanModifyGame)
            return;

        // Set the aspect ratio
        await SetAspectRatioAsync();

        try
        {
            // Get the current dinput type
            var dt = GetCurrentDinput();
            var path = GetDinputPath();

            Logger.Info("The dinput type has been retrieved as {0}", dt);

            if (ControllerSupport)
            {
                if (dt != R2Dinput.Controller)
                {
                    if (dt != R2Dinput.None)
                        // Attempt to delete existing dinput file
                        File.Delete(path);

                    // Write controller patch
                    File.WriteAllBytes(path, Files.dinput_controller);
                }
            }
            else
            {
                var items = KeyItems.
                    // Get only the ones that have changed
                    Where(x => x.NewKey != x.OriginalKey).
                    // Convert to a key mapping item
                    Select(x => new Config_Rayman2_ButtonMappingItem(DirectXKeyHelpers.GetKeyCode(x.OriginalKey), DirectXKeyHelpers.GetKeyCode(x.NewKey))).
                    // Convert to a list
                    ToList();

                if (items.Any())
                {
                    if (dt != R2Dinput.Mapping)
                    {
                        if (dt != R2Dinput.None)
                            // Attempt to delete existing dinput file
                            File.Delete(path);

                        // Write template file
                        File.WriteAllBytes(path, Files.dinput_mapping);
                    }

                    // Apply the patch
                    await Config_Rayman2_ButtonMappingManager.ApplyMappingAsync(path, items);
                }
                else
                {
                    if (dt != R2Dinput.Unknown && dt != R2Dinput.None)
                    {
                        // Attempt to delete existing dinput file
                        File.Delete(path);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving R2 dinput hack data");
            throw;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Sets the aspect ratio for the Rayman 2 executable file
    /// </summary>
    /// <returns>The task</returns>
    private async Task SetAspectRatioAsync()
    {
        try
        {
            Logger.Info("The Rayman 2 aspect ratio is being set...");

            // Get the file path
            FileSystemPath path = Games.Rayman2.GetInstallDir(false) + Games.Rayman2.GetGameInfo().DefaultFileName;

            // Make sure the file exists
            if (!path.FileExists)
            {
                Logger.Warn("The Rayman 2 executable could not be found");

                if (WidescreenSupport)
                    await Services.MessageUI.DisplayMessageAsync(Resources.R2Widescreen_ExeNotFound, MessageType.Error);

                return;
            }

            // Get the location
            var location = GetAspectRatioLocation(path);

            Logger.Debug("The aspect ratio byte location has been detected as {0}", location);

            // Cancel if unknown version
            if (location == -1)
            {
                Logger.Info("The Rayman 2 executable file size does not match any supported version");

                if (WidescreenSupport)
                    await Services.MessageUI.DisplayMessageAsync(Resources.R2Widescreen_ExeNotValid, MessageType.Error);

                return;
            }

            // Apply widescreen patch
            if (WidescreenSupport)
            {
                // Get the aspect ratio
                float ratio = IsHorizontalWidescreen ? (float)GraphicsMode.Height / GraphicsMode.Width : (float)GraphicsMode.Width / GraphicsMode.Height;

                // Multiply by 4/3
                ratio *= (4.0F / 3.0F);

                // Get the hex representation of the value
                string value = BitConverter.GetBytes(ratio).Aggregate(String.Empty, (current, item) => current + item.ToString("X2"));

                // Get the byte array to write
                byte[] input = Enumerable.Range(0, value.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(value.Substring(x, 2), 16))
                    .ToArray();

                Logger.Debug("The Rayman 2 aspect ratio bytes detected as {0}", input.JoinItems(", "));

                // Open the file
                using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.Read))
                {
                    // Set the position
                    stream.Position = location;

                    // Write the bytes
                    await stream.WriteAsync(input, 0, input.Length);
                }

                Logger.Info("The Rayman 2 aspect ratio has been set");
            }
            // Restore to 4/3 if modified previously
            else
            {
                // Open the file
                using Stream readStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Set the position
                readStream.Position = location;

                // Create the buffer
                var buffer = new byte[4];

                // Read the bytes
                await readStream.ReadAsync(buffer, 0, 4);

                // Check if the data has been modified
                if (CheckAspectRatio(buffer))
                {
                    Logger.Info("The Rayman 2 aspect ratio has been detected as modified");

                    // Close previous stream and open a new stream
                    readStream.Dispose();

                    using Stream writeStream = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.Read);

                    // Set the position
                    writeStream.Position = location;

                    // Write the bytes
                    await writeStream.WriteAsync(new byte[]
                    {
                        0,
                        0,
                        128,
                        63
                    }, 0, 4);

                    Logger.Info("The Rayman 2 aspect ratio has been restored");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting R2 aspect ratio");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R2Widescreen_Error);

            if (WidescreenSupport)
                throw;
        }
    }

    /// <summary>
    /// Sets the graphics mode based on the graphics settings
    /// </summary>
    private void SetGraphicsMode()
    {
        if (GLI_DllFile?.Equals(GLI_DllFile_DirectX, StringComparison.InvariantCultureIgnoreCase) == true &&
            GLI_Dll?.Equals(GLI_Dll_DirectX, StringComparison.InvariantCultureIgnoreCase) == true &&
            GLI_Driver?.Equals(GLI_Driver_DirectX, StringComparison.InvariantCultureIgnoreCase) == true &&
            GLI_Device?.Equals(GLI_Device_DirectX, StringComparison.InvariantCultureIgnoreCase) == true)
        {
            _selectedGraphicsMode = R2GraphicsMode.DirectX;
        }
        else if (GLI_DllFile?.Equals(GLI_DllFile_Glide, StringComparison.InvariantCultureIgnoreCase) == true &&
                 GLI_Dll?.Equals(GLI_Dll_Glide, StringComparison.InvariantCultureIgnoreCase) == true &&
                 GLI_Driver.IsNullOrEmpty() &&
                 GLI_Device.IsNullOrEmpty())
        {
            _selectedGraphicsMode = R2GraphicsMode.Glide;
        }
        else
        {
            _selectedGraphicsMode = R2GraphicsMode.Custom;
        }

        OnPropertyChanged(nameof(SelectedGraphicsMode));
    }

    #endregion

    #region Public Methods

    public override void Dispose()
    {
        // Dispose base
        base.Dispose();

        KeyItems?.DisposeAll();
    }

    #endregion

    #region Private Static Methods

    /// <summary>
    /// Gets a value indicating if the aspect ratio for the Rayman 2 executable file
    /// has been modified
    /// </summary>
    /// <param name="ratio">The aspect ratio bytes</param>
    /// <returns>True if the aspect ratio is modified, false if it's the original value</returns>
    private static bool CheckAspectRatio(IReadOnlyList<byte> ratio)
    {
        // Check if the data has been modified
        if (ratio[0] != 0 || ratio[1] != 0 || ratio[2] != 128 || ratio[3] != 63)
            return true;

        return false;
    }

    /// <summary>
    /// Gets the aspect ratio location for a Rayman 2 executable file
    /// </summary>
    /// <param name="path">The file path</param>
    /// <returns>The location or -1 if not found</returns>
    private static int GetAspectRatioLocation(FileSystemPath path)
    {
        // Get the size to determine the version
        var length = path.GetSize();

        // Get the byte location
        int location = -1;

        // Check if it's the disc version
        if ((int)length.Bytes == 676352)
            location = 633496;

        // Check if it's the GOG version
        else if ((int)length.Bytes == 1468928)
            location = 640152;

        return location;
    }

    /// <summary>
    /// Gets the current dinput.dll path for Rayman 2
    /// </summary>
    /// <returns>The path</returns>
    private static FileSystemPath GetDinputPath()
    {
        return Games.Rayman2.GetInstallDir(false) + "dinput.dll";
    }

    /// <summary>
    /// Gets the current config path for the ubi.ini file
    /// </summary>
    /// <returns>The path</returns>
    private static FileSystemPath GetUbiIniPath()
    {
        var path1 = Games.Rayman2.GetInstallDir(false) + "ubi.ini";

        if (path1.FileExists)
            return path1;

        var path2 = AppFilePaths.UbiIniPath1;

        if (path2.FileExists)
            return path2;

        return FileSystemPath.EmptyPath;
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Gets the current dinput file used for Rayman 2
    /// </summary>
    /// <returns>The current dinput file used</returns>
    public static R2Dinput GetCurrentDinput()
    {
        var path = GetDinputPath();

        if (!path.FileExists)
            return R2Dinput.None;

        try
        {
            var size = path.GetSize();

            if (size == ByteSize.FromBytes(136704))
                return R2Dinput.Mapping;

            if (size == ByteSize.FromBytes(66560))
                return R2Dinput.Controller;

            return R2Dinput.Unknown;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting R2 dinput file size");
            return R2Dinput.Unknown;
        }
    }

    /// <summary>
    /// Gets a value indicating if a widescreen patch has been applied
    /// </summary>
    /// <returns>True if a widescreen patch has been applied, false if it has not. Null if an error occurred while checking.</returns>
    public static async Task<bool?> GetIsWidescreenHackAppliedAsync()
    {
        try
        {
            // Get the file path
            FileSystemPath path = Games.Rayman2.GetInstallDir() + Games.Rayman2.GetGameInfo().DefaultFileName;

            // Get the location
            var location = GetAspectRatioLocation(path);

            if (location == -1)
                return null;

            // Open the file
            using Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Set the position
            stream.Position = location;

            // Create the buffer
            var buffer = new byte[4];

            // Read the bytes
            await stream.ReadAsync(buffer, 0, 4);

            // Check if the data has been modified
            if (CheckAspectRatio(buffer))
            {
                Logger.Info("The Rayman 2 aspect ratio has been detected as modified");

                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if R2 aspect ratio has been modified");

            return null;
        }
    }

    #endregion

    #region Public Enums

    /// <summary>
    /// The available types of Rayman 2 dinput.dll file
    /// </summary>
    public enum R2Dinput
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
        /// Button mapping hack
        /// </summary>
        Mapping,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    /// <summary>
    /// The available pre-set Rayman 2 graphics modes
    /// </summary>
    public enum R2GraphicsMode
    {
        /// <summary>
        /// DirectX
        /// </summary>
        DirectX,

        /// <summary>
        /// Glide
        /// </summary>
        Glide,
            
        /// <summary>
        /// Custom
        /// </summary>
        Custom
    }

    #endregion
}