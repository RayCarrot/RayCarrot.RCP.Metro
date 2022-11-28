using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlzEx;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;

namespace RayCarrot.RCP.Metro;

public class DefaultPopupBehavior : Behavior<PopupEx>
{
    private ScrollViewer? _parentScrollViewer;

    public FrameworkElement? ParentElement
    {
        get => (FrameworkElement?)GetValue(ParentElementProperty);
        set => SetValue(ParentElementProperty, value);
    }

    public static readonly DependencyProperty ParentElementProperty = 
        DependencyProperty.Register(nameof(ParentElement), typeof(FrameworkElement), typeof(DefaultPopupBehavior));

    protected override void OnAttached()
    {
        AssociatedObject.Focusable = false;
        AssociatedObject.StaysOpen = true;
        AssociatedObject.AllowsTransparency = true;

        AssociatedObject.Loaded += AssociatedObject_OnLoaded;
        AssociatedObject.Opened += AssociatedObject_OnOpened;
        AssociatedObject.Closed += AssociatedObject_OnClosed;
    }

    protected override void OnDetaching()
    {
        if (_parentScrollViewer != null)
            _parentScrollViewer.ScrollChanged -= ParentScrollViewer_OnScrollChanged;

        _parentScrollViewer = null;

        AssociatedObject.Opened -= AssociatedObject_OnOpened;
        AssociatedObject.Closed -= AssociatedObject_OnClosed;
    }

    private void AssociatedObject_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Hacky fix for closing the popup when you scroll with the mouse scroll bar
        _parentScrollViewer = AssociatedObject.TryFindParent<ScrollViewer>();
        if (_parentScrollViewer != null)
            _parentScrollViewer.ScrollChanged += ParentScrollViewer_OnScrollChanged;

        AssociatedObject.Loaded -= AssociatedObject_OnLoaded;
    }

    private void AssociatedObject_OnOpened(object sender, EventArgs e)
    {
        if (ParentElement != null)
        {
            // Capture the mouse on this so only the parent element (usually the button) and popup can be interacted with
            Mouse.Capture(ParentElement, CaptureMode.SubTree);

            // A bit of a hack. If StaysOpen is false it will close the popup if you press outside of it, which we want.
            // Just using it however won't allow the parent element to be interacted with since only the popup is captured.
            // So what we do is first capture it and then set this. It won't work by itself though, so we still need to add
            // some other checks below... But this will fix an issue where if you click with your mouse inside of the popup
            // then the capture will be lost and it can't be closed (parent capture will still be lost - but that's fine).
            //
            // We might want to get rid of this hack some day. Simplest solution is just to always have StaysOpen set to
            // false and remove the other code here. Reason I didn't do this is because it doesn't look as good when the
            // button you clicked to open the popup can't be interacted with after it's opened.
            // We can also try always keeping StaysOpen to true and keep the other code in here, but then clicking inside
            // the popup (like on a RadioButton) will break the closing functionality.
            AssociatedObject.StaysOpen = false;

            // Check for mouse clicks outside of this control
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(ParentElement, OutsideCapturedElementHandler);
        }

        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.Deactivated += MainWindow_OnDeactivated;
    }

    private void AssociatedObject_OnClosed(object sender, EventArgs e)
    {
        AssociatedObject.StaysOpen = true;

        // Unsubscribe and release the mouse capture
        if (ParentElement != null)
        {
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(ParentElement, OutsideCapturedElementHandler);
            ParentElement.ReleaseMouseCapture();
        }

        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.Deactivated -= MainWindow_OnDeactivated;
    }

    private void ParentScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        AssociatedObject.IsOpen = false;
    }

    private void MainWindow_OnDeactivated(object sender, EventArgs e)
    {
        AssociatedObject.IsOpen = false;
    }

    private void OutsideCapturedElementHandler(object sender, MouseButtonEventArgs e)
    {
        AssociatedObject.IsOpen = false;
    }
}