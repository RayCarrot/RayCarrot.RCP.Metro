#nullable disable
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameOptionsDialog.xaml
/// </summary>
public partial class GameOptionsDialog : WindowContentControl, IRecipient<RemovedGamesMessage>
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
        ViewModel = new GameOptionsDialog_ViewModel(gameInstallation);
        DataContext = ViewModel;

        // Subscribe to events
        Loaded += GameOptions_OnLoadedAsync;
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
    public GameOptionsDialog_ViewModel ViewModel { get; set; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = ViewModel.DisplayName;
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

    public void Receive(RemovedGamesMessage message)
    {
        if (message.GameInstallations.Contains(ViewModel.GameInstallation))
        {
            ForceClose = true;
            WindowInstance.Close();
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        // We don't have to unregister since it's a weak reference, but it's better to do so anyway when we can
        Services.Messenger.UnregisterAll(this);

        foreach (var page in ViewModel.Pages)
            page.Saved -= Page_Saved;

        ViewModel?.Dispose();
    }

    #endregion

    #region Event Handlers

    private void GameOptions_OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        Loaded -= GameOptions_OnLoadedAsync;

        Services.Messenger.RegisterAll(this);

        foreach (var page in ViewModel.Pages)
            page.Saved += Page_Saved;
    }

    private void Page_Saved(object sender, EventArgs e)
    {
        if (Services.Data.App_CloseConfigOnSave)
            WindowInstance.Close();
    }

    private async void PagesTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => await ViewModel.LoadCurrentPageAsync();

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    #endregion
}