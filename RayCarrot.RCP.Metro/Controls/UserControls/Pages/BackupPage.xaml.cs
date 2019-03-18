using RayCarrot.WPF;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for BackupPage.xaml
    /// </summary>
    public partial class BackupPage : VMUserControl<BackupPageViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BackupPage()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Indicates if the backups have been refreshed
        /// </summary>
        private bool HasRefreshed { get; set; }

        #endregion

        #region Event Handlers

        private void BackupPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!HasRefreshed)
                // Refresh the backups
                ViewModel?.RefresCommand.Execute(null);

            // Scroll up the scroll viewer
            BackupPageScrollViewer.ScrollToTop();

            HasRefreshed = true;
        }

        #endregion
    }
}