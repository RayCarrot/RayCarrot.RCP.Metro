using System.Windows;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

/// <summary>
/// Interaction logic for ModLoaderDialog.xaml
/// </summary>
public partial class ModLoaderDialog : WindowContentControl
{
    #region Constructor
    
    public ModLoaderDialog(ModLoaderViewModel viewModel)
    {
        DataContext = viewModel;
        ViewModel = viewModel;

        Loaded += ModLoaderDialog_OnLoaded;

        // Set up UI
        InitializeComponent();
    }

    #endregion

    #region Private Fields

    private bool _forceClose;

    #endregion

    #region Public Properties

    public override bool IsResizable => true;
    public ModLoaderViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        // TODO-UPDATE: Localize
        WindowInstance.Title = String.Format("Mod Loader - {0}", ViewModel.GameInstallation.GetDisplayName());
        WindowInstance.Icon = GenericIconKind.Window_ModLoader;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 950;
        WindowInstance.Height = 650;
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

    private async void ModLoaderDialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ModLoaderDialog_OnLoaded;

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

    public override void Dispose()
    {
        base.Dispose();
        ViewModel.Dispose();
    }

    #endregion
}