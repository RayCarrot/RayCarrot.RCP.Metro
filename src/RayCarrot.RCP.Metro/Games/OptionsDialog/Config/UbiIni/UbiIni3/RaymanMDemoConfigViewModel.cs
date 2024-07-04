#nullable disable
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman M demo configuration
/// </summary>
public class RaymanMDemoConfigViewModel : RaymanMConfigViewModel
{
    #region Constructor

    public RaymanMDemoConfigViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    #endregion

    #region Protected Override Properties

    /// <summary>
    /// The available game patches
    /// </summary>
    protected override FilePatcher_Patch[] Patches => null;

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Loads the <see cref="UbiIniBaseConfigViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The config data</returns>
    protected override RaymanMIniAppData CreateConfig()
    {
        // Load the configuration data
        return new RaymanMIniAppData(AppFilePaths.UbiIniPath1, isDemo: true);
    }

    #endregion
}