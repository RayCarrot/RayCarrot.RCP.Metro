using System;
using System.Windows;
using System.Windows.Controls;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GameTypeSelectionDialog.xaml
    /// </summary>
    public partial class GameTypeSelectionDialog : UserControl, IDialogBaseControl<GameTypeSelectionViewModel, GameTypeSelectionResult>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vm">The view model</param>
        public GameTypeSelectionDialog(GameTypeSelectionViewModel vm)
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
        public GameTypeSelectionViewModel ViewModel { get; }

        /// <summary>
        /// The dialog content
        /// </summary>
        public object UIContent => this;

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        public bool Resizable => false;

        /// <summary>
        /// The base size for the dialog
        /// </summary>
        public DialogBaseSize BaseSize => DialogBaseSize.Smallest;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        public GameTypeSelectionResult GetResult()
        {
            return new GameTypeSelectionResult()
            {
                CanceledByUser = CanceledByUser,
                SelectedType = ViewModel.SelectedType
            };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

        #endregion

        #region Event Handlers

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CanceledByUser = false;

            // Close the dialog
            CloseDialog?.Invoke(this, new EventArgs());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the dialog
            CloseDialog?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}