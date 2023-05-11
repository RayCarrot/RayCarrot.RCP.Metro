#nullable disable
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for AnniversaryUpdateDialog.xaml
/// </summary>
public partial class AnniversaryUpdateDialog : BaseWindow
{
    public AnniversaryUpdateDialog()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}