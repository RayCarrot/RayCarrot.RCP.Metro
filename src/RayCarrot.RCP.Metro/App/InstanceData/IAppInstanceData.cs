#nullable disable
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Used for containing general user data for the application
/// </summary>
public interface IAppInstanceData
{
    /// <summary>
    /// The current user level in the framework
    /// </summary>
    UserLevel CurrentUserLevel { get; set; }

    /// <summary>
    /// The current culture in the framework
    /// </summary>
    CultureInfo CurrentCulture { get; set; }

    /// <summary>
    /// The launch arguments for the current application
    /// </summary>
    string[] Arguments { get; set; }

    /// <summary>
    /// Occurs when the <see cref="CurrentUserLevel"/> changes
    /// </summary>
    event PropertyChangedEventHandler<UserLevel> UserLevelChanged;

    /// <summary>
    /// Occurs when the <see cref="CurrentCulture"/> changes
    /// </summary>
    event PropertyChangedEventHandler<CultureInfo> CultureChanged;
}