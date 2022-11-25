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
            // Capture the mouse on this so only the button and popup can be interacted with
            Mouse.Capture(ParentElement, CaptureMode.SubTree);

            // Check for mouse clicks outside of this control
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(ParentElement, OutsideCapturedElementHandler);
        }

        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.Deactivated += MainWindow_OnDeactivated;
    }

    private void AssociatedObject_OnClosed(object sender, EventArgs e)
    {
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