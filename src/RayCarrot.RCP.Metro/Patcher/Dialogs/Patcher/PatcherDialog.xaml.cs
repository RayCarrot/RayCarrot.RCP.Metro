using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace RayCarrot.RCP.Metro.Patcher;

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

        WindowInstance.Title = $"Patcher - {ViewModel.Game.GetGameInfo().DisplayName}"; // TODO-UPDATE: Localize
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
        if (ViewModel.LoadOperation.IsLoading)
            return false;

        // Ask user if there are pending changes
        if (ViewModel.HasChanges)
            // TODO-UPDATE: Localize and use in other places
            return await Services.MessageUI.DisplayMessageAsync("There are unsaved changed. Do you want to continue and discard them?",
                "Confirm discarding changed", MessageType.Question, true);
        else
            return true;
    }

    #endregion

    #region Event Handlers

    private async void PatcherUI_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PatcherUI_OnLoaded;

        // Initialize
        bool success = await ViewModel.InitializeAsync();

        if (!success)
            WindowInstance.Close();
    }

    private void FileTableHeadersGrid_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Hacky workaround for having the headers be aligned with the table items. Without this
        // the scroll bar thumb will offset it incorrectly.
        FileTableItems.ApplyTemplate();
        ItemsPresenter itemsPresenter = (ItemsPresenter)FileTableItems.Template.FindName("Presenter", FileTableItems);

        itemsPresenter.SizeChanged -= FileTableItemsPresenter_OnSizeChanged;
        itemsPresenter.SizeChanged += FileTableItemsPresenter_OnSizeChanged;

        FileTableHeadersGrid.Width = itemsPresenter.ActualWidth;
    }

    private void FileTableItemsPresenter_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ItemsPresenter control = (ItemsPresenter)sender;
        FileTableHeadersGrid.Width = control.ActualWidth;
    }

    private void PatchesGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r.VisualHit.GetType() != typeof(ListBoxItem))
        {
            ViewModel.SelectedLocalPatch = null;
            ViewModel.SelectedExternalPatch = null;
        }
    }

    private void PatchesListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        var dropHandler = (PatchDropHandler)DragDrop.GetDropHandler(listBox);
        dropHandler.ViewModel = ViewModel;
    }

    private void PatchesListBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Redirect the mouse wheel movement to allow scrolling
        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = e.Source
        };

        PatchesScrollViewer?.RaiseEvent(eventArg);
        e.Handled = true;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    private async void ApplyButton_OnClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.ApplyAsync();

        // Close the dialog
        _forceClose = true;
        WindowInstance.Close();
    }

    #endregion
}