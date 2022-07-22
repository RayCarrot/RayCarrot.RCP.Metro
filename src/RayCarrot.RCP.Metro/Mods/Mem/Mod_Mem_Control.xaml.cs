using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for Mod_Mem_Control.xaml
/// </summary>
public partial class Mod_Mem_Control : UserControl
{
    public Mod_Mem_Control()
    {
        InitializeComponent();
    }

    private void UserControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        ((FrameworkElement)sender).Focus();
    }
}