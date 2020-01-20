using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 1 TPLS utility
    /// </summary>
    public class R1TPLSUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1TPLSUtilityViewModel()
        {
            // Create the commands
            InstallTPLSCommand = new AsyncRelayCommand(InstallTPLSAsync);
            UninstallTPLSCommand = new AsyncRelayCommand(UninstallTPLSAsync);

            // Check if TPLS is installed under the default location
            if (!CommonPaths.TPLSDir.DirectoryExists)
                Data.TPLSData = null;
            else if (Data.TPLSData == null)
                Data.TPLSData = new TPLSData(CommonPaths.TPLSDir);

            if (Data.TPLSData != null)
            {
                _selectedRaymanVersion = Data.TPLSData.RaymanVersion;
                _isEnabled = Data.TPLSData.IsEnabled;
            }

            VerifyTPLS();
        }

        #endregion

        #region Commands

        public ICommand InstallTPLSCommand { get; }

        public ICommand UninstallTPLSCommand { get; }

        #endregion

        #region Private Fields

        private TPLSRaymanVersion _selectedRaymanVersion;

        private bool _isEnabled;

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if TPLS can be enabled
        /// </summary>
        public bool CanEnableTPLS { get; set; }

        /// <summary>
        /// The selected Rayman version
        /// </summary>
        public TPLSRaymanVersion SelectedRaymanVersion
        {
            get => _selectedRaymanVersion;
            set
            {
                _selectedRaymanVersion = value;

                if (Data.TPLSData != null)
                {
                    Data.TPLSData.RaymanVersion = value;
                    _ = Task.Run(Data.TPLSData.UpdateConfigAsync);
                }
            }
        }

        /// <summary>
        /// Indicates if the utility is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                Data.TPLSData.IsEnabled = value;

                _ = Task.Run(async () => await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.Rayman1, false, false, false, true)));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Verifies if TPLS can be enabled
        /// </summary>
        public void VerifyTPLS()
        {
            if (Data.TPLSData == null)
                return;

            // Check if TPLS can be enabled (i.e. if the pre-9 version is still installed)
            CanEnableTPLS = Data.TPLSData.DOSBoxFilePath.FileExists;
        }

        /// <summary>
        /// Installs TPLS
        /// </summary>
        public async Task InstallTPLSAsync()
        {
            try
            {
                RCFCore.Logger?.LogInformationSource($"The TPLS utility is downloading...");

                // Check if the directory exists
                if (CommonPaths.TPLSDir.DirectoryExists)
                    // Delete the directory
                    RCFRCP.File.DeleteDirectory(CommonPaths.TPLSDir);

                // Download the files
                if (!await App.DownloadAsync(new Uri[]
                {
                    new Uri(CommonUrls.R1_TPLS_Url),
                }, true, CommonPaths.TPLSDir))
                {
                    // If canceled, delete the directory
                    RCFRCP.File.DeleteDirectory(CommonPaths.TPLSDir);
                    return;
                }

                // Save
                RCFRCP.Data.TPLSData = new TPLSData(CommonPaths.TPLSDir);

                // Update the version
                await Data.TPLSData.UpdateConfigAsync();

                RCFCore.Logger?.LogInformationSource($"The TPLS utility has been downloaded");
            }
            catch (Exception ex)
            {
                ex.HandleError("Installing TPLS");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_TPLSInstallationFailed, Resources.R1U_TPLSInstallationFailedHeader);
            }
            finally
            {
                VerifyTPLS();
            }
        }

        /// <summary>
        /// Uninstalls TPLS
        /// </summary>
        public async Task UninstallTPLSAsync()
        {
            // Have user confirm uninstall
            if (!await RCFUI.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSConfirmUninstall, Resources.R1U_TPLSConfirmUninstallHeader, MessageType.Question, true))
                return;

            try
            {
                RCFRCP.File.DeleteDirectory(RCFRCP.Data.TPLSData.InstallDir);

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSUninstallSuccess, Resources.R1U_TPLSUninstallSuccessHeader, MessageType.Success);

                RCFRCP.Data.TPLSData = null;

                RCFCore.Logger?.LogInformationSource($"The TPLS utility has been uninstalled");
            }
            catch (Exception ex)
            {
                ex.HandleError("Uninstalling TPLS");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_TPLSUninstallError, Resources.R1U_TPLSUninstallErrorHeader);
            }
        }

        #endregion
    }
}