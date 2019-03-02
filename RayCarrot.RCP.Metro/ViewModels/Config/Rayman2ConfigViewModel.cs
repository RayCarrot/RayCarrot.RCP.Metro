using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// View model for the Rayman 2 configuration
    /// </summary>
    public class Rayman2ConfigViewModel : BaseUbiIniGameConfigViewModel<R2UbiIniHandler>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman2ConfigViewModel() : base(Games.Rayman2)
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

        #region Public Properties

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

        #region Protected Override Methods

        /// <summary>
        /// Loads the <see cref="BaseUbiIniGameConfigViewModel{Handler}.ConfigData"/>
        /// </summary>
        /// <returns>The config data</returns>
        protected override Task<R2UbiIniHandler> LoadConfigAsync()
        {
            // Load the configuration data
            return Task.FromResult(new R2UbiIniHandler(ConfigPath));
        }

        /// <summary>
        /// Imports the <see cref="BaseUbiIniGameConfigViewModel{Handler}.ConfigData"/>
        /// </summary>
        /// <returns>The task</returns>
        protected override async Task ImportConfigAsync()
        {
            var gliMode = ConfigData.FormattedGLI_Mode;

            if (gliMode != null)
            {
                ResX = gliMode.ResX;
                ResY = gliMode.ResY;
            }
            else
            {
                LockToScreenRes = true;
            }

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

            if (GetCurrentDinput() == R2Dinput.Mapping)
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
                    r2Item?.SetInitialNewKey(R2ButtonMappingManager.GetKey(item.NewKey));
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="BaseUbiIniGameConfigViewModel{Handler}.ConfigData"/>
        /// </summary>
        /// <returns>The task</returns>
        protected override Task UpdateConfigAsync()
        {
            ConfigData.GLI_Mode = new RayGLI_Mode()
            {
                ColorMode = ConfigData.FormattedGLI_Mode?.ColorMode ?? 16,
                IsWindowed = ConfigData.FormattedGLI_Mode?.IsWindowed ?? false,
                ResX = ResX,
                ResY = ResY
            }.ToString();

            ConfigData.Language = CurrentLanguage.ToString();

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
                    RCFRCP.File.CreateFile(newFile);

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
        }

        /// <summary>
        /// Saving
        /// </summary>
        /// <returns>The task</returns>
        protected override async Task OnSaveAsync()
        {
            // Set the aspect ratio
            await SetAspectRatioAsync();

            try
            {
                // Get the current dinput type
                var dt = GetCurrentDinput();
                var path = GetDinputPath();

                RCF.Logger.LogInformationSource($"The dinput type has been retrieved as {dt}");

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
                ex.HandleError("Setting R2 aspect ratio");

                await RCF.MessageUI.DisplayMessageAsync("An error occurred when setting the Rayman 2 aspect ratio", "Error", MessageType.Error);

                if (WidescreenSupport)
                    throw;
            }
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
}