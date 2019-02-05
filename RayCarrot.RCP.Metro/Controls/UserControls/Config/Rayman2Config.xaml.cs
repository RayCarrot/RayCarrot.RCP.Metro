using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ByteSizeLib;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Rayman2Config.xaml
    /// </summary>
    public partial class Rayman2Config : BaseUserControl<Rayman2ConfigViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window">The parent window</param>
        public Rayman2Config(Window window)
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

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Temporary solution for scrolling over data grid
            scrollViewer.ScrollToVerticalOffset(scrollViewer.ContentVerticalOffset - (e.Delta / 2d));
        }

        #endregion
    }

    /// <summary>
    /// View model for the Rayman 2 configuration
    /// </summary>
    public class Rayman2ConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman2ConfigViewModel()
        {
            // Set default properties
            IsHorizontalWidescreen = true;
            KeyItems = new ObservableCollection<R2KeyItemViewModel>()
            {
                new R2KeyItemViewModel("Up", Key.Up, this),
                new R2KeyItemViewModel("Down", Key.Down, this),
                new R2KeyItemViewModel("Left", Key.Left, this),
                new R2KeyItemViewModel("Right", Key.Right, this),
                new R2KeyItemViewModel("Jump", Key.A, this),
                new R2KeyItemViewModel("Shoot", Key.Space, this),
                new R2KeyItemViewModel("Walk Slowly", Key.LeftShift, this),
                new R2KeyItemViewModel("Strafe", Key.LeftCtrl, this),
                new R2KeyItemViewModel("Camera Right", Key.Q, this),
                new R2KeyItemViewModel("Camera Left", Key.W, this),
                new R2KeyItemViewModel("Center camera", Key.End, this),
                new R2KeyItemViewModel("Look mode", Key.NumPad0, this),
                new R2KeyItemViewModel("Screenshot", Key.F8, this),
                new R2KeyItemViewModel("Show HUD", Key.J, this),
                new R2KeyItemViewModel("The Knowledge of the World", Key.F1, this),
                new R2KeyItemViewModel("Confirm", Key.Enter, this),
                new R2KeyItemViewModel("Cancel", Key.Escape, this)
            };

            // Create the async lock
            AsyncLock = new AsyncLock();

            // Create the commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        #endregion

        #region Private Fields

        private int _resX;

        private int _resY;

        private bool _lockToScreenRes;

        private bool _widescreenSupport;

        private bool _controllerSupport;

        private R2Languages _currentLanguage;

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for saving the configuration
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The configuration data
        /// </summary>
        public R2UbiIniHandler ConfigData { get; set; }

        /// <summary>
        /// The ubi.ini config file path
        /// </summary>
        public FileSystemPath ConfigPath { get; set; }

        /// <summary>
        /// The current horizontal resolution
        /// </summary>
        public int ResX
        {
            get => _resX;
            set
            {
                _resX = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The current vertical resolution
        /// </summary>
        public int ResY
        {
            get => _resY;
            set
            {
                _resY = value;
                UnsavedChanges = true;
            }
        }

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

                if (value && LockToScreenRes)
                    ResX = (int)SystemParameters.PrimaryScreenWidth;
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
        /// Indicates if the resolution is locked to the current screen resolution
        /// </summary>
        public bool LockToScreenRes
        {
            get => _lockToScreenRes;
            set
            {
                _lockToScreenRes = value;

                if (!value)
                    return;

                ResY = (int)SystemParameters.PrimaryScreenHeight;

                ResX = WidescreenSupport
                    ? (int) SystemParameters.PrimaryScreenWidth
                    : (int) Math.Round((double) ResY / 3 * 4);
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
        public ObservableCollection<R2KeyItemViewModel> KeyItems { get; }

        /// <summary>
        /// Indicates if dinput hacks are allowed
        /// </summary>
        public bool AllowDinputHack { get; set; }

        #endregion

        #region Commands

        public AsyncRelayCommand SaveCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override async Task SetupAsync()
        {
            RCF.Logger.LogInformationSource("Rayman 2 config is being set up");

            // Get the config path
            ConfigPath = GetUbiIniPath();

            RCF.Logger.LogInformationSource($"The ubi.ini path has been retrieved as {ConfigPath}");

            // Get the dinput type
            var dt = GetCurrentDinput();

            RCF.Logger.LogInformationSource($"The dinput type has been retrieved as {dt}");

            ControllerSupport = dt == R2Dinput.Controller;
            AllowDinputHack = dt != R2Dinput.Unknown;

            // If the file does not exist, create a new one
            if (!ConfigPath.FileExists)
            {
                // Check if the game is the GOG version,
                // in which case the file is located in the install directory
                bool isGOG = (Games.Rayman2.GetInfo().InstallDirectory + "goggame.sdb").FileExists;

                // Get the new file path
                var newFile = isGOG ? Games.Rayman2.GetInfo().InstallDirectory + "ubi.ini" : CommonPaths.UbiIniPath1;

                try
                {
                    // Create the file
                    File.Create(newFile);

                    RCF.Logger.LogInformationSource($"A new ubi.ini file has been created under {newFile}");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Creating ubi.ini file");

                    await RCF.MessageUI.DisplayMessageAsync($"No valid ubi.ini file was found and creating a new one failed. Try running the program as administrator " +
                                                            $"or changing the folder permissions for the following path: {newFile.Parent}", "Error", MessageType.Error);

                    throw;
                }
            }

            // Load the configuration data
            ConfigData = new R2UbiIniHandler(ConfigPath);

            RCF.Logger.LogInformationSource($"The ubi.ini file has been loaded");

            // Re-create the section if it doesn't exist
            if (!ConfigData.Exists)
            {
                ConfigData.ReCreate();
                RCF.Logger.LogInformationSource($"The ubi.ini section for Rayman 2 was recreated");
            }

            ResX = ConfigData.FormattedGLI_Mode.ResX;
            ResY = ConfigData.FormattedGLI_Mode.ResY;
            CurrentLanguage = ConfigData.FormattedLanguage ?? R2Languages.English;

            // Check if the aspect ratio has been modified
            try
            {
                // Get the file path
                FileSystemPath path = Games.Rayman2.GetLaunchInfo().Path;

                // Get the location
                var location = GetAspectRatioLocation(path);

                if (location != -1)
                {
                    // Open the file
                    using (Stream stream = File.Open(path, FileMode.Open))
                    {
                        // Set the position
                        stream.Position = location;

                        // Create the buffer
                        var buffer = new byte[4];

                        // Read the bytes
                        await stream.ReadAsync(buffer, 0, 4);

                        // Check if the data has been modified
                        if (CheckAspectRatio(buffer))
                        {
                            RCF.Logger.LogInformationSource($"The Rayman 2 aspect ratio has been detected as modified");

                            WidescreenSupport = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Checking if R2 aspect ratio has been modified");
            }

            UnsavedChanges = false;

            RCF.Logger.LogInformationSource($"All section properties have been loaded");

            if (dt == R2Dinput.Mapping)
            {
                HashSet<KeyMappingItem> result;

                RCF.Logger.LogInformationSource($"Loading current button mapping...");

                try
                {
                    // Load current mapping
                    result = await R2ButtonMappingManager.LoadCurrentMappingAsync(GetDinputPath());
                }
                catch (Exception ex)
                {
                    ex.HandleError("Loading current R2 button mapping");
                    return;
                }

                if (result == null)
                    return;

                RCF.Logger.LogDebugSource($"{result.Count} key items retrieved");

                // Process each item
                foreach (var item in result)
                {
                    // Get the original key
                    var originalKey = R2ButtonMappingManager.GetKey(item.OriginalKey);

                    // Attempt to get corresponding Rayman 2 key
                    var r2Item = KeyItems.FindItem(x => x.OriginalKey == originalKey);

                    // If one was found, set the new key
                    if (r2Item != null)
                        r2Item.SetInitialNewKey(R2ButtonMappingManager.GetKey(item.NewKey));
                }
            }
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"Rayman 2 configuration is saving...");

                try
                {
                    // Update the config data
                    UpdateConfig();

                    // Set the aspect ratio
                    await SetAspectRatioAsync();

                    // Save the config data
                    ConfigData.Save();

                    RCF.Logger.LogInformationSource($"Rayman 2 configuration has been saved");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving R2 ubi.ini data");
                    await RCF.MessageUI.DisplayMessageAsync("An error occurred when saving your Rayman 2 configuration", "Error saving", MessageType.Error);
                    return;
                }

                try
                {
                    // Get the current dinput type
                    var dt = GetCurrentDinput();
                    var path = GetDinputPath();

                    RCF.Logger.LogInformationSource($"The dinput type has been retrieved as {dt}");

                    if (WidescreenSupport)
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
                            // Convert to a ket mapping item
                            Select(x => new KeyMappingItem(R2ButtonMappingManager.GetKeyCode(x.OriginalKey), R2ButtonMappingManager.GetKeyCode(x.NewKey))).
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
                            await R2ButtonMappingManager.ApplyMappingAsync(path, items);
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
                    ex.HandleError("Saving R2 dinput hack data");
                    await RCF.MessageUI.DisplayMessageAsync("An error occurred when saving your Rayman 2 configuration. Some data may have been saved.", "Error saving", MessageType.Error);
                    return;
                }

                UnsavedChanges = false;

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("Your changes have been saved");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the <see cref="ConfigData"/>
        /// </summary>
        private void UpdateConfig()
        {
            ConfigData.GLI_Mode = new RayGLI_Mode()
            {
                ColorMode = ConfigData.FormattedGLI_Mode?.ColorMode ?? 16,
                IsWindowed = ConfigData.FormattedGLI_Mode?.IsWindowed ?? false,
                ResX = ResX,
                ResY = ResY
            }.ToString();

            ConfigData.Language = CurrentLanguage.ToString();
        }

        /// <summary>
        /// Gets a value indicating if the aspect ratio for the Rayman 2 executable file
        /// has been modified
        /// </summary>
        /// <param name="ratio">The aspect ratio bytes</param>
        /// <returns>True if the aspect ratio is modified, false if it's the original value</returns>
        private bool CheckAspectRatio(IReadOnlyList<byte> ratio)
        {
            // Check if the data has been modified
            if (ratio[0] != 0 || ratio[1] != 0 || ratio[2] != 128 || ratio[3] != 63)
                return true;

            return false;
        }

        /// <summary>
        /// Sets the aspect ratio for the Rayman 2 executable file
        /// </summary>
        /// <returns>The task</returns>
        private async Task SetAspectRatioAsync()
        {
            try
            {
                RCF.Logger.LogInformationSource($"The Rayman 2 aspect ratio is being set...");

                // Get the file path
                FileSystemPath path = Games.Rayman2.GetLaunchInfo().Path;

                // Make sure the file exists
                if (!path.FileExists)
                {
                    RCF.Logger.LogWarningSource("The Rayman 2 executable could not be found");

                    if (WidescreenSupport)
                        await RCF.MessageUI.DisplayMessageAsync("The aspect ratio could not be set due to the game executable not being found.", "Error", MessageType.Error);

                    return;
                }

                // Get the location
                var location = GetAspectRatioLocation(path);

                RCF.Logger.LogDebugSource($"The aspect ratio byte location has been detected as {location}");

                // Cancel if unknown version
                if (location == -1)
                {
                    RCF.Logger.LogInformationSource($"The Rayman 2 executable file size does not match any supported version");

                    if (WidescreenSupport)
                        await RCF.MessageUI.DisplayMessageAsync("The aspect ratio could not be set due to the game executable not being valid.", "Error", MessageType.Error);

                    return;
                }

                // Apply widescreen patch
                if (WidescreenSupport)
                {
                    // Get the aspect ratio
                    float ratio = IsHorizontalWidescreen ? (float)ResY / ResX : (float)ResX / ResY;

                    // Multiply by 4/3
                    ratio = ratio * (4.0F / 3.0F);

                    // Get the hex representation of the value
                    string value = Hex.GetHexRepresentation(ratio);

                    // Get the byte array to write
                    byte[] input = Enumerable.Range(0, value.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(value.Substring(x, 2), 16))
                        .ToArray();

                    RCF.Logger.LogDebugSource($"The Rayman 2 aspect ratio bytes detected as {input.JoinItems(", ")}");

                    // Open the file
                    using (Stream stream = File.Open(path, FileMode.Open))
                    {
                        // Set the position
                        stream.Position = location;

                        // Write the bytes
                        await stream.WriteAsync(input, 0, input.Length);
                    }

                    RCF.Logger.LogInformationSource($"The Rayman 2 aspect ratio has been set");
                }
                // Restore to 4/3 if modified previously
                else
                {
                    // Open the file
                    using (Stream stream = File.Open(path, FileMode.Open))
                    {
                        // Set the position
                        stream.Position = location;

                        // Create the buffer
                        var buffer = new byte[4];

                        // Read the bytes
                        await stream.ReadAsync(buffer, 0, 4);

                        // Check if the data has been modified
                        if (CheckAspectRatio(buffer))
                        {
                            RCF.Logger.LogInformationSource($"The Rayman 2 aspect ratio has been detected as modified");

                            // Set the position
                            stream.Position = location;

                            // Write the bytes
                            await stream.WriteAsync(new byte[]
                            {
                                0,
                                0,
                                128,
                                63
                            }, 0, 4);

                            RCF.Logger.LogInformationSource($"The Rayman 2 aspect ratio has been restored");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Fill out

                if (WidescreenSupport)
                    throw;
            }
        }

        #endregion

        #region Private Static Methods

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
        /// Gets the current dinput file used for Rayman 2
        /// </summary>
        /// <returns>The current dinput file used</returns>
        private static R2Dinput GetCurrentDinput()
        {
            var path = GetDinputPath();

            if (!path.FileExists)
                return R2Dinput.None;

            try
            {
                var size = path.GetSize();

                if (size == new ByteSize(136704))
                    return R2Dinput.Mapping;

                if (size == new ByteSize(66560))
                    return R2Dinput.Controller;

                return R2Dinput.Unknown;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting R2 dinput file size");
                return R2Dinput.Unknown;
            }
        }

        /// <summary>
        /// Gets the current dinput.dll path for Rayman 2
        /// </summary>
        /// <returns>The path</returns>
        private static FileSystemPath GetDinputPath()
        {
            return Games.Rayman2.GetInfo().InstallDirectory + "dinput.dll";
        }

        /// <summary>
        /// Gets the current config path for the ubi.ini file
        /// </summary>
        /// <returns>The path</returns>
        private static FileSystemPath GetUbiIniPath()
        {
            var path1 = Games.Rayman2.GetInfo().InstallDirectory + "ubi.ini";

            if (path1.FileExists)
                return path1;

            var path2 = CommonPaths.UbiIniPath1;

            if (path2.FileExists)
                return path1;

            return FileSystemPath.EmptyPath;
        }

        #endregion

        #region Private Enum

        /// <summary>
        /// The available types of Rayman 2 dinput.dll file
        /// </summary>
        private enum R2Dinput
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

        #endregion
    }

    /// <summary>
    /// Manages Rayman 2 button mapping through dinput.dll editing
    /// </summary>
    public static class R2ButtonMappingManager
    {
        #region Public Static Methods

        /// <summary>
        /// Loads the current button mapping
        /// </summary>
        /// <param name="dllFile">The dinput.dll path</param>
        /// <returns>The collection of the current mapping used</returns>
        public static async Task<HashSet<KeyMappingItem>> LoadCurrentMappingAsync(FileSystemPath dllFile)
        {
            var output = new HashSet<KeyMappingItem>();

            // Open the file in a new stream
            using (var stream = new FileStream(dllFile, FileMode.Open))
            {
                // Create the buffer to read into
                byte[] buffer = new byte[4];

                stream.Seek(CodeAdress + 14, SeekOrigin.Begin);
                await stream.ReadAsync(buffer, 0, 4);
                int originalKey = BitConverter.ToInt32(buffer, 0);

                // Get all key items
                while (originalKey > 0)
                {
                    stream.Seek(-14, SeekOrigin.Current);
                    await stream.ReadAsync(buffer, 0, 4);
                    var newKey = BitConverter.ToInt32(buffer, 0) & 0x000000FF;

                    output.Add(new KeyMappingItem(originalKey, newKey));

                    stream.Seek(29, SeekOrigin.Current);
                    await stream.ReadAsync(buffer, 0, 4);
                    originalKey = BitConverter.ToInt32(buffer, 0);
                }
            }

            return output;
        }

        /// <summary>
        /// Applies the specified button mapping
        /// </summary>
        /// <param name="dllFile">The dinput.dll path</param>
        /// <param name="items">The button mapping items to use</param>
        /// <returns>The task</returns>
        public static async Task ApplyMappingAsync(FileSystemPath dllFile, List<KeyMappingItem> items)
        {
            using (FileStream stream = new FileStream(dllFile, FileMode.Open, FileAccess.ReadWrite))
            {
                int i;

                stream.Seek(CodeAdress, SeekOrigin.Begin);

                // Write each key item
                for (i = 0; i < items.Count; i++)
                {
                    await stream.WriteAsync(_code1, 0, 4);

                    stream.WriteByte((byte)items[i].NewKey);

                    await stream.WriteAsync(_code2, 0, 9);

                    stream.WriteByte((byte)items[i].OriginalKey);

                    await stream.WriteAsync(_code3, 0, 4);

                    // Custom "jmp" instruction, you should always append to the final codef code
                    await stream.WriteAsync(BitConverter.GetBytes((items.Count - i - 1) * 23), 0, 4);
                }

                // End code
                await stream.WriteAsync(_codef, 0, 7);

                uint n = 0xFFFE4082;

                n -= (uint)(23 * i);

                // Custom "jmp" instruction, go back to where we were at the beginning
                await stream.WriteAsync(BitConverter.GetBytes(n), 0, 4);

                // Set the remaining bytes to 0 until the end of the file
                while (stream.Position < stream.Length)
                    stream.WriteByte(0);
            }
        }

        /// <summary>
        /// Converts a dinput key code to a <see cref="Key"/>
        /// </summary>
        /// <param name="key">The key code to convert</param>
        /// <returns>The <see cref="Key"/></returns>
        public static Key GetKey(int key)
        {
            switch (key)
            {
                case 0x01:
                    return Key.Escape;

                case 0x02:
                    return Key.D1;

                case 0x03:
                    return Key.D2;

                case 0x04:
                    return Key.D3;

                case 0x05:
                    return Key.D4;

                case 0x06:
                    return Key.D5;

                case 0x07:
                    return Key.D6;

                case 0x08:
                    return Key.D7;

                case 0x09:
                    return Key.D8;

                case 0x0A:
                    return Key.D9;

                case 0x0B:
                    return Key.D0;

                case 0x0C:
                    return Key.OemMinus;

                //case 0x0D:
                //    return "EQUALS";

                //case 0x0E:
                //    return "BACK";

                case 0x0F:
                    return Key.Tab;

                case 0x10:
                    return Key.Q;

                case 0x11:
                    return Key.W;

                case 0x12:
                    return Key.E;

                case 0x13:
                    return Key.R;

                case 0x14:
                    return Key.T;

                case 0x15:
                    return Key.Y;

                case 0x16:
                    return Key.U;

                case 0x17:
                    return Key.I;

                case 0x18:
                    return Key.O;

                case 0x19:
                    return Key.P;

                //case 0x1A:
                //    return "LBRACKET";

                //case 0x1B:
                //    return "RBRACKET";

                case 0x1C:
                    return Key.Return;

                //case 0x1D:
                //    return "LCONTROL";

                case 0x1E:
                    return Key.A;

                case 0x1F:
                    return Key.S;

                case 0x20:
                    return Key.D;

                case 0x21:
                    return Key.F;

                case 0x22:
                    return Key.G;

                case 0x23:
                    return Key.H;

                case 0x24:
                    return Key.J;

                case 0x25:
                    return Key.K;

                case 0x26:
                    return Key.L;

                case 0x27:
                    return Key.OemSemicolon;

                //case 0x28:
                //    return "APOSTROPHE";

                //case 0x29:
                //    return "GRAVE";

                case 0x2A:
                    return Key.LeftShift;

                //case 0x2B:
                //    return "BACKSLASH";

                case 0x2C:
                    return Key.Z;

                case 0x2D:
                    return Key.X;

                case 0x2E:
                    return Key.C;

                case 0x2F:
                    return Key.V;

                case 0x30:
                    return Key.B;

                case 0x31:
                    return Key.N;

                case 0x32:
                    return Key.M;

                case 0x33:
                    return Key.OemComma;

                case 0x34:
                    return Key.OemPeriod;

                //case 0x35:
                //    return "SLASH";

                case 0x36:
                    return Key.RightShift;

                case 0x37:
                    return Key.Multiply;

                case 0x38:
                    return Key.LeftAlt;

                case 0x39:
                    return Key.Space;

                case 0x3A:
                    return Key.Capital;

                case 0x3B:
                    return Key.F1;

                case 0x3C:
                    return Key.F2;

                case 0x3D:
                    return Key.F3;

                case 0x3E:
                    return Key.F4;

                case 0x3F:
                    return Key.F5;

                case 0x40:
                    return Key.F6;

                case 0x41:
                    return Key.F7;

                case 0x42:
                    return Key.F8;

                case 0x43:
                    return Key.F9;

                case 0x44:
                    return Key.F10;

                case 0x45:
                    return Key.NumLock;

                case 0x46:
                    return Key.Scroll;

                case 0x47:
                    return Key.NumPad7;

                case 0x48:
                    return Key.NumPad8;

                case 0x49:
                    return Key.NumPad9;

                case 0x4A:
                    return Key.Subtract;

                case 0x4B:
                    return Key.NumPad4;

                case 0x4C:
                    return Key.NumPad5;

                case 0x4D:
                    return Key.NumPad6;

                case 0x4E:
                    return Key.Add;

                case 0x4F:
                    return Key.NumPad1;

                case 0x50:
                    return Key.NumPad2;

                case 0x51:
                    return Key.NumPad3;

                case 0x52:
                    return Key.NumPad0;

                case 0x53:
                    return Key.Decimal;

                case 0x57:
                    return Key.F11;

                case 0x58:
                    return Key.F12;

                case 0x64:
                    return Key.F13;

                case 0x65:
                    return Key.F14;

                case 0x66:
                    return Key.F15;

                //case 0x70:
                //    return "KANA";

                //case 0x73:
                //    return "ABNT_C1";

                //case 0x79:
                //    return "CONVERT";

                //case 0x7B:
                //    return "NOCONVERT";

                //case 0x7D:
                //    return "YEN";

                //case 0x7E:
                //    return "ABNT_C2";

                //case 0x8D:
                //    return "NUMPADEQUALS";

                //case 0x90:
                //    return "PREVTRACK";

                //case 0x91:
                //    return "AT";

                //case 0x92:
                //    return "COLON";

                //case 0x93:
                //    return "UNDERLINE";

                //case 0x94:
                //    return "KANJI";

                //case 0x95:
                //    return "STOP";

                //case 0x96:
                //    return "AX";

                //case 0x97:
                //    return "UNLABELED";

                //case 0x99:
                //    return Key.MediaNextTrack;

                //case 0x9C:
                //    return "NUMPADENTER";

                case 0x9D:
                    return Key.RightCtrl;

                case 0xA0:
                    return Key.VolumeMute;

                //case 0xA1:
                //    return "CALCULATOR";

                case 0xA2:
                    return Key.MediaPlayPause;

                case 0xA4:
                    return Key.MediaStop;

                case 0xAE:
                    return Key.VolumeDown;

                case 0xB0:
                    return Key.VolumeUp;

                case 0xB2:
                    return Key.BrowserHome;

                //case 0xB3:
                //    return "NUMPADCOMMA";

                //case 0xB5:
                //    return "DIVIDE";

                //case 0xB7:
                //    return "SYSRQ";

                //case 0xB8:
                //    return "ALT_GR";

                case 0xC5:
                    return Key.Pause;

                case 0xC7:
                    return Key.Home;

                case 0xC8:
                    return Key.Up;

                case 0xC9:
                    return Key.PageUp;

                case 0xCB:
                    return Key.Left;

                case 0xCD:
                    return Key.Right;

                case 0xCF:
                    return Key.End;

                case 0xD0:
                    return Key.Down;

                case 0xD1:
                    return Key.PageDown;

                case 0xD2:
                    return Key.Insert;

                case 0xD3:
                    return Key.Delete;

                case 0xDB:
                    return Key.LWin;

                case 0xDC:
                    return Key.RWin;

                case 0xDD:
                    return Key.Apps;

                //case 0xDE:
                //    return "POWER";

                case 0xDF:
                    return Key.Sleep;

                //case 0xE3:
                //    return "WAKE";

                case 0xE5:
                    return Key.BrowserSearch;

                case 0xE6:
                    return Key.BrowserFavorites;

                case 0xE7:
                    return Key.BrowserRefresh;

                case 0xE8:
                    return Key.BrowserStop;

                case 0xE9:
                    return Key.BrowserForward;

                case 0xEA:
                    return Key.BrowserBack;

                //case 0xEB:
                //    return "MYCOMPUTER";

                case 0xEC:
                    return Key.LaunchMail;

                case 0xED:
                    return Key.SelectMedia;

                default:
                    return Key.None;
            }
        }

        /// <summary>
        /// Converts a <see cref="Key"/> to a dinput key code
        /// </summary>
        /// <param name="key">The key to convert</param>
        /// <returns>The dinput key code</returns>
        public static int GetKeyCode(Key key)
        {
            // TODO: Update below conversion with complete list from GetKey method?

            // Handle numbers 1-9
            if ((int)key <= 43 && (int)key >= 35)
                return (int)key - 33;

            // Handle F1-F10
            if ((int)key <= 99 && (int)key >= 90)
                return (int)key - 31;

            switch (key)
            {
                case Key.LeftShift:
                    return 0x2A;

                case Key.RightShift:
                    return 0x36;

                case Key.LeftCtrl:
                    return 0x1D;

                case Key.RightCtrl:
                    return 0x9D;

                case Key.LeftAlt:
                    return 0x38;

                case Key.RightAlt:
                    return 0xB8;

                case Key.Up:
                    return 0xC8;

                case Key.Left:
                    return 0xCB;

                case Key.Right:
                    return 0xCD;

                case Key.Down:
                    return 0xD0;

                case Key.Space:
                    return 0x39;

                case Key.Tab:
                    return 0x0F;

                case Key.D0:
                    return 0x0B;

                case Key.F11:
                    return 0x57;

                case Key.F12:
                    return 0x58;

                case Key.Escape:
                    return 0x01;

                case Key.Back:
                    return 0x0E;

                case Key.Enter:
                    return 0x1C;

                case Key.CapsLock:
                    return 0x3A;

                case Key.Pause:
                    return 0xC5;

                case Key.Home:
                    return 0xC7;

                case Key.End:
                    return 0xCF;

                case Key.PageUp:
                    return 0xC9;

                case Key.PageDown:
                    return 0xD1;

                case Key.Insert:
                    return 0xD2;

                case Key.Delete:
                    return 0xD3;

                case Key.OemTilde:
                    return 0x29;

                case Key.OemComma:
                    return 0x33;

                case Key.OemPeriod:
                    return 0x34;

                case Key.OemBackslash:
                    return 0x2B;

                case Key.A:
                    return 0x1E;

                case Key.B:
                    return 0x30;

                case Key.C:
                    return 0x2E;

                case Key.D:
                    return 0x20;

                case Key.E:
                    return 0x12;

                case Key.F:
                    return 0x21;

                case Key.G:
                    return 0x22;

                case Key.H:
                    return 0x23;

                case Key.I:
                    return 0x17;

                case Key.J:
                    return 0x24;

                case Key.K:
                    return 0x25;

                case Key.L:
                    return 0x26;

                case Key.M:
                    return 0x32;

                case Key.N:
                    return 0x31;

                case Key.O:
                    return 0x18;

                case Key.P:
                    return 0x19;

                case Key.Q:
                    return 0x10;

                case Key.R:
                    return 0x13;

                case Key.S:
                    return 0x1F;

                case Key.T:
                    return 0x14;

                case Key.U:
                    return 0x16;

                case Key.V:
                    return 0x2F;

                case Key.W:
                    return 0x11;

                case Key.X:
                    return 0x2D;

                case Key.Y:
                    return 0x15;

                case Key.Z:
                    return 0x2C;

                case Key.NumPad0:
                    return 0x52;

                case Key.NumPad1:
                    return 0x4F;

                case Key.NumPad2:
                    return 0x50;

                case Key.NumPad3:
                    return 0x51;

                case Key.NumPad4:
                    return 0x4B;

                case Key.NumPad5:
                    return 0x4C;

                case Key.NumPad6:
                    return 0x4D;

                case Key.NumPad7:
                    return 0x47;

                case Key.NumPad8:
                    return 0x48;

                case Key.NumPad9:
                    return 0x49;

                default:
                    return 0;
            }
        }

        #endregion

        #region Private Fields

        private static readonly byte[] _code1 = { 0x8B, 0xC3, 0x83, 0xE8 };
        private static readonly byte[] _code2 = { 0x0F, 0x85, 0x0C, 0x00, 0x00, 0x00, 0xC7, 0x45, 0x10 };
        private static readonly byte[] _code3 = { 0x00, 0x00, 0x00, 0xE9 };
        private static readonly byte[] _codef = { 0xFF, 0x75, 0x10, 0xFF, 0x75, 0x0C, 0xE9 };

        #endregion

        #region Private Constants

        /// <summary>
        /// The offset of the DLL almost to the end of this, from which the code is injected
        /// </summary>
        private const int CodeAdress = 0x213c3;

        #endregion
    }

    public class KeyMappingItem
    {
        public KeyMappingItem(int originalKey, int newKey)
        {
            OriginalKey = originalKey;
            NewKey = newKey;
        }

        public int OriginalKey { get; }

        public int NewKey { get; }
    }

    /// <summary>
    /// A Rayman 2 key item view model
    /// </summary>
    public class R2KeyItemViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="actionName">The name of the action the key represents</param>
        /// <param name="originalKey">The original key for the action</param>
        /// <param name="parent">The parent view model</param>
        public R2KeyItemViewModel(string actionName, Key originalKey, Rayman2ConfigViewModel parent)
        {
            ActionName = actionName;
            OriginalKey = _newKey = originalKey;
            Parent = parent;

            ResetCommand = new RelayCommand(Reset);
        }

        #endregion

        #region Private Fields

        private Key _newKey;

        #endregion

        #region Private Properties

        /// <summary>
        /// The parent view model
        /// </summary>
        private Rayman2ConfigViewModel Parent { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The name of the action the key represents
        /// </summary>
        public string ActionName { get; }

        /// <summary>
        /// The original key for the action
        /// </summary>
        public Key OriginalKey { get; }

        /// <summary>
        /// The new key for the action
        /// </summary>
        public Key NewKey
        {
            get => _newKey;
            set
            {
                _newKey = value;
                Parent.UnsavedChanges = true;
            }
        }

        #endregion

        #region Commands

        public RelayCommand ResetCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the new key to the original key
        /// </summary>
        public void Reset()
        {
            NewKey = OriginalKey;
        }

        /// <summary>
        /// Sets the initial value for the new key
        /// </summary>
        /// <param name="newKey">The new key for the action</param>
        public void SetInitialNewKey(Key newKey)
        {
            _newKey = newKey;
            OnPropertyChanged(nameof(NewKey));
        }

        #endregion
    }
}