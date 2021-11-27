#nullable disable
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Configuration view model for Rayman 1 PC spin-off .dat archives
/// </summary>
public class Ray1PCArchiveConfigViewModel : BaseViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="settings">The settings when serializing the data</param>
    public Ray1PCArchiveConfigViewModel(Ray1Settings settings)
    {
        Settings = settings;

        switch (settings.Game)
        {
            case Ray1Game.RayEdu:
                PrimaryVersion = "EDU";
                SecondaryVersion = "EDU";
                break;

            case Ray1Game.RayKit:
                PrimaryVersion = "KIT";
                SecondaryVersion = "KIT";
                break;
        }
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The settings when serializing the data
    /// </summary>
    public Ray1Settings Settings { get; }

    /// <summary>
    /// The primary version. Usually KIT, EDU or QUI.
    /// </summary>
    public string PrimaryVersion { get; set; }

    /// <summary>
    /// The secondary version. Usually th same as the primary version or the volume, such as US1.
    /// </summary>
    public string SecondaryVersion { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Configures the archive data with the default settings for the current settings
    /// </summary>
    /// <param name="data">The archive data to configure</param>
    public void ConfigureArchiveData(Rayman1PCArchiveData data)
    {
        data.PrimaryKitHeader = PrimaryVersion;
        data.SecondaryKitHeader = SecondaryVersion;
        data.Short_0A = 256;
    }

    #endregion
}