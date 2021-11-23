using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines a utility
/// </summary>
public interface IUtility
{
    /// <summary>
    /// The header for the utility. This property is retrieved again when the current culture is changed.
    /// </summary>
    string DisplayHeader { get; }

    /// <summary>
    /// The utility information text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    string InfoText { get; }

    /// <summary>
    /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    string WarningText { get; }

    /// <summary>
    /// Indicates if the utility requires additional files to be downloaded remotely
    /// </summary>
    bool RequiresAdditionalFiles { get; }

    /// <summary>
    /// Indicates if the utility is work in process
    /// </summary>
    bool IsWorkInProcess { get; }

    /// <summary>
    /// The utility UI content
    /// </summary>
    object UIContent { get; }

    /// <summary>
    /// Indicates if the utility requires administration privileges
    /// </summary>
    bool RequiresAdmin { get; }

    /// <summary>
    /// Indicates if the utility is available to the user
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Retrieves a list of applied utilities from this utility
    /// </summary>
    /// <returns>The applied utilities</returns>
    IEnumerable<string> GetAppliedUtilities();
}