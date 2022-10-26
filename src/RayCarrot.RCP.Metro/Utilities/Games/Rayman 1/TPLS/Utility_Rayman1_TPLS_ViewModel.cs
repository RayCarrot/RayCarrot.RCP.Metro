#nullable disable
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman 1 TPLS utility
/// </summary>
public class Utility_Rayman1_TPLS_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Rayman1_TPLS_ViewModel()
    {
        // Create the commands
        InstallTPLSCommand = new AsyncRelayCommand(InstallTPLSAsync);
        UninstallTPLSCommand = new AsyncRelayCommand(UninstallTPLSAsync);
        EnableToggledCommand = new AsyncRelayCommand(EnableToggledAsync);

        // Check if TPLS is installed under the default location
        if (!AppFilePaths.R1TPLSDir.DirectoryExists)
            Data.Utility_TPLSData = null;
        else if (Data.Utility_TPLSData == null)
            Data.Utility_TPLSData = new UserData_TPLSData(AppFilePaths.R1TPLSDir);

        if (Data.Utility_TPLSData != null)
        {
            SelectedRaymanVersion = Data.Utility_TPLSData.RaymanVersion;
            IsEnabled = Data.Utility_TPLSData.IsEnabled;
        }

        VerifyTPLS();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand InstallTPLSCommand { get; }
    public ICommand UninstallTPLSCommand { get; }
    public ICommand EnableToggledCommand { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if TPLS can be enabled
    /// </summary>
    public bool CanEnableTPLS { get; set; }

    /// <summary>
    /// The selected Rayman version
    /// </summary>
    public Utility_Rayman1_TPLS_RaymanVersion SelectedRaymanVersion { get; set; }

    /// <summary>
    /// Indicates if the utility is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Verifies if TPLS can be enabled
    /// </summary>
    public void VerifyTPLS()
    {
        if (Data.Utility_TPLSData == null)
            return;

        // Check if TPLS can be enabled (i.e. if the pre-9 version is still installed)
        CanEnableTPLS = Data.Utility_TPLSData.DOSBoxFilePath.FileExists;
    }

    /// <summary>
    /// Installs TPLS
    /// </summary>
    public async Task InstallTPLSAsync()
    {
        try
        {
            Logger.Info("The TPLS utility is downloading...");

            // Check if the directory exists
            if (AppFilePaths.R1TPLSDir.DirectoryExists)
                // Delete the directory
                Services.File.DeleteDirectory(AppFilePaths.R1TPLSDir);

            // Download the files
            if (!await App.DownloadAsync(new Uri[]
                {
                    new Uri(AppURLs.R1_TPLS_Url),
                }, true, AppFilePaths.R1TPLSDir))
            {
                // If canceled, delete the directory
                Services.File.DeleteDirectory(AppFilePaths.R1TPLSDir);
                return;
            }

            // Save
            Services.Data.Utility_TPLSData = new UserData_TPLSData(AppFilePaths.R1TPLSDir);

            // Update the version
            await Data.Utility_TPLSData.UpdateConfigAsync();

            Logger.Info("The TPLS utility has been downloaded");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Installing TPLS");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_TPLSInstallationFailed, Resources.R1U_TPLSInstallationFailedHeader);
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
        if (!await Services.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSConfirmUninstall, Resources.R1U_TPLSConfirmUninstallHeader, MessageType.Question, true))
            return;

        try
        {
            Services.File.DeleteDirectory(Services.Data.Utility_TPLSData.InstallDir);

            await Services.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSUninstallSuccess, Resources.R1U_TPLSUninstallSuccessHeader, MessageType.Success);

            Services.Data.Utility_TPLSData = null;

            Logger.Info("The TPLS utility has been uninstalled");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Uninstalling TPLS");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_TPLSUninstallError, Resources.R1U_TPLSUninstallErrorHeader);
        }
    }

    public async Task EnableToggledAsync()
    {
        Data.Utility_TPLSData.IsEnabled = IsEnabled;

        await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.Rayman1.GetInstallation(), RefreshFlags.GameInfo));
    }

    #endregion
}