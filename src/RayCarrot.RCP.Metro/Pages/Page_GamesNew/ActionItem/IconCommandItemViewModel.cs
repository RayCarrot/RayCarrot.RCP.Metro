using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

public class IconCommandItemViewModel : CommandItemViewModel
{
    #region Constructor

    /// <summary>
    /// Creates a new command item using a <see cref="GenericIcon"/>
    /// </summary>
    /// <param name="header">The item header</param>
    /// <param name="iconKind">The item icon kind</param>
    /// <param name="command">The item command</param>
    /// <param name="minUserLevel">The minimum user level for the action</param>
    public IconCommandItemViewModel(LocalizedString header, GenericIconKind iconKind, ICommand command, UserLevel minUserLevel = UserLevel.Normal) 
        : base(header, command, minUserLevel)
    {
        IconKind = iconKind;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The item icon kind
    /// </summary>
    public GenericIconKind IconKind { get; }

    #endregion
}