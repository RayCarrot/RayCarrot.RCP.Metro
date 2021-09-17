using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for StringInputDialog.xaml
    /// </summary>
    public partial class StringInputDialog : WindowContentControl, IDialogWindowControl<StringInputViewModel, StringInputResult>
    { 
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vm">The view model</param>
        public StringInputDialog(StringInputViewModel vm)
        {
            InitializeComponent();
            ViewModel = vm;
            DataContext = ViewModel;
            CanceledByUser = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the dialog was canceled by the user, default is true
        /// </summary>
        public bool CanceledByUser { get; set; }

        /// <summary>
        /// The view model
        /// </summary>
        public StringInputViewModel ViewModel { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        public StringInputResult GetResult()
        {
            return new StringInputResult()
            {
                CanceledByUser = CanceledByUser,
                StringInput = ViewModel.StringInput
            };
        }

        #endregion

        #region Event Handlers

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CanceledByUser = false;

            // Close the dialog
            WindowInstance.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the dialog
            WindowInstance.Close();
        }

        #endregion
    }
}