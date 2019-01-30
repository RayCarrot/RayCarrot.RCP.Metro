using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        public Rayman2Config()
        {
            InitializeComponent();
        }
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
            IsHorizontalWidescreen = true;

            AsyncLock = new AsyncLock();

            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        #endregion

        #region Private Fields

        private int _resX;

        private int _resY;

        private bool _lockToScreenRes;

        private bool _widescreenSupport;

        #endregion

        #region Private Properties

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

                if (value && LockToScreenRes)
                    ResX = (int)SystemParameters.PrimaryScreenWidth;
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
            ConfigPath = GetUbiIniPath();

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

            // Re-create the section if it doesn't exist
            if (!ConfigData.Exists)
                ConfigData.ReCreate();

            ResX = ConfigData.FormattedGLI_Mode.ResX;
            ResY = ConfigData.FormattedGLI_Mode.ResY;

            UnsavedChanges = false;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                try
                {
                    ConfigData.GLI_Mode = new RayGLI_Mode()
                    {
                        ColorMode = ConfigData.FormattedGLI_Mode?.ColorMode ?? 16,
                        IsWindowed = ConfigData.FormattedGLI_Mode?.IsWindowed ?? false,
                        ResX = ResX,
                        ResY = ResY
                    }.ToString();

                    await SetAspectRatioAsync();
                    // TODO: If widescreen support is true, edit .exe file - if false, edit to make sure it's removed from previous sessions

                    ConfigData.Save();

                    UnsavedChanges = false;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving R2 ubi.ini data");
                    // TODO: Handle error
                }
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
                // Get the file path
                FileSystemPath path = Games.Rayman2.GetLaunchInfo().Path;

                // Make sure the file exists
                if (!path.FileExists)
                {
                    // TODO: Fill out

                    if (WidescreenSupport)
                        await RCF.MessageUI.DisplayMessageAsync("", "Error", MessageType.Error);
                    else
                        RCF.Logger.LogWarningSource("");

                    return;
                }

                // Get the size to determine the version
                var length = path.GetSize();

                // Get the byte location
                int location = 0;

                // Check if it's the disc version
                if ((int)length.Bytes == 676352)
                    location = 633496;

                // Check if it's the GOG version
                else if ((int)length.Bytes == 1468928)
                    location = 640152;

                // Cancel if unknown version
                if (location == 0)
                {
                    // TODO: Fill out

                    if (WidescreenSupport)
                        await RCF.MessageUI.DisplayMessageAsync("", "Error", MessageType.Error);
                    else
                        RCF.Logger.LogWarningSource("");

                    return;
                }

                // Apply widescreen patch
                if (WidescreenSupport)
                {
                    // Get the aspect ratio
                    float ratio = (float)ResY / ResX;

                    // Multiply by 4/3
                    ratio = ratio * (4.0F / 3.0F);

                    // Get the hex representation of the value
                    string value = Hex.GetHexRepresentation(ratio);

                    // Get the byte array to write
                    byte[] input = Enumerable.Range(0, value.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(value.Substring(x, 2), 16))
                        .ToArray();

                    // Open the file
                    using (Stream stream = File.Open(path, FileMode.Open))
                    {
                        // Set the position
                        stream.Position = location;

                        // Write the bytes
                        await stream.WriteAsync(input, 0, input.Length);
                    }
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
                        if (buffer[0] != 0 || buffer[1] != 0 || buffer[2] != 128 || buffer[3] != 63)
                        {
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
    }
}