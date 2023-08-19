using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace RayCarrot.RCP.Metro.Patcher.Dialogs.Patcher;

/// <summary>
/// Interaction logic for PatcherLibraryControl.xaml
/// </summary>
public partial class PatcherLibraryPageControl : UserControl
{
    public PatcherLibraryPageControl()
    {
        InitializeComponent();
    }

    private PatcherDialog PatcherDialog => this.FindParent<PatcherDialog>() ??
                                           throw new Exception($"The library control must be a child of {nameof(PatcherDialog)}");

    private PatcherViewModel ViewModel => DataContext as PatcherViewModel ??
                                          throw new Exception($"The data context must be of type {nameof(PatcherViewModel)}");

    private void PatchesGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r.VisualHit.GetType() != typeof(ListBoxItem))
        {
            ViewModel.SelectedInstalledPatch = null;
        }
    }

    private void PatchesListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        var listBox = (ListBox)sender;
        var dropHandler = (PatchDropHandler)DragDrop.GetDropHandler(listBox);
        dropHandler.ViewModel = ViewModel;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        PatcherDialog.Close();
    }

    private async void ApplyButton_OnClick(object sender, RoutedEventArgs e)
    {
        bool success = await ViewModel.ApplyAsync();

        if (!success)
            PatcherDialog.ForceClose();
    }
}