using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

public class ImageCommandItemViewModel : CommandItemViewModel
{
    #region Constructor

    /// <summary>
    /// Creates a new command item using an <see cref="ImageSource"/>
    /// </summary>
    /// <param name="header">The item header</param>
    /// <param name="description">The item description or info</param>
    /// <param name="imageSource">The item image source</param>
    /// <param name="command">The item command</param>
    /// <param name="minUserLevel">The minimum user level for the action</param>
    public ImageCommandItemViewModel(
        LocalizedString header,
        LocalizedString? description, 
        ImageSource imageSource, 
        ICommand command, 
        UserLevel minUserLevel = UserLevel.Normal) 
        : base(header, description, command, minUserLevel)
    {
        ImageSource = imageSource;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The item image source
    /// </summary>
    public ImageSource ImageSource { get; }

    #endregion
}