#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 translation utility
/// </summary>
public class Utility_Rayman2_Translation : IUtility
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Rayman2_Translation()
    {
        ViewModel = new Utility_Rayman2_Translation_ViewModel();
    }

    #endregion

    #region Interface Members

    /// <summary>
    /// The header for the utility. This property is retrieved again when the current culture is changed.
    /// </summary>
    public string DisplayHeader => Resources.R2U_TranslationsHeader;

    public GenericIconKind Icon => GenericIconKind.Utilities_Rayman2_Translation;

    /// <summary>
    /// The utility information text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public string InfoText => Resources.R2U_TranslationsInfo;

    /// <summary>
    /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public string WarningText => null;

    /// <summary>
    /// Indicates if the utility requires additional files to be downloaded remotely
    /// </summary>
    public bool RequiresAdditionalFiles => true;

    /// <summary>
    /// Indicates if the utility is work in process
    /// </summary>
    public bool IsWorkInProcess => false;

    /// <summary>
    /// The utility UI content
    /// </summary>
    public object UIContent => new Utility_Rayman2_Translation_UI()
    {
        DataContext = ViewModel
    };

    /// <summary>
    /// Indicates if the utility requires administration privileges
    /// </summary>
    public bool RequiresAdmin => !Services.File.CheckFileWriteAccess(ViewModel.GetFixSnaFilePath());

    /// <summary>
    /// Indicates if the utility is available to the user
    /// </summary>
    public bool IsAvailable => ViewModel.InstallDir.DirectoryExists && ViewModel.GetFixSnaFilePath().FileExists && ViewModel.GetTexturesCntFilePath().FileExists;

    /// <summary>
    /// Retrieves a list of applied utilities from this utility
    /// </summary>
    /// <returns>The applied utilities</returns>
    public IEnumerable<string> GetAppliedUtilities()
    {
        var translation = ViewModel.GetAppliedRayman2Translation();

        if (translation != Utility_Rayman2_Translation_ViewModel.Rayman2Translation.Original && translation != null)
            yield return Resources.R2U_TranslationsHeader;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The view model
    /// </summary>
    public Utility_Rayman2_Translation_ViewModel ViewModel { get; }

    #endregion
}