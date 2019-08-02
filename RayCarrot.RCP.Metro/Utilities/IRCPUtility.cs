using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    //TODO: Finish this

    /*
     
        A utility in the Rayman Control Panel is defined by the RayCarrot.RCP.Metro.IRCPUtility interface in the API. The utility, deriving from
        the interface, must correctly implement each member. All properties are read-only and should not be cached during construction. Instead
        whenever the property is retrieved it should re-calculate the value as it may change depending on the current culture or available game.
        
        A utility is primarily made for a single game. To make it available for several games it needs to have several classes deriving from the
        same class, each with a different game which is retrieved from the Game property. These classes should not use the same files as they may
        run simultaneously.

        The UIElement should be a native WPF UIElement. No fixed size or ScrollViewer should be used. The content should be put in a StackPanel
        where the individual items wrap to avoid overflow.

        The ID for the utility should not be more than 99 characters. Ideally it should be a GUID, although it is not required as long as it is unique.

        The utility has to reference the Rayman Control Panel API. Referencing the Carrot Framework is not recommended due to the API handling the
        important framework services, such as logging and dependency injection. The utility will not load if it references the Rayman Control Panel directly.
        
         
         */

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

        // TODO: Show in UI
        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        bool RequiresAdmin { get; }

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        bool IsAvailable { get; }

        // TODO: Show in UI
        /// <summary>
        /// The developers of the utility
        /// </summary>
        string Developers { get; }

        // TODO: Show in UI
        /// <summary>
        /// Any additional developers to credit for the utility
        /// </summary>
        string AdditionalDevelopers { get; }

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        IEnumerable<string> GetAppliedUtilities();
    }
}