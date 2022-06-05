using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for Mod_R1_UI.xaml
/// </summary>
public partial class Mod_R1_UI : UserControl
{
    public Mod_R1_UI()
    {
        InitializeComponent();
    }

    private void UserControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        ((FrameworkElement)sender).Focus();
    }
}