using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// Interaction logic for PatchCreatorDialog.xaml
/// </summary>
public partial class PatchCreatorDialog : WindowContentControl
{
    #region Constructor
    
    public PatchCreatorDialog(PatchCreatorViewModel viewModel)
    {
        DataContext = viewModel;
        ViewModel = viewModel;

        // Set up UI
        InitializeComponent();

        Loaded += PatchCreatorUI_Loaded;
    }

    #endregion

    #region Private Fields

    private bool _forceClose;

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    public PatchCreatorViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = Metro.Resources.PatchCreator_Title;
        WindowInstance.Icon = GenericIconKind.Window_PatchCreator;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 900;
        WindowInstance.Height = 600;
    }

    protected override async Task<bool> ClosingAsync()
    {
        if (!await base.ClosingAsync())
            return false;

        if (_forceClose)
            return true;

        // Cancel the closing if it's loading
        if (ViewModel.LoaderViewModel.IsRunning)
            return false;

        // Ask user to confirm
        return await Services.MessageUI.DisplayMessageAsync(Metro.Resources.UnsavedChangesQuestion,
            Metro.Resources.UnsavedChangesQuestionHeader, MessageType.Question, true);
    }

    #endregion

    #region Event Handlers

    private void PatchCreatorUI_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PatchCreatorUI_Loaded;

        // Disable the closing button when loading
        ViewModel.LoaderViewModel.IsRunningChanged += (_, _) =>
            WindowInstance.CanClose = !ViewModel.LoaderViewModel.IsRunning;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    private async void CreateButton_OnClick(object sender, RoutedEventArgs e)
    {
        bool result = await ViewModel.CreatePatchAsync();
        
        if (!result)
            return;

        _forceClose = true;
        WindowInstance.Close();
    }

    #endregion
}