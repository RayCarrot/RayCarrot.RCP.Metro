using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interface for a RCP utility
    /// </summary>
    public interface IRCPUtility
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
        /// The utility UI content
        /// </summary>
        UIElement UIContent { get; }

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
}