namespace RayCarrot.RCP.Metro;

/// <summary>
/// A dialog message box with standard WPF controls
/// </summary>
public partial class DialogMessageBox : WindowContentControl, IDialogWindowControl<DialogMessageViewModel, UserInputResult>
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
    protected UserInputResult DialogResult { get; set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The view model
    /// </summary>
    public DialogMessageViewModel ViewModel => DataContext as DialogMessageViewModel;

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Icon = GenericIconKind.Window_DialogMessage;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current result
    /// </summary>
    /// <returns>The result</returns>
    public UserInputResult GetResult() => DialogResult;

    #endregion

    #region Event Handler

    private void DialogAction_ActionHandled(object sender, DialogMessageActionHandledEventArgs e)
    {
        // Set the result
        DialogResult = e.ActionResult;
            
        // Close if set to do so
        if (e.ShouldCloseDialog)
            WindowInstance.Close();
    }

    #endregion
}