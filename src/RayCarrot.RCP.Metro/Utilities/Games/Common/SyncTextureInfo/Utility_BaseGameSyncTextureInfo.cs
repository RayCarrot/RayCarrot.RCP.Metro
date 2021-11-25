#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base game sync texture info utility
/// </summary>
public abstract class Utility_BaseGameSyncTextureInfo : IUtility
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="viewModel">The view model</param>
    protected Utility_BaseGameSyncTextureInfo(Utility_BaseGameSyncTextureInfo_ViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    #endregion

    #region Interface Members

    /// <summary>
    /// The header for the utility. This property is retrieved again when the current culture is changed.
    /// </summary>
    public string DisplayHeader => Resources.Utilities_SyncTextureInfo_Header;

    /// <summary>
    /// The utility information text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public string InfoText => Resources.Utilities_SyncTextureInfo_Info;

    /// <summary>
    /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public string WarningText => null;

    /// <summary>
    /// Indicates if the utility requires additional files to be downloaded remotely
    /// </summary>
    public bool RequiresAdditionalFiles => false;

    /// <summary>
    /// Indicates if the utility is work in process
    /// </summary>
    public bool IsWorkInProcess => false;

    /// <summary>
    /// The utility UI content
    /// </summary>
    public object UIContent => new Utility_BaseGameSyncTextureInfo_UI()
    {
        DataContext = ViewModel
    };

    /// <summary>
    /// Indicates if the utility requires administration privileges
    /// </summary>
    public bool RequiresAdmin => false;

    /// <summary>
    /// Indicates if the utility is available to the user
    /// </summary>
    public bool IsAvailable => true;

    /// <summary>
    /// Retrieves a list of applied utilities from this utility
    /// </summary>
    /// <returns>The applied utilities</returns>
    public IEnumerable<string> GetAppliedUtilities() => new string[0];

    #endregion

    #region Public Properties

    /// <summary>
    /// The view model
    /// </summary>
    public Utility_BaseGameSyncTextureInfo_ViewModel ViewModel { get; }

    #endregion
}