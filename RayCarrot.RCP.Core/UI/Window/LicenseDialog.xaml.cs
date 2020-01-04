using System.Windows;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Interaction logic for LicenseDialog.xaml
    /// </summary>
    public partial class LicenseDialog : BaseWindow
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LicenseDialog()
        {
            InitializeComponent();
            Accepted = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the license has been accepted by the user
        /// </summary>
        public bool Accepted { get; private set; }

        #endregion

        #region Event Handlers

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            Close();
        }

        private void DoNotAccept_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            Close();
        }

        #endregion
    }
}