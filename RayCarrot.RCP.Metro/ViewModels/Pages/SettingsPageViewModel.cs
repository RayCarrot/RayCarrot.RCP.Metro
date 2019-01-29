using System.Windows.Input;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the settings page
    /// </summary>
    public class SettingsPageViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsPageViewModel()
        {
            ResetCommand = new RelayCommand(RCFRCP.App.ResetData);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Shortcut to the app user data
        /// </summary>
        public AppUserData AppUserData => RCFRCP.Data;

        #endregion

        #region Commands

        public ICommand ResetCommand { get; }

        #endregion
    }
}