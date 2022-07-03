using System.Windows;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// Interaction logic for ArchivePatchCreatorUI.xaml
/// </summary>
public partial class ArchivePatchCreatorUI : WindowContentControl
{
    #region Constructor
    
    public ArchivePatchCreatorUI(ArchivePatchCreatorViewModel viewModel)
    {
        DataContext = viewModel;
        ViewModel = viewModel;

        // Set up UI
        InitializeComponent();
    }

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