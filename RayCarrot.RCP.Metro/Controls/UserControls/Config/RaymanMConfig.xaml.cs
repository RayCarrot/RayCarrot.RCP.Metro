using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.Rayman;
using System.Threading.Tasks;
using System.Windows;
using ByteSizeLib;
using IniParser.Model;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for RaymanMConfig.xaml
    /// </summary>
    public partial class RaymanMConfig : BaseUserControl<RaymanMConfigViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window">The parent window</param>
        public RaymanMConfig(Window window)
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

    public class RaymanMConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanMConfigViewModel()
        {
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

        private bool _fullscreenMode;

        private bool _triLinear;

        private bool _tnL;

        private bool _isTextures32Bit;

        private bool _compressedTextures;

        private int _videoQuality;

        private bool _autoVideoQuality;

        private bool _isVideo32Bpp;

        private RMLanguages _currentLanguage;

        private bool _controllerSupport;

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
        public RMUbiIniHandler ConfigData { get; set; }

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
                ResX = (int)Math.Round((double)ResY / 3 * 4);
            }
        }

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
        public RMLanguages CurrentLanguage
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
            RCF.Logger.LogInformationSource("Rayman M config is being set up");

            // Get the current dinput type
            var dinputType = GetCurrentDinput();

            RCF.Logger.LogInformationSource($"The dinput type has been retrieved as {dinputType}");

            AllowDinputHack = dinputType != RMDinput.Unknown;
            ControllerSupport = dinputType == RMDinput.Controller;

            // If the primary config file does not exist, create a new one
            if (!CommonPaths.UbiIniPath1.FileExists)
            {
                try
                {
                    // Create the file
                    RCFRCP.File.CreateFile(CommonPaths.UbiIniPath1);

                    RCF.Logger.LogInformationSource($"A new ubi.ini file has been created under {CommonPaths.UbiIniPath1}");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Creating ubi.ini file");

                    await RCF.MessageUI.DisplayMessageAsync($"No valid ubi.ini file was found and creating a new one failed. Try running the program as administrator " +
                                                            $"or changing the folder permissions for the following path: {CommonPaths.UbiIniPath1.Parent}", "Error", MessageType.Error);

                    throw;
                }
            }

            // If the secondary config file does not exist, attempt to create a new one
            if (!CommonPaths.UbiIniPath2.FileExists)
            {
                try
                {
                    // Create the file
                    RCFRCP.File.CreateFile(CommonPaths.UbiIniPath2);

                    RCF.Logger.LogInformationSource($"A new ubi.ini file has been created under {CommonPaths.UbiIniPath2}");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Creating ubi.ini file");
                }
            }

            // Load the configuration data
            ConfigData = new RMUbiIniHandler(CommonPaths.UbiIniPath1);

            RCF.Logger.LogInformationSource($"The ubi.ini file has been loaded");

            // Re-create the section if it doesn't exist
            if (!ConfigData.Exists)
            {
                ConfigData.ReCreate();
                RCF.Logger.LogInformationSource($"The ubi.ini section for Rayman M was recreated");
            }

            var gliMode = ConfigData.FormattedGLI_Mode;

            if (gliMode != null)
            {
                ResX = gliMode.ResX;
                ResY = gliMode.ResY;
                FullscreenMode = !gliMode.IsWindowed;
                IsTextures32Bit = gliMode.ColorMode != 16;
            }
            else
            {
                LockToScreenRes = true;
                FullscreenMode = true;
                IsTextures32Bit = true;
            }

            TriLinear = ConfigData.FormattedTriLinear;
            TnL = ConfigData.FormattedTnL;
            CompressedTextures = ConfigData.FormattedTexturesCompressed;
            VideoQuality = ConfigData.FormattedVideo_WantedQuality ?? 4;
            AutoVideoQuality = ConfigData.FormattedVideo_AutoAdjustQuality;
            IsVideo32Bpp = ConfigData.FormattedVideo_BPP != 16;
            CurrentLanguage = ConfigData.FormattedRMLanguage ?? RMLanguages.English;

            UnsavedChanges = false;

            RCF.Logger.LogInformationSource($"All section properties have been loaded");
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"Rayman M configuration is saving...");

                try
                {
                    // Update the config data
                    UpdateConfig();

                    // Save the config data
                    ConfigData.Save();

                    // Attempt to copy data to secondary file
                    if (CommonPaths.UbiIniPath2.FileExists)
                    {
                        try
                        {
                            // Get the current data
                            var sectionData = ConfigData.GetSectionData();

                            // Load the file data
                            var secondaryDataHandler = new DuplicateSectionUbiIniHandler(CommonPaths.UbiIniPath2, RMUbiIniHandler.SectionName);

                            // Duplicate the data
                            secondaryDataHandler.Duplicate(sectionData);

                            // Save the file
                            secondaryDataHandler.Save();
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Saving RM ubi.ini secondary data");
                        }
                    }

                    RCF.Logger.LogInformationSource($"Rayman M configuration has been saved");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving RM ubi.ini data");
                    await RCF.MessageUI.DisplayMessageAsync("An error occurred when saving your Rayman M configuration", "Error saving", MessageType.Error);
                    return;
                }

                try
                {
                    // Get the current dinput type
                    var dt = GetCurrentDinput();
                    var path = GetDinputPath();

                    RCF.Logger.LogInformationSource($"The dinput type has been retrieved as {dt}");

                    if (ControllerSupport)
                    {
                        if (dt != RMDinput.Controller)
                        {
                            if (dt != RMDinput.None)
                                // Attempt to delete existing dinput file
                                File.Delete(path);

                            // Write controller patch
                            File.WriteAllBytes(path, Files.dinput_controller);
                        }
                    }
                    else if (dt == RMDinput.Controller)
                    {
                        // Attempt to delete existing dinput file
                        File.Delete(path);
                    }

                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving RM dinput hack data");
                    await RCF.MessageUI.DisplayMessageAsync("An error occurred when saving your Rayman M configuration. Some data may have been saved.", "Error saving", MessageType.Error);
                    return;
                }

                UnsavedChanges = false;

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("Your changes have been saved");

                OnSave();
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
                ColorMode = IsTextures32Bit ? 32 : 16,
                IsWindowed = !FullscreenMode,
                ResX = ResX,
                ResY = ResY
            }.ToString();

            ConfigData.FormattedTriLinear = TriLinear;
            ConfigData.FormattedTnL = TnL;
            ConfigData.FormattedTexturesCompressed = CompressedTextures;
            ConfigData.Video_WantedQuality = VideoQuality.ToString();
            ConfigData.FormattedVideo_AutoAdjustQuality = AutoVideoQuality;
            ConfigData.Video_BPP = IsVideo32Bpp ? "32" : "16";
            ConfigData.Language = CurrentLanguage.ToString();
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the current dinput file used for Rayman M
        /// </summary>
        /// <returns>The current dinput file used</returns>
        private static RMDinput GetCurrentDinput()
        {
            var path = GetDinputPath();

            if (!path.FileExists)
                return RMDinput.None;

            try
            {
                var size = path.GetSize();

                if (size == new ByteSize(118272))
                    return RMDinput.Controller;

                return RMDinput.Unknown;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting RM dinput file size");
                return RMDinput.Unknown;
            }
        }

        /// <summary>
        /// Gets the current dinput.dll path for Rayman M
        /// </summary>
        /// <returns>The path</returns>
        private static FileSystemPath GetDinputPath()
        {
            return Games.RaymanM.GetInfo().InstallDirectory + "dinput8.dll";
        }

        #endregion

        #region Private Enum

        /// <summary>
        /// The available types of Rayman M dinput8.dll file
        /// </summary>
        private enum RMDinput
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

        #region Private Classes

        /// <summary>
        /// Provides support to duplicate a section
        /// in a ubi ini file
        /// </summary>
        private class DuplicateSectionUbiIniHandler : UbiIniHandler
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="path">The path of the ubi.ini file</param>
            /// <param name="sectionName">The name of the section to retrieve, usually the name of the game</param>
            public DuplicateSectionUbiIniHandler(FileSystemPath path, string sectionName) : base(path, sectionName)
            {

            }

            public void Duplicate(KeyDataCollection sectionData)
            {
                // Recreate the section
                ReCreate();

                // Add all new keys
                sectionData.ForEach(x => Section.AddKey(x));
            }
        }

        #endregion
    }
}