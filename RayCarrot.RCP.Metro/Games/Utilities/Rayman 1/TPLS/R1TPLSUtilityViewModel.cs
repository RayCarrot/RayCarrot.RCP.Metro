using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

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
            StartTPLSCommand = new RelayCommand(StartTPLS);
            StopTPLSCommand = new RelayCommand(StopTPLS);
            UninstallTPLSCommand = new AsyncRelayCommand(UninstallTPLSAsync);

            // Check if TPLS is installed under the default location
            if (!CommonPaths.TPLSDir.DirectoryExists)
                Data.TPLSData = null;
            else if (Data.TPLSData == null)
                Data.TPLSData = new TPLSData(CommonPaths.TPLSDir);
        }

        #endregion

        #region Commands

        public ICommand InstallTPLSCommand { get; }

        public ICommand StartTPLSCommand { get; }

        public ICommand StopTPLSCommand { get; }

        public ICommand UninstallTPLSCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Installs TPLS
        /// </summary>
        public async Task InstallTPLSAsync()
        {
            // Verify the install directory
            if (!await VerifyInstallDirAsync(Games.Rayman1.GetInstallDir(false)))
                return;

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

                RCFCore.Logger?.LogInformationSource($"The TPLS utility has been downloaded");
            }
            catch (Exception ex)
            {
                ex.HandleError("Installing TPLS");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_TPLSInstallationFailed, Resources.R1U_TPLSInstallationFailedHeader);
            }
        }

        /// <summary>
        /// Starts the TPLS service
        /// </summary>
        public void StartTPLS()
        {
            new TPLS().Start(null);
        }

        /// <summary>
        /// Stops a running TPLS service
        /// </summary>
        public void StopTPLS()
        {
            TPLS.StopCurrent();
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

        #region Private Static Methods

        /// <summary>
        /// Verifies the specified install directory for a valid Rayman installation
        /// </summary>
        /// <param name="dir">The directory</param>
        /// <returns>True if it is valid, false if not</returns>
        private static async Task<bool> VerifyInstallDirAsync(FileSystemPath dir)
        {
            var files = new FileSystemPath[]
            {
                dir + "RAYMAN.EXE",
                dir + "VIGNET.DAT",
            };

            if (!files.FilesExist() || !Directory.Exists(dir + "PCMAP"))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSInvalidDirectory, Resources.R1U_TPLSInvalidDirectoryHeader, MessageType.Error);
                return false;
            }

            return true;
        }

        #endregion
    }
}