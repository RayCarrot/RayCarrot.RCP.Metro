using System.Windows;

namespace RayCarrot.RCP.Metro.Patcher.Dialogs.Patcher;

/// <summary>
/// Interaction logic for PatcherDialog.xaml
/// </summary>
public partial class PatcherDialog : WindowContentControl
{
    #region Constructor
    
    public PatcherDialog(PatcherViewModel viewModel)
    {
        DataContext = viewModel;
        ViewModel = viewModel;

        Loaded += PatcherUI_OnLoaded;

        // Set up UI
        InitializeComponent();
    }

    #endregion

    #region Private Fields

    private bool _forceClose;

    #endregion

    #region Public Properties

    public override bool IsResizable => true;
    public PatcherViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = String.Format(Metro.Resources.Patcher_DialogTitle, ViewModel.GameInstallation.GetDisplayName());
        WindowInstance.Icon = GenericIconKind.Window_Patcher;
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

        // Ask user if there are pending changes
        if (ViewModel.HasChanges)
            return await Services.MessageUI.DisplayMessageAsync(Metro.Resources.UnsavedChangesQuestion,
                Metro.Resources.UnsavedChangesQuestionHeader, MessageType.Question, true);
        else
            return true;
    }

    #endregion

    #region Event Handlers

    private async void PatcherUI_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PatcherUI_OnLoaded;

        // Disable the closing button when loading
        ViewModel.LoaderViewModel.IsRunningChanged += (_, _) =>
            WindowInstance.CanClose = !ViewModel.LoaderViewModel.IsRunning;

        // Initialize
        bool success = await ViewModel.InitializeAsync();

        if (!success)
            ForceClose();
    }

    #endregion

    #region Public Methods

    public void Close()
    {
        WindowInstance.Close();
    }
    
    public void ForceClose()
    {
        _forceClose = true;
        WindowInstance.Close();
    }

    #endregion
}