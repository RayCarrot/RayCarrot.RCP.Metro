using System;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Archive.Ray1;

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
        switch (settings.EngineVersion)
        {
            case Ray1EngineVersion.PC_Edu:
                PrimaryVersion = "EDU";
                SecondaryVersion = "EDU";
                break;

            case Ray1EngineVersion.PC_Kit:
            case Ray1EngineVersion.PC_Fan:
                PrimaryVersion = "KIT";
                SecondaryVersion = "KIT";
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Public Properties

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
    public void ConfigureArchiveData(PC_FileArchive data)
    {
        data.PrimaryKitHeader = PrimaryVersion;
        data.SecondaryKitHeader = SecondaryVersion;
        data.Ushort_0A = 256;
    }

    #endregion
}