using System;
using System.Collections.Generic;
using System.IO;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.Rayman;
using System.Threading.Tasks;
using System.Windows;

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

            // TODO: Check if controller support dll is there
            // ControllerSupport = 

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
            }
            else
            {
                // TODO: Check lock to screen resolution
            }

            // TODO: Get other properties

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
                    // TODO: Save controller fix
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
                // ColorMode = ConfigData.FormattedGLI_Mode?.ColorMode ?? 16,
                // IsWindowed = ConfigData.FormattedGLI_Mode?.IsWindowed ?? false,
                ResX = ResX,
                ResY = ResY
            }.ToString();

            // TODO: Update other properti
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the current dinput.dll path for Rayman 2
        /// </summary>
        /// <returns>The path</returns>
        private static FileSystemPath GetDinputPath()
        {
            return Games.Rayman2.GetInfo().InstallDirectory + "dinput.dll";
        }

        #endregion
    }
}