using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

/// <summary>
/// Interaction logic for ModLoaderDownloadPageControl.xaml
/// </summary>
public partial class ModLoaderDownloadPageControl : UserControl
{
    public ModLoaderDownloadPageControl()
    {
        InitializeComponent();
    }

    public DownloadableModsViewModel ViewModel => (DownloadableModsViewModel)DataContext;

    private void ModsGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r.VisualHit.GetType() != typeof(ListBoxItem))
        {
            ViewModel.SelectedMod = null;
        }
    }
}