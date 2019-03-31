using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for an item in a <see cref="OverflowButton"/>
    /// </summary>
    public class OverflowButtonItemViewModel : ActionItemViewModel
    {
        #region Constructors

        /// <summary>
        /// Constructor for a separator
        /// </summary>
        /// <param name="minUserLevel">The minimum user level for the action</param>
        public OverflowButtonItemViewModel(UserLevel minUserLevel = UserLevel.Normal) : base(null, null, null, minUserLevel)
        {
            IsSeparator = true;
        }

        /// <summary>
        /// Default constructor for an action
        /// </summary>
        /// <param name="header">The item header</param>
        /// <param name="iconKind">The item icon kind</param>
        /// <param name="command">The item command</param>
        /// <param name="minUserLevel">The minimum user level for the action</param>
        public OverflowButtonItemViewModel(string header, PackIconMaterialKind iconKind, ICommand command, UserLevel minUserLevel = UserLevel.Normal) : base(header, iconKind, command, minUserLevel)
        {
        }

        /// <summary>
        /// Default constructor for an action
        /// </summary>
        /// <param name="header">The item header</param>
        /// <param name="iconSource">The icon source</param>
        /// <param name="command">The item command</param>
        /// <param name="minUserLevel">The minimum user level for the action</param>
        public OverflowButtonItemViewModel(string header, ImageSource iconSource, ICommand command, UserLevel minUserLevel = UserLevel.Normal) : base(header, iconSource, command, minUserLevel)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the item is a separator
        /// </summary>
        public bool IsSeparator { get; }

        #endregion
    }
}