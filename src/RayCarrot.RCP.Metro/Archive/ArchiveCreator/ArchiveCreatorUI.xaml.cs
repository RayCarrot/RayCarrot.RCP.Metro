#nullable disable
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for ArchiveCreatorUI.xaml
/// </summary>
public partial class ArchiveCreatorUI : WindowContentControl
{
    #region Constructor
        
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="vm">The view model</param>
    public ArchiveCreatorUI(ArchiveCreatorDialogViewModel vm)
    {
        // Set up UI
        InitializeComponent();

        // Set properties
        ViewModel = vm;
        DataContext = ViewModel;

        // Set up remaining things once fully loaded
        Loaded += ArchiveExplorer_Loaded;
    }

    #endregion
        
    #region Public Properties

    /// <summary>
    /// The view model
    /// </summary>
    public ArchiveCreatorDialogViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = ViewModel.Title;
        WindowInstance.Icon = GenericIconKind.Window_ArchiveCreator;
    }

    protected override async Task<bool> ClosingAsync()
    {
        if (!await base.ClosingAsync())
            return false;

        // Cancel the closing if an archive is running an operation
        return !ViewModel.IsLoading;
    }

    #endregion

    #region Event Handlers

    private void ArchiveExplorer_Loaded(object sender, RoutedEventArgs e)
    {
        // Make sure this won't get called again
        Loaded -= ArchiveExplorer_Loaded;

        ViewModel.PropertyChanged += (_, ee) =>
        {
            // Disable the closing button when loading
            if (ee.PropertyName == nameof(ArchiveExplorerDialogViewModel.IsLoading))
                WindowInstance.CanClose = !ViewModel.IsLoading;
        };
    }

    private async void OKButton_ClickAsync(object sender, RoutedEventArgs e)
    {
        if (!await ViewModel.CreateArchiveAsync())
            return;

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