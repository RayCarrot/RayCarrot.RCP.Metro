#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends debug commands utility
/// </summary>
public class Utility_RaymanLegends_DebugCommands : IUtility
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanLegends_DebugCommands()
    {
        ViewModel = new Utility_RaymanLegends_DebugCommands_ViewModel();
    }

    #endregion

    #region Interface Members

    /// <summary>
    /// The header for the utility. This property is retrieved again when the current culture is changed.
    /// </summary>
    public string DisplayHeader => Resources.ROU_DebugCommandsHeader;

    public GenericIconKind Icon => GenericIconKind.Utilities_RaymanLegends_DebugCommands;

    /// <summary>
    /// The utility information text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public string InfoText => Resources.ROU_DebugCommandsInfo;

    /// <summary>
    /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public string WarningText => Resources.ROU_DebugCommandsWarning;

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
    public object UIContent => new Utility_RaymanLegends_DebugCommands_UI()
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
    public bool IsAvailable => !(ViewModel.GameFilePath.Parent + "steam_api.dll").FileExists;

    /// <summary>
    /// Retrieves a list of applied utilities from this utility
    /// </summary>
    /// <returns>The applied utilities</returns>
    public IEnumerable<string> GetAppliedUtilities()
    {
        return new string[0];
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The view model
    /// </summary>
    public Utility_RaymanLegends_DebugCommands_ViewModel ViewModel { get; }

    #endregion
}