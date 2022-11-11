#nullable disable
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for an item in a <see cref="OverflowButton"/>
/// </summary>
public class OverflowButtonItemViewModel : CommandItemViewModel
{
    #region Constructors

    /// <summary>
    /// Constructor for a separator
    /// </summary>
    /// <param name="minUserLevel">The minimum user level for the action</param>
    public OverflowButtonItemViewModel(UserLevel minUserLevel = UserLevel.Normal) : base(null, null, minUserLevel)
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
    public OverflowButtonItemViewModel(string header, GenericIconKind iconKind, ICommand command, UserLevel minUserLevel = UserLevel.Normal) : base(header, command, minUserLevel)
    {
        IconKind = iconKind;
    }

    /// <summary>
    /// Default constructor for an action
    /// </summary>
    /// <param name="header">The item header</param>
    /// <param name="iconSource">The icon source</param>
    /// <param name="command">The item command</param>
    /// <param name="minUserLevel">The minimum user level for the action</param>
    public OverflowButtonItemViewModel(string header, ImageSource iconSource, ICommand command, UserLevel minUserLevel = UserLevel.Normal) : base(header, command, minUserLevel)
    {
        IconSource = iconSource;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the item is a separator
    /// </summary>
    public bool IsSeparator { get; }

    public GenericIconKind IconKind { get; }
    public ImageSource IconSource { get; }

    #endregion
}