using System.Globalization;

namespace RayCarrot.RCP.Metro;

// TODO: Either remove need for this or include other runtime data here

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
    CultureInfo? CurrentCulture { get; set; }

    /// <summary>
    /// Occurs when the <see cref="CurrentUserLevel"/> changes
    /// </summary>
    event PropertyChangedEventHandler<UserLevel> UserLevelChanged;

    /// <summary>
    /// Occurs when the <see cref="CurrentCulture"/> changes
    /// </summary>
    event PropertyChangedEventHandler<CultureInfo> CultureChanged;
}