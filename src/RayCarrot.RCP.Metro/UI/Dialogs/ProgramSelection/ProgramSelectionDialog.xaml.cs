using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for StringInputDialog.xaml
/// </summary>
public partial class ProgramSelectionDialog : WindowContentControl, IDialogWindowControl<ProgramSelectionViewModel, ProgramSelectionResult>
{ 
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="vm">The view model</param>
    public ProgramSelectionDialog(ProgramSelectionViewModel vm)
    {
        InitializeComponent();
        ViewModel = vm;
        DataContext = ViewModel;
        CanceledByUser = true;
    }

    #endregion

    #region Private Fields

    private bool _hasLoaded;

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the dialog was canceled by the user, default is true
    /// </summary>
    public bool CanceledByUser { get; set; }

    /// <summary>
    /// The view model
    /// </summary>
    public ProgramSelectionViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Icon = GenericIconKind.Window_ProgramSelection;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current result for the dialog
    /// </summary>
    /// <returns>The result</returns>
    public ProgramSelectionResult GetResult()
    {
        return new ProgramSelectionResult()
        {
            CanceledByUser = CanceledByUser,
            ProgramFilePath = ViewModel.ProgramFilePath
        };
    }

    #endregion

    #region Event Handlers

    private async void ProgramSelectionDialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_hasLoaded)
            return;

        _hasLoaded = true;

        await ViewModel.LoadProgramsAsync();
    }

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