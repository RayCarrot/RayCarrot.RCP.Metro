using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

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
        public OverflowButtonItemViewModel() : base(null, null, null)
        {
            IsSeparator = true;
        }

        /// <summary>
        /// Default constructor for an action
        /// </summary>
        /// <param name="header">The item header</param>
        /// <param name="iconKind">The item icon kind</param>
        /// <param name="command">The item command</param>
        public OverflowButtonItemViewModel(string header, PackIconMaterialKind iconKind, ICommand command) : base(header, iconKind, command)
        {
        }

        /// <summary>
        /// Default constructor for an action
        /// </summary>
        /// <param name="header">The item header</param>
        /// <param name="iconSource">The icon source</param>
        /// <param name="command">The item command</param>
        public OverflowButtonItemViewModel(string header, ImageSource iconSource, ICommand command) : base(header, iconSource, command)
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