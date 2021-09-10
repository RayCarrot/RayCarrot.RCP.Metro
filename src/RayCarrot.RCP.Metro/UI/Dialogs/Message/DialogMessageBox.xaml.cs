using System;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A dialog message box with standard WPF controls
    /// </summary>
    public partial class DialogMessageBox : UserControl, IDialogBaseControl<DialogMessageViewModel, object>
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="DialogMessageBox"/>
        /// </summary>
        /// <param name="dialogVM">The dialog view model</param>
        public DialogMessageBox(DialogMessageViewModel dialogVM)
        {
            InitializeComponent();

            // Set the data context
            DataContext = dialogVM;

            // Reset the result
            DialogResult = ViewModel.DefaultActionResult;

            // Subscribe to events
            ViewModel.DialogActions?.ForEach(x => x.ActionHandled += DialogAction_ActionHandled);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The dialog result
        /// </summary>
        protected object DialogResult { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public DialogMessageViewModel ViewModel => DataContext as DialogMessageViewModel;

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
        public DialogBaseSize BaseSize => DialogBaseSize.Small;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current result
        /// </summary>
        /// <returns>The result</returns>
        public object GetResult() => DialogResult;

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

        #region Event Handler

        private void DialogAction_ActionHandled(object sender, DialogMessageActionHandledEventArgs e)
        {
            // Set the result
            DialogResult = e.ActionResult;
            
            // Close if set to do so
            if (e.ShouldCloseDialog)
                CloseDialog?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}