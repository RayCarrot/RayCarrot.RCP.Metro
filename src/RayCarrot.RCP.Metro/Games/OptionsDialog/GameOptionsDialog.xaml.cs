using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// Interaction logic for GameOptionsDialog.xaml
/// </summary>
public partial class GameOptionsDialog : WindowContentControl, IInitializable, IRecipient<RemovedGamesMessage>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation to show the options for</param>
    public GameOptionsDialog(GameInstallation gameInstallation)
    {
        // Set up UI
        InitializeComponent();

        // Create view model
        ViewModel = new GameOptionsDialogViewModel(gameInstallation);
        DataContext = ViewModel;
    }

    #endregion

    #region Private Properties

    /// <summary>
    /// Indicates if the window should be closed
    /// even though there are unsaved changes
    /// </summary>
    private bool ForceClose { get; set; }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    /// <summary>
    /// The window view model
    /// </summary>
    public GameOptionsDialogViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = ViewModel.GameInstallation.GetDisplayName();
        WindowInstance.Icon = GenericIconKind.Window_GameOptions;
        WindowInstance.MinWidth = 500;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 650;
        WindowInstance.Height = 700;
    }

    protected override async Task<bool> ClosingAsync()
    {
        if (!await base.ClosingAsync())
            return false;

        // Always allow the window to close if it's set to force close
        if (ForceClose)
            return true;

        // Cancel the closing if loading
        if (ViewModel.IsLoading)
            return false;

        // Attempt to find the first page which has unsaved changes
        var unsavedPage = ViewModel.Pages.FirstOrDefault(x => x.UnsavedChanges);

        // If no page has unsaved changes we can close
        if (unsavedPage == null)
            return true;

        // Go to the page with unsaved changes
        ViewModel.SelectedPage = unsavedPage;

        // Let the user decide if we should close and thus ignore the unsaved changes
        return await Services.MessageUI.DisplayMessageAsync(Metro.Resources.GameOptions_UnsavedChanges, Metro.Resources.GameOptions_UnsavedChangesHeader, MessageType.Question, true);
    }

    #endregion

    #region Public Methods

    public void Initialize()
    {
        Services.Messenger.RegisterAll(this);

        foreach (var page in ViewModel.Pages)
            page.Saved += Page_Saved;
    }

    public void Deinitialize()
    {
        Services.Messenger.UnregisterAll(this);

        foreach (var page in ViewModel.Pages)
            page.Saved -= Page_Saved;
    }

    public override void Dispose()
    {
        base.Dispose();
        ViewModel.Dispose();
    }

    #endregion

    #region Message Receivers

    void IRecipient<RemovedGamesMessage>.Receive(RemovedGamesMessage message)
    {
        if (message.GameInstallations.Contains(ViewModel.GameInstallation))
        {
            ForceClose = true;
            WindowInstance.Close();
        }
    }

    #endregion

    #region Event Handlers

    private void Page_Saved(object sender, EventArgs e)
    {
        if (Services.Data.App_CloseConfigOnSave)
            WindowInstance.Close();
    }

    private async void PagesTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => 
        await ViewModel.LoadCurrentPageAsync();

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    #endregion
}