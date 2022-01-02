#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 disc patch utility
/// </summary>
public class Utility_Rayman2_DiscPatch : IUtility
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Rayman2_DiscPatch()
    {
        ViewModel = new Utility_Rayman2_DiscPatch_ViewModel();
    }

    #endregion

    #region Interface Members

    /// <summary>
    /// The header for the utility. This property is retrieved again when the current culture is changed.
    /// </summary>
    public string DisplayHeader => Resources.R2U_DiscPatchHeader;

    public GenericIconKind Icon => GenericIconKind.Utilities_Rayman2_DiscPatch;

    /// <summary>
    /// The utility information text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public string InfoText => Resources.R2U_DiscPatchHeaderInfo;

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
    public object UIContent => new Utility_Rayman2_DiscPatch_UI()
    {
        DataContext = ViewModel
    };

    /// <summary>
    /// Indicates if the utility requires administration privileges
    /// </summary>
    public bool RequiresAdmin => !Services.File.CheckDirectoryWriteAccess(ViewModel.InstallDir);

    /// <summary>
    /// Indicates if the utility is available to the user
    /// </summary>
    public bool IsAvailable => ViewModel.InstallDir.DirectoryExists;

    /// <summary>
    /// Retrieves a list of applied utilities from this utility
    /// </summary>
    /// <returns>The applied utilities</returns>
    public IEnumerable<string> GetAppliedUtilities()
    {
        // Due to the patch not being reversible without reinstalling we do not set it as an applied patch
        return new string[0];
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The view model
    /// </summary>
    public Utility_Rayman2_DiscPatch_ViewModel ViewModel { get; }

    #endregion
}