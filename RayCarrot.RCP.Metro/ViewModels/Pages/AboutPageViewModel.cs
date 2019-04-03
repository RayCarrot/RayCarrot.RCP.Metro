using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using RayCarrot.CarrotFramework;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the about page
    /// </summary>
    public class AboutPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AboutPageViewModel()
        {
            // Create commands
            OpenUrlCommand = new RelayCommand(x => OpenUrl(x?.ToString()));
            ShowVersionHistoryCommand = new RelayCommand(ShowVersionHistory);
            CheckForUpdatesCommand = new AsyncRelayCommand(async () => await App.CheckForUpdatesAsync(true));
        }

        #endregion

        #region Commands

        public ICommand OpenUrlCommand { get; }

        public ICommand ShowVersionHistoryCommand { get; }

        public ICommand CheckForUpdatesCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the specified url
        /// </summary>
        /// <param name="url">The url to open</param>
        public void OpenUrl(string url)
        {
            try
            {
                Process.Start(url)?.Dispose();
            }
            catch (Exception ex)
            {
                ex.HandleError($"Opening url {url}");
            }
        }

        /// <summary>
        /// Shows the application version history
        /// </summary>
        public void ShowVersionHistory()
        {
            WindowHelpers.ShowWindow<AppNewsDialog>();
        }

        #endregion
    }
}