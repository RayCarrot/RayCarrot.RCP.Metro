using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for Mod_Mem_UI.xaml
/// </summary>
public partial class Mod_Mem_UI : UserControl
{
    public Mod_Mem_UI()
    {
        InitializeComponent();
    }

    private void UserControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        ((FrameworkElement)sender).Focus();
    }
}