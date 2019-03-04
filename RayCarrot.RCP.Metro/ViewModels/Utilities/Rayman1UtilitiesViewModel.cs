using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// VIew model Rayman 1 utilities
    /// </summary>
    public class Rayman1UtilitiesViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman1UtilitiesViewModel()
        {
            AvailableRaymanVersions = new string[]
            {
                "Auto",
                "1.12",
                "1.20",
                "1.21"
            };

            AvailableDosBoxVersions = new string[]
            {
                "0.74",
                "SVN Daum"
            };

            // Check if TPLS is installed under the default location
            if (CommonPaths.TPLSDir.DirectoryExists)
            {
                if (Data.TPLSData == null)
                    Data.TPLSData = new TPLSData(CommonPaths.TPLSDir);
            }
            else
            {
                Data.TPLSData = null;
            }

            InstallCommand = new AsyncRelayCommand(InstallAsync);
            UninstallCommand = new AsyncRelayCommand(UninstallAsync);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The available Rayman versions to select
        /// </summary>
        public string[] AvailableRaymanVersions { get; }

        /// <summary>
        /// The available DosBox versions to select
        /// </summary>
        public string[] AvailableDosBoxVersions { get; }

        #endregion

        #region Commands

        public ICommand InstallCommand { get; }

        public ICommand UninstallCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Installs TPLS
        /// </summary>
        public async Task InstallAsync()
        {
            // Verify the install directory
            if (!await VerifyInstallDirAsync(Games.Rayman1.GetInfo().InstallDirectory))
                return;

            try
            {
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
                    // If cancelled, delete the directory
                    RCFRCP.File.DeleteDirectory(CommonPaths.TPLSDir);
                    return;
                }

                // Save
                RCFRCP.Data.TPLSData = new TPLSData(CommonPaths.TPLSDir);
            }
            catch (Exception ex)
            {
                ex.HandleError("Installing TPLS");
                await RCF.MessageUI.DisplayMessageAsync("Installation failed.", "Installation Failed", MessageType.Error);
            }
        }

        /// <summary>
        /// Uninstalls TPLS
        /// </summary>
        public async Task UninstallAsync()
        {
            // Have user confirm uninstall
            if (!await RCF.MessageUI.DisplayMessageAsync("Are you sure you want to uninstall the PlayStation Soundtrack utility?", "Confirm Uninstall", MessageType.Question, true))
                return;

            try
            {
                RCFRCP.File.DeleteDirectory(RCFRCP.Data.TPLSData.InstallDir);
                await RCF.MessageUI.DisplayMessageAsync("Utility was successfully uninstalled", "Uninstall Complete", MessageType.Success);

                RCFRCP.Data.TPLSData = null;
            }
            catch (Exception ex)
            {
                ex.HandleError("Uninstalling TPLS");
                await RCF.MessageUI.DisplayMessageAsync($"An error occurred uninstalling. Error message: {ex.Message}", "Uninstallation Failed", MessageType.Error);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Verifies the specified install directory for a valid Rayman installation
        /// </summary>
        /// <param name="dir">The directory</param>
        /// <returns>True if it is valid, false if not</returns>
        private async Task<bool> VerifyInstallDirAsync(FileSystemPath dir)
        {
            var files = new FileSystemPath[]
            {
                dir + "RAYMAN.EXE",
                dir + "VIGNET.DAT",
            };

            if (!files.FilesExist())
            {
                await RCF.MessageUI.DisplayMessageAsync("The selected directory does not contain a valid installation", "Missing Files", MessageType.Error);
                return false;
            }

            if (!Directory.Exists(dir + "PCMAP"))
            {
                await RCF.MessageUI.DisplayMessageAsync("The selected directory does not contain a valid installation", "Missing Directory", MessageType.Error);
                return false;
            }

            return true;
        }

        #endregion
    }
}