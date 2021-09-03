using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 2 disc patch utility
    /// </summary>
    public class Utility_Rayman2_DiscPatch_ViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Utility_Rayman2_DiscPatch_ViewModel()
        {
            // Set properties
            InstallDir = Games.Rayman2.GetInstallDir();
            RequiresPatching = (InstallDir + "RAYMAN2.ICD").FileExists;

            RL.Logger?.LogInformationSource($"The R2 disc patch utility has detected that the currently installed game does {(RequiresPatching ? "require" : "not require")} the patch");

            // Create commands
            ApplyPatchCommand = new AsyncRelayCommand(ApplyPatchAsync);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game install directory
        /// </summary>
        public FileSystemPath InstallDir { get; }

        /// <summary>
        /// Indicates if game requires the patch
        /// </summary>
        public bool RequiresPatching { get; set; }

        #endregion

        #region Commands

        public ICommand ApplyPatchCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies the patch to the current installation
        /// </summary>
        /// <returns>The task</returns>
        public async Task ApplyPatchAsync()
        {
            if (!RequiresPatching)
                return;

            try
            {
                RL.Logger?.LogInformationSource($"The R2 disc patch is being applied...");

                // Write the GOG executable file
                File.WriteAllBytes(InstallDir + "Rayman2.exe", Files.Rayman2_GOG);

                // Write the GOG config file
                File.WriteAllBytes(InstallDir + "GXSetup.exe", Files.GXSetup_GOG);

                // Write default ubi.ini file
                File.WriteAllText(InstallDir + "ubi.ini", Files.ubi);

                // Delete unnecessary files
                RCPServices.File.DeleteFile(InstallDir + "RAYMAN2.ICD");

                // NOTE: The below files are no longer included in the disc installer but have to be removed here for legacy reasons
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\ANIMS0.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\ANIMS1.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\ANIMS2.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\GRAPHICS0.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\GRAPHICS1.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\GRAPHICS2.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\LEVELS1.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\LEVELS2.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MAP0.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MAP1.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MAP2.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MENU0.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MENU1.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MENU2.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MUSIC0.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MUSIC1.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\MUSIC2.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\SOUNDS0.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\SOUNDS1.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\SOUNDS2.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\TEXTURES0.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\TEXTURES1.DAT");
                RCPServices.File.DeleteFile(InstallDir + @"Data\World\Levels\TEXTURES2.DAT");

                RequiresPatching = false;

                RL.Logger?.LogInformationSource($"The R2 disc patch has been applied");

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.R2U_DiscPatchApplied);
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying R2 disc patch");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R2U_DiscPatchError);
            }
        }

        #endregion
    }
}