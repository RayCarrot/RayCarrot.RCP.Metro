using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

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

    private void PlayButtonControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Hacky fix for closing the popup when you scroll with the mouse scroll bar
        ScrollViewer? parentScrollViewer = this.TryFindParent<ScrollViewer>();
        if (parentScrollViewer != null)
            parentScrollViewer.ScrollChanged += (_, _) => DropDownPopup.IsOpen = false;

        // Unsubscribe as the loaded event will trigger each time this tab is selected
        Loaded -= PlayButtonControl_OnLoaded;
    }

    private void DropDownButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Toggle the state of the popup
        DropDownPopup.IsOpen ^= true;
    }

    private void DropDownPopup_OnOpened(object sender, EventArgs e)
    {
        // Capture the mouse on this so only the button and popup can be interacted with
        Mouse.Capture(this, CaptureMode.SubTree);

        // Check for mouse clicks outside of this control
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OutsideCapturedElementHandler);

        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.Deactivated += MainWindow_OnDeactivated;
    }

    private void DropDownPopup_OnClosed(object sender, EventArgs e)
    {
        // Release the mouse capture
        ReleaseMouseCapture();

        // Unsubscribe
        Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OutsideCapturedElementHandler);
        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.Deactivated -= MainWindow_OnDeactivated;
    }

    private void MainWindow_OnDeactivated(object sender, EventArgs e)
    {
        DropDownPopup.IsOpen = false;
    }

    private void OutsideCapturedElementHandler(object sender, MouseButtonEventArgs e)
    {
        DropDownPopup.IsOpen = false;
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        DropDownPopup.IsOpen = false;
    }
}