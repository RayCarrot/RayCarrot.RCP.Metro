using System.IO;

namespace RayCarrot.RCP.Metro.Games.Data;

/// <summary>
/// TPLS data for the Rayman 1 utility
/// </summary>
public class Rayman1TplsData
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="installDir">The directory it is installed under</param>
    public Rayman1TplsData(FileSystemPath installDir)
    {
        InstallDir = installDir;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The directory it is installed under
    /// </summary>
    public FileSystemPath InstallDir { get; }

    /// <summary>
    /// The id for the game client installation added for the TPLS DOSBox version
    /// </summary>
    public string? GameClientInstallationId { get; set; }

    /// <summary>
    /// The selected Rayman version to search for
    /// </summary>
    public Utility_Rayman1_TPLS_RaymanVersion RaymanVersion { get; set; }

    /// <summary>
    /// Gets the DOSBox TPLS config file path
    /// </summary>
    public FileSystemPath ConfigFilePath => InstallDir + "TPLS.conf";

    /// <summary>
    /// Gets the DOSBox exe file path
    /// </summary>
    public FileSystemPath DosBoxFilePath => InstallDir + "dosbox.exe";

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the config file
    /// </summary>
    /// <returns>The task</returns>
    public async Task UpdateConfigAsync()
    {
        static string GetVersionName(Utility_Rayman1_TPLS_RaymanVersion version) => version switch
        {
            Utility_Rayman1_TPLS_RaymanVersion.Auto => "auto",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_12_0 => "1.12.0",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_20 => "1.20",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_21 => "1.21",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_00 => "1.00",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_12_1 => "1.12.1",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_12_2 => "1.12.2",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_12_Unprotected => "1.12_Unprotected",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_10 => "1.10",
            Utility_Rayman1_TPLS_RaymanVersion.Ray_1_21_Chinese => "1.21_Chinese",
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        try
        {
            lock (this) // TODO: Doing lock on 'this' is not a good idea - change this
            {
                File.WriteAllLines(ConfigFilePath, new[]
                {
                    "[rayman]",
                    $"gameversion={GetVersionName(RaymanVersion)}",
                    "musicfile=Music.dat"
                });
            }

            Logger.Info("Updated TPLS config with version {0}", RaymanVersion);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Updating TPLS config");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.TPLS_UpdateVersionError);
        }
    }

    #endregion
}