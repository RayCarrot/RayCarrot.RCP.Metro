using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// Interaction logic for PatcherUI.xaml
/// </summary>
public partial class PatcherUI : WindowContentControl
{
    #region Constructor
    
    public PatcherUI(PatcherViewModel viewModel)
    {
        DataContext = viewModel;
        ViewModel = viewModel;

        Loaded += PatcherUI_OnLoaded;

        // Set up UI
        InitializeComponent();
    }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    public PatcherViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Patcher"; // TODO-UPDATE: Localize
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

        // Cancel the closing if it's loading
        return !ViewModel.LoadOperation.IsLoading;
    }

    #endregion

    #region Event Handlers

    private async void PatcherUI_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PatcherUI_OnLoaded;

        await ViewModel.LoadPatchesAsync();
    }

    private void PatchesGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r.VisualHit.GetType() != typeof(ListBoxItem))
            ViewModel.SelectedPatch = null;
    }

    private void PatchesListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        var dropHandler = (PatchDropHandler)DragDrop.GetDropHandler(listBox);
        dropHandler.ViewModel = ViewModel;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    private async void ApplyButton_OnClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.ApplyAsync();

        // Close the dialog
        WindowInstance.Close();
    }

    #endregion
}