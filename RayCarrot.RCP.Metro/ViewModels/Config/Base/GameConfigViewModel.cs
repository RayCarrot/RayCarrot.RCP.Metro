using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game configuration
    /// </summary>
    public abstract class GameConfigViewModel : BaseRCPViewModel
    {
        /// <summary>
        /// Indicates if there are any unsaved changes
        /// </summary>
        public bool UnsavedChanges { get; set; }

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public abstract Task SetupAsync();

        /// <summary>
        /// Action to run upon saving
        /// </summary>
        public Action OnSave { get; set; }

        public bool IsAvailable { get; set; }
    }
}