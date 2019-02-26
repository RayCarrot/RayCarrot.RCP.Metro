using RayCarrot.CarrotFramework;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a help item
    /// </summary>
    public class HelpItemViewModel : BaseViewModel
    {
        /// <summary>
        /// The display header
        /// </summary>
        public string DisplayHeader { get; set; }

        /// <summary>
        /// The help text
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// True if there are sub items, false if not
        /// </summary>
        public bool HasSubItems => SubItems != null;

        /// <summary>
        /// The sub items
        /// </summary>
        public ObservableCollection<HelpItemViewModel> SubItems { get; set; }

        /// <summary>
        /// The required <see cref="UserLevel"/>
        /// </summary>
        public UserLevel RequiredUserLevel { get; set; } = UserLevel.Normal;
    }
}