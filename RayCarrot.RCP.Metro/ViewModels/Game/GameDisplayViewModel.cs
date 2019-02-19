using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game display
    /// </summary>
    public class GameDisplayViewModel : BaseRCPViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="displayName">The display name</param>
        /// <param name="iconSource">The icon source</param>
        /// <param name="mainAction">The main action</param>
        /// <param name="launchActions">The launch actions</param>
        public GameDisplayViewModel(string displayName, string iconSource, ActionItemViewModel mainAction, IEnumerable<OverflowButtonItemViewModel> launchActions)
        {
            DisplayName = displayName;
            IconSource = iconSource;
            LaunchActions = launchActions ?? new OverflowButtonItemViewModel[0];
            MainAction = mainAction;
        }

        /// <summary>
        /// The main actions
        /// </summary>
        public ActionItemViewModel MainAction { get; }

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The icons source
        /// </summary>
        public string IconSource { get; }

        /// <summary>
        /// The launch actions
        /// </summary>
        public IEnumerable<OverflowButtonItemViewModel> LaunchActions { get; }
    }
}