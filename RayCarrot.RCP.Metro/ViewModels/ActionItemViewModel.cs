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
        public ActionItemViewModel(string header, PackIconMaterialKind iconKind, ICommand command)
        {
            Header = header;
            IconKind = iconKind;
            Command = command;
        }

        /// <summary>
        /// Default constructor for an action
        /// </summary>
        /// <param name="header">The item header</param>
        /// <param name="iconSource">The icon source</param>
        /// <param name="command">The item command</param>
        public ActionItemViewModel(string header, ImageSource iconSource, ICommand command)
        {
            Header = header;
            IconSource = iconSource;
            Command = command;
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

        #endregion
    }
}