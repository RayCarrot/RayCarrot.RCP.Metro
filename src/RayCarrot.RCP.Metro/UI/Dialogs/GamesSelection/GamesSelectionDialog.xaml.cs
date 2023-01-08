using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GamesSelectionDialog.xaml
/// </summary>
public partial class GamesSelectionDialog : WindowContentControl, IDialogWindowControl<GamesSelectionViewModel, GamesSelectionResult>
{ 
    #region Constructor

    public GamesSelectionDialog(GamesSelectionViewModel vm)
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
    public GamesSelectionViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Icon = GenericIconKind.Window_GamesSelection;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current result for the dialog
    /// </summary>
    /// <returns>The result</returns>
    public GamesSelectionResult GetResult()
    {
        return new GamesSelectionResult()
        {
            CanceledByUser = CanceledByUser,
            SelectedGame = ViewModel.SelectedGames.FirstOrDefault()?.GameInstallation,
            SelectedGames = ViewModel.SelectedGames.Select(x => x.GameInstallation).ToList(),
        };
    }

    #endregion

    #region Event Handlers

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasSelection)
            return;

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