using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 1 fix config utility
    /// </summary>
    public class R1FixConfigUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1FixConfigUtilityViewModel()
        {
            // Create commands
            FixConfigCommand = new AsyncRelayCommand(FixConfigAsync);
        }

        #endregion

        #region Commands

        public ICommand FixConfigCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path to the config file
        /// </summary>
        public FileSystemPath ConfigPath => Games.Rayman1.GetInstallDir() + "RAYMAN.CFG";

        #endregion

        #region Public Methods

        /// <summary>
        /// Replaced the configuration file
        /// </summary>
        /// <returns>The task</returns>
        public async Task FixConfigAsync()
        {
            // Get the path
            var path = ConfigPath;

            try
            {
                // Replace the file
                File.WriteAllBytes(path, Files.RAYMAN);

                RCFCore.Logger?.LogInformationSource($"The Rayman 1 config file has been fixed");

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.R1U_FixConfig_Success);
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying R1 fix config patch");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_FixConfig_Error);
            }
        }

        #endregion
    }
}