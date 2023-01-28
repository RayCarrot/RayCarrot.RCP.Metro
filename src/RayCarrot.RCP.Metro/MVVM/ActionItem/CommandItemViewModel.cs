using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

public abstract class CommandItemViewModel : ActionItemViewModel
{
    #region Constructor

    /// <summary>
    /// Creates a new command item
    /// </summary>
    /// <param name="header">The item header</param>
    /// <param name="description">The item description or info</param>
    /// <param name="command">The item command</param>
    /// <param name="minUserLevel">The minimum user level for the action</param>
    protected CommandItemViewModel(
        LocalizedString header,
        LocalizedString? description, 
        ICommand command, 
        UserLevel minUserLevel = UserLevel.Normal) 
        : base(minUserLevel)
    {
        Header = header;
        Description = description;
        Command = command;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The item header
    /// </summary>
    public LocalizedString Header { get; }

    /// <summary>
    /// The item description or info
    /// </summary>
    public LocalizedString? Description { get; }

    /// <summary>
    /// The item command
    /// </summary>
    public ICommand Command { get; }

    #endregion
}