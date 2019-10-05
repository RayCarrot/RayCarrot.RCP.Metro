using System;
using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a RCP utility
    /// </summary>
    public class RCPUtilityViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="utility">The utility</param>
        public RCPUtilityViewModel(IRCPUtility utility)
        {
            Utility = utility;
            RCFCore.Data.CultureChanged += Data_CultureChanged;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the utility can not be run due to requiring the app to run as administrator
        /// </summary>
        public bool RequiresAdmin => !App.IsRunningAsAdmin && Utility.RequiresAdmin;

        /// <summary>
        /// The utility header
        /// </summary>
        public string DisplayHeader => Utility.DisplayHeader;

        /// <summary>
        /// The utility info
        /// </summary>
        public string InfoText => Utility.InfoText;

        /// <summary>
        /// The utility warning
        /// </summary>
        public string WarningText => Utility.WarningText;

        /// <summary>
        /// The utility
        /// </summary>
        public IRCPUtility Utility { get; }

        #endregion

        #region Event Handlers

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<System.Globalization.CultureInfo> e)
        {
            OnPropertyChanged(nameof(DisplayHeader));
            OnPropertyChanged(nameof(InfoText));
            OnPropertyChanged(nameof(WarningText));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            RCFCore.Data.CultureChanged -= Data_CultureChanged;
        }

        #endregion
    }
}