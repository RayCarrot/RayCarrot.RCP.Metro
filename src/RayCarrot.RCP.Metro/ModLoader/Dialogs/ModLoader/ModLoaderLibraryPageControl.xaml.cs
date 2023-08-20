using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

/// <summary>
/// Interaction logic for ModLoaderLibraryPageControl.xaml
/// </summary>
public partial class ModLoaderLibraryPageControl : UserControl
{
    public ModLoaderLibraryPageControl()
    {
        InitializeComponent();
    }

    private ModLoaderDialog ModLoaderDialog => this.FindParent<ModLoaderDialog>() ??
                                           throw new Exception($"The library control must be a child of {nameof(ModLoaderDialog)}");

    private ModLoaderViewModel ViewModel => DataContext as ModLoaderViewModel ??
                                          throw new Exception($"The data context must be of type {nameof(ModLoaderViewModel)}");

    private void ModsGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r.VisualHit.GetType() != typeof(ListBoxItem))
        {
            ViewModel.SelectedInstalledMod = null;
        }
    }

    private void ModsListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        var listBox = (ListBox)sender;
        var dropHandler = (ModDropHandler)DragDrop.GetDropHandler(listBox);
        dropHandler.ViewModel = ViewModel;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        ModLoaderDialog.Close();
    }

    private async void ApplyButton_OnClick(object sender, RoutedEventArgs e)
    {
        bool success = await ViewModel.ApplyAsync();

        if (!success)
            ModLoaderDialog.ForceClose();
    }
}