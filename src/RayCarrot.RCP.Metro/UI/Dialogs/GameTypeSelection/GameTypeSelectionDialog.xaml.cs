using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameTypeSelectionDialog.xaml
/// </summary>
public partial class GameTypeSelectionDialog : WindowContentControl, IDialogWindowControl<GameTypeSelectionViewModel, GameTypeSelectionResult>
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

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Icon = GenericIconKind.Window_GameTypeSelection;
    }

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