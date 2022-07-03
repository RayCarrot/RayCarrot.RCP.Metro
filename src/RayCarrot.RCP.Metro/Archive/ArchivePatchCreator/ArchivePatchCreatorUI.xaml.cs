using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// Interaction logic for ArchivePatchCreatorUI.xaml
/// </summary>
public partial class ArchivePatchCreatorUI : WindowContentControl
{
    #region Constructor
    
    public ArchivePatchCreatorUI(ArchivePatchCreatorViewModel viewModel, FileSystemPath? existingPatch)
    {
        DataContext = viewModel;
        ViewModel = viewModel;

        _patchToImportFrom = existingPatch;

        // Set up UI
        InitializeComponent();

        Loaded += ArchivePatchCreatorUI_Loaded;
    }

    private async void ArchivePatchCreatorUI_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ArchivePatchCreatorUI_Loaded;

        if (_patchToImportFrom == null)
            return;

        bool success = await ViewModel.ImportFromPatchAsync(_patchToImportFrom.Value);

        if (!success)
            WindowInstance.Close();
    }

    #endregion

    #region Private Fields

    private readonly FileSystemPath? _patchToImportFrom;

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    public ArchivePatchCreatorViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Archive Patch Creator"; // TODO-UPDATE: Localize
        WindowInstance.Icon = GenericIconKind.Window_ArchivePatcher; // TODO-UPDATE: Change icon
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 900;
        WindowInstance.Height = 600;
    }

    protected override async Task<bool> ClosingAsync()
    {
        if (!await base.ClosingAsync())
            return false;

        // Cancel the closing if it's loading
        return !ViewModel.IsLoading;
    }

    #endregion

    #region Public Methods

    public override void Dispose()
    {
        base.Dispose();

        DataContext = null;
        ViewModel.Dispose();
    }

    #endregion

    #region Event Handlers

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    private async void CreateButton_OnClick(object sender, RoutedEventArgs e)
    {
        bool result = await ViewModel.CreatePatchAsync();
        
        if (!result)
            return;
        
        WindowInstance.Close();
    }

    #endregion
}