using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for PlayButtonControl.xaml
/// </summary>
public partial class PlayButtonControl : UserControl
{
    public PlayButtonControl()
    {
        InitializeComponent();
    }

    private void DropDownButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Toggle the state of the popup
        DropDownPopup.IsOpen ^= true;
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        DropDownPopup.IsOpen = false;
    }
}