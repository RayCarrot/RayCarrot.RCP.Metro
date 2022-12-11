using System.Diagnostics;
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Contains general user data for the application
/// </summary>
[DebuggerDisplay("UL = {CurrentUserLevel}, Culture = {CurrentCulture}")]
public class AppInstanceData : IAppInstanceData
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public AppInstanceData()
    {
        CurrentUserLevel = UserLevel.Normal;
        CurrentCulture = CultureInfo.DefaultThreadCurrentCulture;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private UserLevel _currentUserLevel;

    private CultureInfo? _currentCulture;

    #endregion

    #region Public Properties

    /// <summary>
    /// The current user level in the framework
    /// </summary>
    public UserLevel CurrentUserLevel
    {
        get => _currentUserLevel;
        set
        {
            if (_currentUserLevel == value)
                return;

            // Save the old value
            var oldValue = _currentUserLevel;

            // Set the field
            _currentUserLevel = value;

            // Log the change
            Logger.Info("The user level has changed from {0} to {1}", oldValue, CurrentUserLevel);

            // Fire event
            UserLevelChanged(this, new PropertyChangedEventArgs<UserLevel>(oldValue, CurrentUserLevel));
        }
    }

    /// <summary>
    /// The current culture in the framework
    /// </summary>
    public CultureInfo? CurrentCulture
    {
        get => _currentCulture;
        set
        {
            if (_currentCulture?.Equals(value) ?? false)
                return;

            // Save the old value
            var oldValue = _currentCulture;

            // Set the field
            _currentCulture = value;

            // Log the change
            Logger.Info("The current culture has changed from {0} to {1}", oldValue, CurrentCulture);

            // Fire event
            CultureChanged(this, new PropertyChangedEventArgs<CultureInfo>(oldValue, CurrentCulture));
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the <see cref="CurrentUserLevel"/> changes
    /// </summary>
    public event PropertyChangedEventHandler<UserLevel> UserLevelChanged = delegate { };

    /// <summary>
    /// Occurs when the <see cref="CurrentCulture"/> changes
    /// </summary>
    public event PropertyChangedEventHandler<CultureInfo> CultureChanged = delegate { };

    #endregion
}