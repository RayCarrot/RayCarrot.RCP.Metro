using System.Windows.Input;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the settings page
    /// </summary>
    public class SettingsPageViewModel : BaseRCPViewModel
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

        #region Commands

        public ICommand ResetCommand { get; }

        #endregion
    }
}