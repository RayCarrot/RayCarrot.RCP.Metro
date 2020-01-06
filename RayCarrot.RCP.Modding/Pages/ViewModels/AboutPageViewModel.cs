using RayCarrot.RCP.Core;
using RayCarrot.UI;
using System.Windows.Input;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// View model for the about page
    /// </summary>
    public class AboutPageViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AboutPageViewModel()
        {
            // Create commands
            OpenUrlCommand = new RelayCommand(x => RCFRCPM.App.OpenUrl(x?.ToString()));
            CheckForUpdatesCommand = new AsyncRelayCommand(async () => await RCFRCPM.App.CheckForUpdatesAsync(true));

            // Refresh the update badge property based on if new update is available
            RCFRCPC.Data.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(RCFRCPC.Data.IsUpdateAvailable))
                    OnPropertyChanged(nameof(UpdateBadge));
            };
        }

        #endregion

        #region Commands

        public ICommand OpenUrlCommand { get; }

        public ICommand CheckForUpdatesCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The update badge, indicating if new updates are available
        /// </summary>
        public string UpdateBadge => RCFRCPC.Data.IsUpdateAvailable ? "1" : null;

        #endregion
    }
}