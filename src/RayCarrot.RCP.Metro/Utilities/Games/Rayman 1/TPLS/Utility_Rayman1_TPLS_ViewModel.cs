using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Clients.Data;
using RayCarrot.RCP.Metro.Games.Clients.DosBox;
using RayCarrot.RCP.Metro.Games.Clients.DosBox.Data;
using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman 1 TPLS utility
/// </summary>
public class Utility_Rayman1_TPLS_ViewModel : BaseRCPViewModel
{
    #region Constructor

    public Utility_Rayman1_TPLS_ViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        // Create the commands
        InstallTPLSCommand = new AsyncRelayCommand(InstallTPLSAsync);
        UninstallTPLSCommand = new AsyncRelayCommand(UninstallTPLSAsync);

        Rayman1TplsData? data = gameInstallation.GetObject<Rayman1TplsData>(GameDataKey.R1_TplsData);

        if (data != null)
        {
            // Make sure it's still installed
            if (!data.InstallDir.DirectoryExists)
            {
                gameInstallation.SetObject<Rayman1TplsData>(GameDataKey.R1_TplsData, null);
                data = null;
            }
            else
            {
                _selectedRaymanVersion = data.RaymanVersion;
            }
        }

        IsInstalled = data != null;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand InstallTPLSCommand { get; }
    public ICommand UninstallTPLSCommand { get; }

    #endregion

    #region Private Fields

    private Utility_Rayman1_TPLS_RaymanVersion _selectedRaymanVersion;

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }

    /// <summary>
    /// The selected Rayman version
    /// </summary>
    public Utility_Rayman1_TPLS_RaymanVersion SelectedRaymanVersion
    {
        get => _selectedRaymanVersion;
        set
        {
            _selectedRaymanVersion = value;

            updateVersion();

            async void updateVersion()
            {
                Rayman1TplsData? data = GameInstallation.GetObject<Rayman1TplsData>(GameDataKey.R1_TplsData);

                if (data == null)
                    return;

                data.RaymanVersion = value;
                await data.UpdateConfigAsync();
                GameInstallation.SetObject(GameDataKey.R1_TplsData, data);
            }
        }
    }

    public bool IsInstalled { get; set; }

    #endregion

    #region Public Methods

    public async Task SetInstallationAsync(FileSystemPath installDir, Utility_Rayman1_TPLS_RaymanVersion version, bool isEnabled)
    {
        // Create the TPLS data for the game
        Rayman1TplsData data = new(installDir) { RaymanVersion = version };
        GameInstallation.SetObject(GameDataKey.R1_TplsData, data);

        // Perform the initial config update
        await data.UpdateConfigAsync();

        // Create an emulator installation
        GameClientDescriptor gameClientDescriptor = Services.GameClients.GetGameClientDescriptor<DosBoxGameClientDescriptor>();
        GameClientInstallation gameClientInstallation = await Services.GameClients.AddGameClientAsync(gameClientDescriptor, InstallLocation.FromFilePath(data.DosBoxFilePath), new ConfigureGameClientInstallation(x =>
        {
            // Add the TPLS config file to the data
            x.ModifyObject<DosBoxConfigFilePaths>(GameClientDataKey.DosBox_ConfigFilePaths,
                y => y.FilePaths.Add(data.ConfigFilePath));

            // Set the game client installation id
            data.GameClientInstallationId = x.InstallationId;
            GameInstallation.SetObject(GameDataKey.R1_TplsData, data);

            // Limit to only work on this game installation
            RequiredGameInstallations requiredGames = new();
            requiredGames.GameInstallationIds.Add(GameInstallation.InstallationId);
            x.SetObject(GameClientDataKey.RCP_RequiredGameInstallations, requiredGames);

            // Give the emulator a name so it's apparent what it's for
            // TODO-UPDATE: Localize
            x.SetValue(GameClientDataKey.RCP_CustomName, "DOSBox (per-level soundtrack)");
        }));

        if (isEnabled)
            await Services.GameClients.AttachGameClientAsync(GameInstallation, gameClientInstallation);
    }

    /// <summary>
    /// Installs TPLS
    /// </summary>
    public async Task InstallTPLSAsync()
    {
        try
        {
            Logger.Info("The TPLS utility is downloading...");

            // Use a unique directory for each game to avoid conflicts
            FileSystemPath tplsDir = AppFilePaths.UtilitiesBaseDir + ("TPLS_" + GameInstallation.InstallationId);

            // Check if the directory exists
            if (tplsDir.DirectoryExists)
                // Delete the directory
                Services.File.DeleteDirectory(tplsDir);

            // Download the files
            if (!await App.DownloadAsync(new[] { new Uri(AppURLs.R1_TPLS_Url), }, true, tplsDir))
            {
                // If canceled, delete the directory
                Services.File.DeleteDirectory(tplsDir);
                return;
            }

            // Set the installation
            await SetInstallationAsync(tplsDir, Utility_Rayman1_TPLS_RaymanVersion.Auto, true);

            IsInstalled = true;

            Logger.Info("The TPLS utility has been downloaded");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Installing TPLS");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_TPLSInstallationFailed, Resources.R1U_TPLSInstallationFailedHeader);
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
            Rayman1TplsData? data = GameInstallation.GetObject<Rayman1TplsData>(GameDataKey.R1_TplsData);

            if (data != null)
            {
                Services.File.DeleteDirectory(data.InstallDir);
                GameInstallation.SetObject<Rayman1TplsData>(GameDataKey.R1_TplsData, null);

                if (data.GameClientInstallationId != null)
                    await Services.GameClients.RemoveGameClientAsync(data.GameClientInstallationId);
            }
            else
            {
                Logger.Warn("The TPLS data was null when uninstalling");
            }

            IsInstalled = false;

            await Services.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSUninstallSuccess, Resources.R1U_TPLSUninstallSuccessHeader, MessageType.Success);

            Logger.Info("The TPLS utility has been uninstalled");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Uninstalling TPLS");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_TPLSUninstallError, Resources.R1U_TPLSUninstallErrorHeader);
        }
    }

    #endregion
}