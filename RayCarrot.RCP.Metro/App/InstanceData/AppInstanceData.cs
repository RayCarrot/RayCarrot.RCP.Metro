using NLog;
using System.Diagnostics;
using System.Globalization;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Contains general user data for the application
    /// </summary>
    [DebuggerDisplay("UL = {CurrentUserLevel}, Culture = {CurrentCulture}, {Arguments.Length} args")]
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

        private CultureInfo _currentCulture;

        private string[] _arguments;

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
                Logger.Info($"The user level has changed from {oldValue} to {CurrentUserLevel}");

                // Fire event
                UserLevelChanged?.Invoke(this, new PropertyChangedEventArgs<UserLevel>(oldValue, CurrentUserLevel));
            }
        }

        /// <summary>
        /// The current culture in the framework
        /// </summary>
        public CultureInfo CurrentCulture
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
                Logger.Info($"The current culture has changed from {oldValue} to {CurrentCulture}");

                // Fire event
                CultureChanged?.Invoke(this, new PropertyChangedEventArgs<CultureInfo>(oldValue, CurrentCulture));
            }
        }

        /// <summary>
        /// The launch arguments for the current application
        /// </summary>
        public string[] Arguments
        {
            get => _arguments;
            set
            {
                if (_arguments == value)
                    return;

                // Set the field
                _arguments = value;

                // Log the change
                Logger.Info($"The program arguments have been set to \"{Arguments.JoinItems(", ")}\"");
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="CurrentUserLevel"/> changes
        /// </summary>
        public event PropertyChangedEventHandler<UserLevel> UserLevelChanged;

        /// <summary>
        /// Occurs when the <see cref="CurrentCulture"/> changes
        /// </summary>
        public event PropertyChangedEventHandler<CultureInfo> CultureChanged;

        #endregion
    }
}