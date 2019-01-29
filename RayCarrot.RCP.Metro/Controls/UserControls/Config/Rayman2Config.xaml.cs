using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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
        }

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
        }

        #endregion

        #region Private Fields

        private bool _lockToScreenRes;

        private bool _widescreenSupport;

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
        public int ResX { get; set; }

        /// <summary>
        /// The current vertical resolution
        /// </summary>
        public int ResY { get; set; }

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

                if (WidescreenSupport)
                    ResX = (int)SystemParameters.PrimaryScreenWidth;
                else
                    ResX = (int)Math.Round(((double)ResY / 3) * 4);
            }
        }

        /// <summary>
        /// Indicates if the widescreen support is horizontal, otherwise it is vertical
        /// </summary>
        public bool IsHorizontalWidescreen { get; set; }

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