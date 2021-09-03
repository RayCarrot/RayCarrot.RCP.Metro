using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a action item
    /// </summary>
    public class ActionItemViewModel : BaseRCPViewModel
    {
        #region Constructors

        /// <summary>
        /// Default constructor for an action
        /// </summary>
        /// <param name="header">The item header</param>
        /// <param name="iconKind">The item icon kind</param>
        /// <param name="command">The item command</param>
        /// <param name="minUserLevel">The minimum user level for the action</param>
        public ActionItemViewModel(string header, PackIconMaterialKind iconKind, ICommand command, UserLevel minUserLevel = UserLevel.Normal)
        {
            Header = header;
            IconKind = iconKind;
            Command = command;
            MinUserLevel = minUserLevel;
        }

        /// <summary>
        /// Default constructor for an action
        /// </summary>
        /// <param name="header">The item header</param>
        /// <param name="iconSource">The icon source</param>
        /// <param name="command">The item command</param>
        /// <param name="minUserLevel">The minimum user level for the action</param>
        public ActionItemViewModel(string header, ImageSource iconSource, ICommand command, UserLevel minUserLevel = UserLevel.Normal)
        {
            iconSource?.Freeze();

            Header = header;
            IconSource = iconSource;
            Command = command;
            MinUserLevel = minUserLevel;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The item header
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// The item icon kind
        /// </summary>
        public PackIconMaterialKind IconKind { get; }

        /// <summary>
        /// The icon source
        /// </summary>
        public ImageSource IconSource { get; }

        /// <summary>
        /// The item command
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// The minimum user level for the action
        /// </summary>
        public UserLevel MinUserLevel { get; }

        #endregion
    }
}